using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility.SO;
using Unity.Netcode;

namespace BlockYourFriends.Gameplay
{
    //This class is the primary AI Controller for non-player characters in the game
    //Currently operates with a simple lookup for the closest ball refreshed every "ballchecktimer" seconds and moves the paddle towards that location
    //The AI Could be improved with proper heuristics to identify the most likely scoring situations, however in testing the performance is already well balanced
    //On level start the position and tracking is reset
    //Balls spawning add to a managed list for the paddle to maintain the closest moving ball

    public class AIPaddleControl : NetworkBehaviour
    {
        [Header("Constraints and Settings")]
        [Tooltip("Max Axis position in units on the right or up axis")]
        public float positiveAxisConstraint = 7.5f;
        [Tooltip("Max Axis position in units on the left or up down")]
        public float negativeAxisConstraing = -7.5f;
        [Tooltip("Paddle Movement max rate per second")]
        public float movementRate = 10f;
        [Tooltip("Paddle position on the grid relative to the player")]
        public PaddleLocation paddleLocation;
        [Tooltip("Max Distance for ball detection")]
        public float maxDistance = 100f; //set to a high number to start
        [Tooltip("Time between checks for closest ball")]
        public float ballCheckTimer = .5f;
        [Tooltip("Distance for the AI to prioritize ball return over launching a new ball")]
        public float dangerDistance = 4f;
        [Tooltip("AI distance from brick along constraints to fire")] //Should be less than the ball width, can add some complexity at some point to take rotation or even consider specific outcomes
        public float minDistToFire = .5f;

        //obtained via BallManager based on set distance from position
        private Vector2 ballSpawnDistance;

        //obtained via ball manager based on position and base velocity settings
        private Vector2 startingVelolcity;

        //hold details on the player for htis paddle
        private Player player;

        private Vector2 startingPosition; //starting position of the paddle, set in start, but used throughout
        private bool movementFrozen = false; //paddle motion is frozen (useful for transitions)
        private Vector2 velocityToAdd = new Vector2(0, 0); //Velocity to add on ball impact on paddle, set with movement, will generally be negligible
        private float halfPaddleWidth = 1.5f; //To use for offset ofs tarting position
        private bool ballAvailable = false; //whether or not the AI has a ball ready
        private bool specialLoaded = false; //whether or not the AI has a special ball ready
        private PowerUpType specialBall;
       
        [Header("References")]
        public Ball closestBall = null; //track the closest ball
        public Brick randomBrick = null; //track a random brick to fire at
        public Ball aiBall = null; //Ball Object to spawn (non-power up)
        public PowerUpList powerUpList; //the SO containing the power ups and their associated balls to spawn
        public GameObject ballsInPlayTracker;

        //Ball Tracking
        public int ballsAvailable = 0;
        private Vector3 targetLocation;
        private bool activeBallExists = false; //track if any balls are in play (to enable movement)

        void Start()
        {
            player = GetComponent<Player>();
            SetStartingPosition();
            ResetAIPosition();
            targetLocation = transform.position;
        }

        //The BallsAvailable functions set whether or not the AI paddle has a ball available to use, as assigned by the Ball Manager
        public void SetBallsAvailable(int newBalls)
        {
            ballsAvailable = newBalls;
        }

        public void AddBallAvailable()
        {
            ballsAvailable++;
            ballAvailable = true;
        }

        public void RemoveBallAvailable()
        {
            ballsAvailable--;
            if (ballsAvailable <= 0)
            {
                ballAvailable = false;
            }
        }

        //When a special power up is received, the appropriate type is set here so the AI paddle is aware it is available
        public void SetPowerUp(PowerUpType powerUpType)
        {
            specialBall = powerUpType;
            specialLoaded = true;
        }

        private void SetStartingPosition()
        {
            if (paddleLocation == PaddleLocation.left)
            {
                startingPosition = new Vector2(negativeAxisConstraing - halfPaddleWidth, 0);
            }
            else if (paddleLocation == PaddleLocation.top)
            {
                startingPosition = new Vector2(0, positiveAxisConstraint + halfPaddleWidth);
            }
            else if (paddleLocation == PaddleLocation.right)
            {
                startingPosition = new Vector2(positiveAxisConstraint + halfPaddleWidth, 0);
            }
            else
            {
                Debug.Log("Hrrr, not a valid AI position"); //AI can't be on the bottom as there is at least one player/host who will be there.
            }
            startingVelolcity = BallManager.Instance.GetStartingVelocity(paddleLocation);
            ballSpawnDistance = BallManager.Instance.GetStartingPosition(paddleLocation);
        }

        public void ResetAIPosition()
        {
            transform.position = startingPosition;
        }

        public void FreezeAIMovement()
        {
            movementFrozen = true;
            StopAllCoroutines(); //Stop any ball checks in progress :)
        }

        public void UnFreezeAIMovement()
        {
            movementFrozen = false;
        }

        public void IncreaseBallCount()
        {
            BallManager.Instance.ballsInPlayCount++;
            activeBallExists = true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "ball")
            {
                collision.gameObject.GetComponent<Ball>().AddVelocity(velocityToAdd);
            }
        }

        public void OnBallCountChanged()
        { 
            if (BallManager.Instance.ballsInPlayCount <= 0)
            {
                activeBallExists = false;
                closestBall = null;
            }
        }

