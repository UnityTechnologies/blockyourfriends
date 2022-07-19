using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility.SO;
using Unity.Netcode;

namespace BlockYourFriends.Gameplay
{
    //This class controls the behaviour of individual balls and special balls (through inheritance)
    //Collision and movement are managed here

    [RequireComponent(typeof(Rigidbody2D))]
    public class Ball : NetworkBehaviour
    {
        [Header("References")]
        //asigned automatically
        private Rigidbody2D myRigidBody;
        private GameObject fxParent;
        private Player ballOwner;
        [SerializeField] private string fxParentName = "FXParent";
        [Tooltip("Visual FX for ball destruction")]
        [SerializeField] private ParticleSystem destructionFX;
        [Tooltip("Sound FX for paddle ball bounce")]
        [SerializeField] private SFXClip paddleBounce;


        [Header("Ball Properties")]
        [SerializeField] private int damageToBricks = 1; //damage to bricks (can be higher for other kinds of balls?).
        [SerializeField] private float destructionTimer = .5f;
        [SerializeField] public string boundaryTag = "boundary";
        [SerializeField] private float reboundVelocity = 15f;
        private bool beingDestroyed = false;
        private int paddleCollisionCount = 0; //track paddle collisions consecutive to avoid infinite loops
        [Tooltip("Min and Max range to randomize the rebound")]
        [Range(.25f, .5f)] public float minReboundUnit; //rebound angles represented in units on the Unit circle away from origin.
        [Range(.5f, .75f)] public float maxReboundUnit;

        bool isHostBall = true;


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isHostBall = IsHost;
        }

        //set initial referneces
        void OnEnable()
        {
            myRigidBody = GetComponent<Rigidbody2D>();
            fxParent = GameObject.Find(fxParentName);
            if (!fxParent)
            {
                Instantiate(new GameObject(fxParentName));
                fxParent = fxParent = GameObject.Find(fxParentName);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isHostBall)
                return;

            if (collision.gameObject.tag == "brick")
            {
                ProcessBrickCollision(collision); // On Brick collision (ovverride for other ball types (eg: bomb)
            }
            if (collision.gameObject.tag == "ball") // When two balls collide
            {
                RedirectBall(collision);
                ballOwner = GetComponent<BallScore>().GetLastPlayer();
                FindObjectOfType<ExplosionManager>().BallExplosion(transform.localPosition, ballOwner);
            }
            else if (collision.gameObject.tag == "Player") //Tracking consecutive hits to ensure the ball is not in an endless loop
            {
                if (paddleBounce)
                {
                    PlaySoundFX();
                    //SFXManager.Instance.PlaySFX(paddleBounce);
                    //float volume = SFXManager.Instance.GetSFXVolume();
                    //AudioSource.PlayClipAtPoint(paddleBounce.sfxClip, transform.position, paddleBounce.defaultVolume * volume);
                }
                paddleCollisionCount++;
                if (paddleCollisionCount > 2)
                {
                    RedirectBall(collision);
                }
            }
            else paddleCollisionCount = 0;
        }

        private void PlaySoundFX()
        {
            SFXManager.Instance.PlaySFX(paddleBounce);
            //float volume = SFXManager.Instance.GetSFXVolume();
            //AudioSource.PlayClipAtPoint(brickImpact.sfxClip, transform.position, brickImpact.defaultVolume * volume);
        }

        public virtual void ProcessBrickCollision(Collision2D collision)
        {
            collision.gameObject.GetComponent<Brick>().ApplyDamageToBrick(damageToBricks); //default behaviour regular ball
        }

        //RedirectBall will change the direciton of the ball after a number of consecutive paddle hits
        private void RedirectBall(Collision2D collision)
        {
            Vector2 newDirection = new Vector2(collision.otherCollider.transform.position.x - collision.collider.transform.position.x,
                                                            collision.otherCollider.transform.position.y - collision.collider.transform.position.y);
            Vector2 unitNewDirection = newDirection.normalized;

            //Change the direction of the rebound if it is along or the x or y axis directly (to avoid infinite bounces)
            if (Mathf.Abs(unitNewDirection.x) < .25)
            {
                unitNewDirection.x = Random.Range(minReboundUnit, maxReboundUnit);
                unitNewDirection = unitNewDirection.normalized;
            }
            else if (Mathf.Abs(unitNewDirection.y) < .25)
            {
                unitNewDirection.y = Random.Range(minReboundUnit, maxReboundUnit);
                unitNewDirection = unitNewDirection.normalized;
            }
            myRigidBody.velocity = unitNewDirection * reboundVelocity;
        }

        public void AddVelocity(Vector2 velocityToAdd)
        {
            if (myRigidBody != null)
            {
                myRigidBody.velocity += velocityToAdd;
            }
            else
            {
                Debug.Log("No rigidbody?");
            }
        }

        public virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isHostBall)
                return;

            if (collision.gameObject.tag == boundaryTag)
            {
                DestroyBall(false);
            }
        }

        public void DestroyBall(bool instant)
        {
            if (!instant)
            {
                if (!beingDestroyed) //check if it is in the process of being destroyed (takes .5 seconds due to the timer and fx)... avoids the double destruction from passing through 2 colliders.
                {
                    beingDestroyed = true;
                    if (destructionFX)
                    {
                        var fxSettings = destructionFX.main;
                        fxSettings.startColor = GetComponent<Renderer>().material.color;
                        Instantiate(destructionFX, transform.position, Quaternion.identity, fxParent.transform);
                    }
                    StartCoroutine(SelfDestruct());
                    BallManager.Instance.RemoveBallInPlay();
                }
            }
            else
            {
                if ((GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost) ||
                    GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                {
                    BallManager.Instance.RemoveBall();
                    Destroy(gameObject);
                }
            }
        }

        private IEnumerator SelfDestruct()
        {
            yield return new WaitForSecondsRealtime(destructionTimer);
            Destroy(gameObject);
        }
    }
}
