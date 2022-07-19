using Unity.Netcode;
using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    public class BallScore : NetworkBehaviour
    {
        Player lastPlayerToTouchBall;
        bool boundaryIsHit = false;
        bool isHostBall = true;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isHostBall = IsHost;
        }

        public void SetInitialPlayer(Player spawningPlayer)
        {
            lastPlayerToTouchBall = spawningPlayer;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isHostBall)
                return;

            if (collision.gameObject.tag == "Player")
            {
                lastPlayerToTouchBall = collision.gameObject.GetComponent<Player>();
            }

            if (collision.gameObject.tag == "brick")
            {
                lastPlayerToTouchBall.AddScore(50);
            }
        }

        //return the ownever of this ball
        public Player GetLastPlayer()
        {
            return lastPlayerToTouchBall;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isHostBall)
                return;

            if (collision.gameObject.tag == "boundary")
            {
                if (!boundaryIsHit)
                {
                    boundaryIsHit = true; //Only first boundary hit is affected, second is ignored

                    Player playerHit = collision.gameObject.GetComponent<Boundary>().BoundaryPlayer();

                    if (playerHit != lastPlayerToTouchBall) //did not score on self
                    {
                        lastPlayerToTouchBall.AddScore(100);
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            lastPlayerToTouchBall = null;
        }
    }
}