        private void GetClosestBall() //return the closest ball
        {
            float closestDistance = maxDistance;
            Ball tempClosestBall = null;
            Ball[] ballsInPlay = FindObjectsOfType<Ball>();
            if (ballsInPlay == null)
            {
                Debug.Log("NO balls in play");
                return;
            }
            foreach (Ball ball in ballsInPlay)
            {
                float currentDistance = Vector3.Distance(transform.position, ball.transform.position);
                if (currentDistance <= closestDistance)
                {
                    closestDistance = currentDistance;
                    tempClosestBall = ball;
                }
            }
            closestBall = tempClosestBall;
        }

        private void MoveWithBall() //move the paddle towards the ball along the constraints
        {
            if (closestBall == null)
            {
                StartCoroutine(CheckClosestBall());
            }
            else
                MoveToLocation(closestBall.transform.position);
        }

        private void MoveToLocation(Vector2 targetLoc) //Generic movement function used both by movement with the ball and moving into laucnh position
        {
            if (paddleLocation == PaddleLocation.left)
            {
                targetLocation = new Vector2(transform.position.x, Mathf.Clamp(targetLoc.y, negativeAxisConstraing, positiveAxisConstraint));
                velocityToAdd = new Vector2(0f, transform.position.y - targetLocation.y);
            }
            else if (paddleLocation == PaddleLocation.top)
            {
                targetLocation = new Vector3(Mathf.Clamp(targetLoc.x, negativeAxisConstraing, positiveAxisConstraint), transform.position.y, 0);
                velocityToAdd = new Vector2(transform.position.x - targetLocation.x, 0f);
            }
            else if (paddleLocation == PaddleLocation.right)
            {
                targetLocation = new Vector3(transform.position.x, Mathf.Clamp(targetLoc.y, negativeAxisConstraing, positiveAxisConstraint), 0);
                velocityToAdd = new Vector2(0f, transform.position.y - targetLocation.y);
            }
            else
            {
                Debug.Log("AI attempting to move incorrectly defined paddle");
            }
            transform.position = Vector3.MoveTowards(transform.position, targetLocation, movementRate * Time.deltaTime);
        }

        private IEnumerator CheckClosestBall()
        {
            GetClosestBall();
            yield return new WaitForSeconds(ballCheckTimer);

            if (activeBallExists)
            {
                StartCoroutine(CheckClosestBall()); //recursive call to continue this if htere is an active ball
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }

        //AI Decision making, where the AI will determine if it should move defensively to deflect a ball (based on dangerDistance parameter) or move to fire
        private void AIThinkyThinky()
        {
            if (ballAvailable || specialLoaded)
            {
                if (activeBallExists)
                {
                    if (closestBall == null)
                    {
                        GetClosestBall();
                    }
                    else if (Vector2.Distance(closestBall.transform.position, transform.position) < dangerDistance)
                    {
                        MoveWithBall();
                        return; // keep moving with the ball if this is true
                    }
                }
                MoveToFire(); //move to fire the ball available

            }
            else if (activeBallExists)
            {
                MoveWithBall();
            }
        }

        private void MoveToFire() //Move the AI paddle into firing position
        {
            if (randomBrick == null)
            {
                randomBrick = FindObjectOfType<Brick>();
            }
            if (randomBrick != null)
            {
                MoveToLocation(randomBrick.transform.position);
            }
            
            if (Vector2.Distance(targetLocation, transform.position) <= minDistToFire)
            {
                if (specialLoaded)
                {
                    LaunchSpecialBall();
                }
                LaunchBall();
            }
        }

        private void LaunchSpecialBall() //If there is a special ball loaded the AI will launch it before using a regular ball (if one is available as well)
        {
            specialLoaded = false;
            GameObject ballToSpawn;
            for (int i = 0; i < powerUpList.powerUpObjects.Length; i++)
            {
                if (powerUpList.powerUpObjects[i].powerUpType == specialBall)
                {
                    ballToSpawn = Instantiate(powerUpList.powerUpObjects[i].powerUpBall, (Vector2) transform.position + ballSpawnDistance, Quaternion.identity, ballsInPlayTracker.transform);
                    if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                        ballToSpawn.GetComponent<NetworkObject>().Spawn();
                    ballToSpawn.GetComponent<Ball>().AddVelocity(startingVelolcity);
                    ballToSpawn.GetComponent<BallScore>().SetInitialPlayer(player);
                }
            }
        }

        private void LaunchBall()
        {
            if (ballAvailable)
            {
                {
                    Ball newBall = Instantiate(aiBall, (Vector2) transform.position + ballSpawnDistance, Quaternion.identity, ballsInPlayTracker.transform);
                    BallManager.Instance.BallSpawned();
                    newBall.AddVelocity(startingVelolcity);
                    newBall.GetComponent<BallScore>().SetInitialPlayer(player);
                    if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                        newBall.GetComponent<NetworkObject>().Spawn();
                    GameManager.Instance.ActivateAIPaddles();
                    ballsAvailable--;
                    if (ballsAvailable <=0)
                    {
                        ballAvailable = false;
                    }
                }
            }
        }

        void Update()
        {
            if (!movementFrozen)
            {
                AIThinkyThinky();
            }
        }
    }
}
