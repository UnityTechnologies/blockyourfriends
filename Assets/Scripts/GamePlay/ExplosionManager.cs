using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    public class ExplosionManager : MonoBehaviour
    {
        //Class to generate explosions, detects two colliding balls (or in the future can call an explosion at a specific location, and creates a single detonation point
        //Can be called by ball collisions (two points) or by direct call for point explosions (from BombBall.cs for example on Bomb detonation)

        [Header("Settings/Parameters")]
        [Tooltip("Radius of the explosion circle, for collision purposes")]
        [SerializeField] private float explosionRadius = 2f;
        [Tooltip("Damage to bricks")]
        [SerializeField] private int explosionDamage = 1;
        [Tooltip("Layer mask to only check brick layer for collisions")]
        [SerializeField] public LayerMask layersToHit;
        [Tooltip("Points per brick value")]
        public int pointsPerBrick = 50;

        [Header("References")]
        [Tooltip("Prefab to instantiate for the ball collision")]
        [SerializeField] private GameObject ballExplosion; 

        private int explosivePointCount = 0;
        private Vector2[] loc = { Vector2.zero, Vector2.zero };
        private Player[] playerOwner = { null, null };

        private static ExplosionManager _instance;
        public static ExplosionManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        //reset the explosion "owners" for scoring purposes after the explosion on ball collision is generated
        public void ResetPlayers()
        {
            playerOwner[0] = null;
            playerOwner[1] = null;
        }

        //takes in collision data from balls (will be twice) and creates one explosion
        public void BallExplosion(Vector2 location, Player ballOwner)
        {
            if (explosivePointCount < 2)
            {
                loc[explosivePointCount] = location;
                playerOwner[explosivePointCount] = ballOwner;
                explosivePointCount++;
            }
            if (explosivePointCount >=2)
            {
                if (Vector2.Distance(loc[0], loc[1]) <= 2f) //Checking proximity before averaging to make sure the explosion data is for the same collision, should be unless we add many balls.
                {
                    explosivePointCount = 0;
                    Vector2 explosionPoint = new Vector2((loc[0].x + loc[1].x) / 2, (loc[1].y + loc[1].y) / 2);
                    SpawnExplosion(explosionPoint);
                    ApplyDamage(explosionPoint, true);
                }
            }
        }

        public void GenerateExplosion(Vector2 location, Player ballOwner) //Future use, to detonate power up
        {
            playerOwner[0] = ballOwner;
            SpawnExplosion(location);
            ApplyDamage(location, false);
        }

        private void ApplyDamage(Vector2 explosionPoint, bool multiPlayer)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPoint, explosionRadius, layersToHit);
            foreach (Collider2D hit in hits)
            {
                //Debug.Log("Hit on " + hit.gameObject.name);
                hit.gameObject.GetComponent<Brick>().ApplyDamageToBrick(explosionDamage);
                if (multiPlayer) //multipler players involved in explosion (balls)
                {
                    if (playerOwner[0]==null||playerOwner[1] == null)
                    {
                        Debug.LogError("missing player data for ball collision");
                    }
                    else
                    {
                        playerOwner[0].AddScore(pointsPerBrick);
                        playerOwner[1].AddScore(pointsPerBrick);
                    }

                }
                else //single explosion, eg bomb
                {
                    if (playerOwner[0] == null )
                    {
                        Debug.LogError("missing player data for ball collision");
                    }
                    else
                    {
                        playerOwner[0].AddScore(pointsPerBrick);
                    }
                }
            }
        }

        private void SpawnExplosion(Vector2 location)
        {
            Instantiate(ballExplosion, location, Quaternion.identity, transform);
        }
    }
}
