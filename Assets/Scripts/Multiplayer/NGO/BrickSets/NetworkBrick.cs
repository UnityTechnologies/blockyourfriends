using BlockYourFriends.Gameplay;
using Unity.Netcode;

namespace BlockYourFriends.Multiplayer.ngo
{
    public class NetworkBrick : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                BrickSetManager brickSetManager = FindObjectOfType<BrickSetManager>();
                if (brickSetManager != null)
                {
                    while (brickSetManager.layoutParentFromClient == null)
                    {
                        // Wait for layout parent
                    }

                    transform.SetParent(brickSetManager.layoutParentFromClient);
                }
            }
        }
    }
}
