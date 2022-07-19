using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    //BoombBall power up script, inherits from ball and overrides the collision effect (to explode on impact)
    public class BombBall : Ball
    {
        public override void ProcessBrickCollision(Collision2D collision)
        {
            Player ballOwner = GetComponent<BallScore>().GetLastPlayer();
            FindObjectOfType<ExplosionManager>().GenerateExplosion(collision.contacts[0].point, ballOwner);
            Destroy(gameObject); //ball is not tracked so we don't call existing methods for cleanup
        }

        public override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == boundaryTag)
            {
                Destroy(gameObject); //don't add an extra ball to the pool avialable :)
            }
        }
    }
}
