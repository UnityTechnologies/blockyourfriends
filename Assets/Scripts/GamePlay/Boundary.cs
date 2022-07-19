using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    public class Boundary : MonoBehaviour
    {
        [SerializeField] Player myPlayer;

        public Player BoundaryPlayer()
        {
            return myPlayer;
        }
    }
}
