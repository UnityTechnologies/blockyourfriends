using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using BlockYourFriends.Utility.SO;

namespace BlockYourFriends.Gameplay
{
    //Player Paddle Controller
    //This class handles the player input for the player paddle (movement on the appropriate axis and ball launching)

    public class PaddleControl : NetworkBehaviour
    {
        [Header("Positional/movement settings")]
        public float rightSideLimit = 9f; //x-axis constraint for player movement
        public float leftSideLimit = -9f;
        public float topSideLimit = 7.5f; //y-axis constraint for player movement
        public float bottomSideLimit = -7.5f;
        public float movementRate = 10f; //speed that the paddle can move per second
        public Vector2 startingPosition = new Vector2(0, -9f);

        [Header("Ball Related settings")]

        public Vector3 ballSpawnDistance = new Vector3(0, 2, 0); //where to spawn the ball relative to the paddle
        public Vector2 startingVelocity = new Vector2(0, -5); //starting velocity for the ball
        public float forceMultiplier = 1f; //multiplier for directional force to add on collision
        public float minTimeBetweenBalls = .5f; //time between ball launches (Min)

        private PowerUpType specialBall; //type of powered up ball loaded if there is one
        private bool specialLoaded = false;

        private Vector2 unitForceToAdd = new Vector2(1, 0);

        [Header("Object References")]
        public GameObject ballsInPlayTracker; //just a game object to parent the balls in play to
        public GameObject ballLaunchObject = null;
        public Ball playerBall; //object to spawn as a ball
        public Player player; //ref to local player
        [Tooltip("The Scriptable Object containing the power up information (prefab and name)")]
        public PowerUpList powerUpList;
        private PaddlesManager paddlesManager;

        private enum paddleMovement { none, left, right };

        private paddleMovement paddleMove;

        private bool inputFrozen;

        public int ballsAvailable = 0;

        private void Awake()
        {
            player = GetComponent<Player>();
            paddlesManager = transform.parent.GetComponent<PaddlesManager>();
        }

        private void Start ()
        {
            ballSpawnDistance = BallManager.Instance.GetStartingPosition(player.paddlePos);
            startingVelocity = BallManager.Instance.GetStartingVelocity(player.paddlePos);
        }

        private void EnableBallLauncher()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
            {
                DoEnableBallLauncher();
            }
            else if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
            {
                paddlesManager.EnableBallLauncher_ServerRpc((int)player.paddlePos);
            }
         }

        public void DoEnableBallLauncher()
        {
            ballLaunchObject.gameObject.SetActive(true);
        }

