using BlockYourFriends.Gameplay;
using Unity.Netcode;

namespace BlockYourFriends.Multiplayer.ngo
{
    public class NetworkBrickSetParent : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                BrickSetManager brickSetManager = FindObjectOfType<BrickSetManager>();
                if (brickSetManager != null)
                {
                    brickSetManager.layoutParentFromClient = transform;
                }
            }
        }
    }
}