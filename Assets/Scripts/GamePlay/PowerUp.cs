using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BlockYourFriends.Utility.SO;
using Random = UnityEngine.Random;
using Unity.Netcode;

namespace BlockYourFriends.Gameplay
{
    public class PowerUp : NetworkBehaviour
    {
        [Header("Settings/Parameters")]
        public PowerUpType powerUpType = PowerUpType.Bomb;
        [Tooltip("Time to destroy in seconds if not picked up")]
        public float selfDestructTime = 5f;
        [Tooltip("Time remaning at which to start blinking")]
        public float blinkTimer = 1f;
        [Tooltip("The time of one blink pulse")]
        public float pulseTime = .25f;
        [Tooltip("Half width/height of play space in units")]
        public float halfWidth = 9f;
        [Tooltip("Move speed once claimed")]
        public float claimedSpeed = 10f;
        [Tooltip("Distance at which power up is claimed")]
        public float minClaimedDist = 1f;
        [Tooltip("Time between floating target set change")]
        public float floatTimeChange = 1f;
        [Tooltip("ratio of the bounding box the power ups can float in (max 1, min 0)")]
        [Range(0f, 1f)] public float floatDistanceRatio = .5f;

        [Tooltip("Countdown to destroy after pick up (time/seconds)")]
        public float selfDestructTimer = 1f;

        [Header("References")]
        [Tooltip("Visual FX to play when picked up")]
        public GameObject pickupVFX;
        private SpriteRenderer myRenderer;

        //Determined by distance to floating point an
        private float floatingSpeed;
        private float maxFloatBoundingBox = 7f;

        //Target location for power up to move to once claimed (based on paddle loc)
        public Vector2 targetLoc = new Vector2(0, 0);

        //who currently owns the power up (track where it's moving to and attribute power up)
        private Player owner = null;

        //track if the power up is blinking (end of life)
        private bool blinking = false;
        //Track if the power up is claimed or free floating
        public bool claimed = false;

        bool isHostPowerUp = true;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isHostPowerUp = IsHost;
        }

        private void OnEnable()
        {
            maxFloatBoundingBox = halfWidth * floatDistanceRatio;
            myRenderer = GetComponent<SpriteRenderer>();

            if (isHostPowerUp)
            {
                StartCoroutine(CountDownToDestruct());
                StartCoroutine(FloatTargetSet());
            }
        }

        private IEnumerator FloatTargetSet()
        {
            //This method sets the target location on a timed interval (floatTimeChange) for power ups that have not been claimed.
            SetFloatTarget();
            yield return new WaitForSeconds(floatTimeChange);
            StartCoroutine(FloatTargetSet()); //called recursively to set ever interval
        }

        private void SetFloatTarget()
        {
            float newXPos = Random.Range(-maxFloatBoundingBox, maxFloatBoundingBox);
            float newYPos = Random.Range(-maxFloatBoundingBox, maxFloatBoundingBox);
            targetLoc = new Vector2(newXPos, newYPos);
            floatingSpeed = Vector2.Distance(transform.position, targetLoc) / floatTimeChange;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isHostPowerUp)
                return;

            StopAllCoroutines(); //Stop destruction timer on pickup and floating
            if (collision.tag == "ball")
            {
                owner = collision.GetComponent<BallScore>().GetLastPlayer();
                myRenderer.color = owner.GetPlayerColor();
                //Debug.Log("claimed by: " + owner.name);
                if (owner.paddlePos == PaddleLocation.bottom)
                {
                    targetLoc = new Vector2(0, -halfWidth);
                }
                else if (owner.paddlePos == PaddleLocation.top)
                {
                    targetLoc = new Vector2(0, halfWidth);
                }
                else if (owner.paddlePos == PaddleLocation.left)
                {
                    targetLoc = new Vector2(- halfWidth, 0);
                }
                else //right side paddle loc
                {
                    targetLoc = new Vector2(halfWidth, 0);
                }
                claimed = true;
            }
        }

        private IEnumerator CountDownToDestruct()
        {
            yield return new WaitForSeconds(selfDestructTime - blinkTimer);
            blinking = true;
            yield return new WaitForSeconds(blinkTimer);
            Destroy(gameObject);
        }

        private void MoveToTarget()
        {
            if (Vector2.Distance(transform.position, targetLoc) <= minClaimedDist)
            {
                ApplyPowerUp();
                Destroy(gameObject); //clean up after playing fx and assigning power up.
            }
            transform.position = Vector2.MoveTowards(transform.position, targetLoc, claimedSpeed * Time.deltaTime);
        }

        private void ApplyPowerUp()
        {
            if (owner == null)
            {
                Debug.LogError("Power Up at player with no owner");
            }
            else if (owner.IsAI())
            {
                owner.gameObject.GetComponent<AIPaddleControl>().SetPowerUp(powerUpType);
                PlayPowerUpFx();
            }
            else //Not AI
            {
                owner.gameObject.GetComponent<PaddleControl>().SetPowerUp(powerUpType);
                PlayPowerUpFx();
            }
        }

        private void PlayPowerUpFx()
        {
            if (pickupVFX == null)
            {
                Debug.LogError("No VFX attached to power up");
            }
            else
            {
                GameObject vfxParent = GameObject.Find("FXParent");
                if (vfxParent == null)
                {
                    vfxParent = new GameObject("FXParent");
                }
                Instantiate(pickupVFX, transform.position, Quaternion.identity, vfxParent.transform);
                StartCoroutine(SelfDestruct());
            }
        }

        private IEnumerator SelfDestruct()
        {
            yield return new WaitForSeconds(selfDestructTime);
            Destroy(gameObject);
        }

        private void FloatAround()
        {
            transform.position = Vector2.MoveTowards(transform.position, targetLoc, floatingSpeed * Time.deltaTime);
        }


        private void Update()
        {
            if (!isHostPowerUp)
                return;

            if (!claimed)
            {
                FloatAround();
            }
            else //claimed
            {
                MoveToTarget();
            }
            if (blinking)
            {
                //blink (to be setup), as time runs down
            }
        }
    }
}