        public void DisableBallLauncher()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
            {
                DoDisableBallLauncher();
            }
            else if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
            {
                paddlesManager.DisableBallLauncher_ServerRpc((int)player.paddlePos);
            }
        }

        public void DoDisableBallLauncher()
        {
            ballLaunchObject.gameObject.SetActive(false);
        }

        public void SetBallsAvailable(int newBalls)
        {
            ballsAvailable = newBalls;
            if (ballsAvailable <= 0)
            {
                DisableBallLauncher();
            }
            else
            {
                EnableBallLauncher();
            }
        }

        public void AddBallAvailable()
        {
            ballsAvailable++;
            EnableBallLauncher();
        }

        private void RemoveBallAvailable()
        {
            ballsAvailable--;
            if (ballsAvailable <= 0)
            {
                DisableBallLauncher();
            }
        }

        public void FreezePlayerInput()
        {
            inputFrozen = true;
        }

        public void UnFreezePlayerInput()
        {
            inputFrozen = false;
        }

        public void ResetPlayerPosition()
        {
            if ((GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost) ||
                GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                transform.position = startingPosition;
        }

        public void SetPowerUp(PowerUpType powerUpType)
        {
            specialBall = powerUpType;
            specialLoaded = true;
            EnableBallLauncher();
        }

        private void LaunchSpecialBall()
        {
            GameObject ballToSpawn;
            for (int i = 0; i<powerUpList.powerUpObjects.Length; i++)
            {
                if (powerUpList.powerUpObjects[i].powerUpType == specialBall)
                {
                    ballToSpawn = Instantiate(powerUpList.powerUpObjects[i].powerUpBall, transform.position +ballSpawnDistance, Quaternion.identity, ballsInPlayTracker.transform);
                    if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer)
                        ballToSpawn.GetComponent<NetworkObject>().Spawn();
                    ballToSpawn.GetComponent<Ball>().AddVelocity(startingVelocity);
                    ballToSpawn.GetComponent<BallScore>().SetInitialPlayer(player);
                }
            }

            if (ballsAvailable <= 0)
            {
                DisableBallLauncher();
            }

            specialLoaded = false;
        }

        private void BallSpawn()
        {
            if ((GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost) ||
                GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
            {
                DoBallSpawn();
            }
            else
            {
                paddlesManager.BallSpawn_ServerRpc((int)player.paddlePos);
            }
        }

        public void DoBallSpawn() // spawn a ball if you don't have one in play
        {
            if ((ballsAvailable > 0) || specialLoaded)
            {
                if (specialLoaded)
                {
                    LaunchSpecialBall();
                }
                else
                {
                    Ball newBall = Instantiate(playerBall, transform.position + ballSpawnDistance, Quaternion.identity, ballsInPlayTracker.transform);
				    if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                    	newBall.GetComponent<NetworkObject>().Spawn();
                    newBall.AddVelocity(startingVelocity);
                    newBall.GetComponent<BallScore>().SetInitialPlayer(player);
                    BallManager.Instance.BallSpawned();
                    GameManager.Instance.ActivateAIPaddles();
                    RemoveBallAvailable();
                }
            }

            if (Application.isMobilePlatform)
            {
                if (ballsAvailable<=0)
                {
                    DoDisableBallLauncher(); //deactivate the Mobile UI to launch a ball
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "ball")
            {
                Vector2 forceToAdd = unitForceToAdd;
                if (paddleMove == paddleMovement.right)
                {
                    forceToAdd = forceMultiplier * forceToAdd;
                }
                else if (paddleMove == paddleMovement.left)
                {
                    forceToAdd = -forceMultiplier * forceToAdd;
                }
                else
                {
                    forceToAdd = 0 * forceToAdd;
                }
                collision.gameObject.GetComponent<Ball>().AddVelocity(forceToAdd);
            }
        }

        private void MoveLeft()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer ||
               (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost))
            {
                DoMoveLeft();
            }
            else
            {
                paddlesManager.MoveLeft_ServerRpc((int)player.paddlePos);
            }
        }

        public void DoMoveLeft()
        {
            //keyboard move left
            float newXPos = transform.position.x;
            float newYPos = transform.position.y;

            if (player.paddlePos == PaddleLocation.bottom)
                newXPos -= Time.deltaTime * movementRate;
            else if (player.paddlePos == PaddleLocation.top)
                newXPos += Time.deltaTime * movementRate;
            else if (player.paddlePos == PaddleLocation.right)
                newYPos -= Time.deltaTime * movementRate;
            else if (player.paddlePos == PaddleLocation.left)
                newYPos += Time.deltaTime * movementRate;

            gameObject.transform.position = new Vector3(newXPos, newYPos, transform.position.z);
        }

        private void MoveRight()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer ||
               (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost))
            {
                DoMoveRight();
            }
            else
            {
                paddlesManager.MoveRight_ServerRpc((int)player.paddlePos);
            }
        }

        public void DoMoveRight()
        {
            //keyboard move right
            float newXPos = transform.position.x;
            float newYPos = transform.position.y;

            if (player.paddlePos == PaddleLocation.bottom)
                newXPos += Time.deltaTime * movementRate;
            else if (player.paddlePos == PaddleLocation.top)
                newXPos -= Time.deltaTime * movementRate;
            else if (player.paddlePos == PaddleLocation.right)
                newYPos += Time.deltaTime * movementRate;
            else if (player.paddlePos == PaddleLocation.left)
                newYPos -= Time.deltaTime * movementRate;

            gameObject.transform.position = new Vector3(newXPos, newYPos, transform.position.z);
        }

        private void MovePaddle(Vector2 moveTarget) //mobile paddle movement to clamped position on the x-axis (movement range) at the speed towards the point clicked
        {
            moveTarget = new Vector2(moveTarget.x * paddlesManager.screenSizeDif.x, moveTarget.y * paddlesManager.screenSizeDif.y);
            if ((GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost) ||
                GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                DoMovePaddle(moveTarget);
            else
                paddlesManager.MovePaddle_ServerRpc((int)player.paddlePos, moveTarget);
        }

        public void DoMovePaddle(Vector2 moveTarget)
        {
            Vector2 clampedTarget = Vector2.zero;

            if (player.paddlePos == PaddleLocation.bottom || player.paddlePos == PaddleLocation.top)
                clampedTarget = new Vector2(Mathf.Clamp(moveTarget.x, leftSideLimit, rightSideLimit), transform.position.y);
            else if (player.paddlePos == PaddleLocation.right || player.paddlePos == PaddleLocation.left)
                clampedTarget = new Vector2(transform.position.x, Mathf.Clamp(moveTarget.y, bottomSideLimit, topSideLimit));

            transform.position = Vector2.MoveTowards(transform.position, clampedTarget, movementRate * Time.deltaTime);
        }

        private bool IsLeftLimitReached()
        {
            bool isReached = true;

            if (player.paddlePos == PaddleLocation.bottom)
                isReached = transform.position.x <= leftSideLimit;
            else if (player.paddlePos == PaddleLocation.top)
                isReached = transform.position.x >= rightSideLimit;
            else if (player.paddlePos == PaddleLocation.right)
                isReached = transform.position.y <= bottomSideLimit;
            else if (player.paddlePos == PaddleLocation.left)
                isReached = transform.position.y >= topSideLimit;

            return isReached;
        }

        private bool IsRightLimitReached()
        {
            bool isReached = true;

            if (player.paddlePos == PaddleLocation.bottom)
                isReached = transform.position.x >= rightSideLimit;
            else if (player.paddlePos == PaddleLocation.top)
                isReached = transform.position.x <= leftSideLimit;
            else if (player.paddlePos == PaddleLocation.right)
                isReached = transform.position.y >= topSideLimit;
            else if (player.paddlePos == PaddleLocation.left)
                isReached = transform.position.y <= bottomSideLimit;

            return isReached;
        }

        void Update()
        {
            if (!inputFrozen && player.isCurrentPlayer)
            {
                if (Application.isEditor)
                {
                    if (Input.GetKey(KeyCode.LeftArrow) && !IsLeftLimitReached())
                    {
                        MoveLeft();
                        paddleMove = paddleMovement.left;
                    }
                    else if (Input.GetKey(KeyCode.RightArrow) && !IsRightLimitReached())
                    {
                        MoveRight();
                        paddleMove = paddleMovement.right;
                    }
                    else
                    {
                        paddleMove = paddleMovement.none;
                    }

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        BallSpawn();
                    }
                }

                else if (Application.isMobilePlatform)
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0); //use the first touch
                        Vector3 touchedPos = Camera.main.ScreenToWorldPoint(touch.position);
                        MovePaddle(touchedPos);

                        RaycastHit2D hit = Physics2D.Raycast(touchedPos, Vector2.zero);
                        {
                            if (hit)
                            {
                                if (hit.collider != null)
                                {
                                    if (hit.collider.tag == "BallLaunch")
                                    {
                                        BallSpawn();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
