using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockYourFriends;
using BlockYourFriends.Multiplayer;
using System;

namespace BlockYourFriends.Gameplay
{
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager _instance;
        public static PlayerManager Instance { get { return _instance; } }

        [SerializeField] private GameObject Camera;
        [SerializeField] private GameObject Background;
        [SerializeField] private Color[] playerColors;
        [SerializeField] private Player[] players;
        private Player currentPlayer;

        void Awake()
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

        public void ChangePlayerName(string name)
        {
            // Store changed name of the player for use it later in next sessions
            string playerID = Locator.Get.Identity.GetSubIdentity(Multiplayer.Auth.IIdentityType.Auth).GetContent("id");
            PlayerPrefs.SetString(playerID, name);
            GameManager.Instance.singlePlayerName = name;

            Locator.Get.Messenger.OnReceiveMessage(MessageType.RenameRequest, name);
            UIManager.PlayerNameChanged(name);
        }

        public void SetupView(PaddleLocation paddleLocation)
        {
            int angle = 0;
            Vector3 position = new Vector3(0, 2, 0);

            if (paddleLocation == PaddleLocation.top)
            {
                angle = 180;
                position = new Vector3(0, -2, 0);
            }
            else if (paddleLocation == PaddleLocation.right)
            {
                angle = 90;
                position = new Vector3(-2, 0, 0);

            }
            else if (paddleLocation == PaddleLocation.left)
            {
                angle = 270;
                position = new Vector3(2, 0, 0);
            }

            Camera.transform.eulerAngles = new Vector3(0, 0, angle);
            Camera.transform.position = position;
            Background.transform.eulerAngles = new Vector3(0, 0, angle);
        }

        public void ResetView()
        {
            Camera.transform.eulerAngles = Vector3.zero;
            Camera.transform.position = new Vector3(0, 2, 0);
            Background.transform.eulerAngles = Vector3.zero;
        }

        public Color GetPlayerColor(PaddleLocation paddleLocation)
        {
            //Debug.Log("color");

            Color color = Color.white;

            if ((int)paddleLocation < playerColors.Length)
                color = playerColors[(int)paddleLocation];

            //Debug.Log("Color: " + (Color32)color);

            return color;
        }

        public string GetPlayerID(PaddleLocation paddleLocation)
        {
            return GetPlayer(paddleLocation).playerID;
        }

        public Player GetPlayer(PaddleLocation paddleLocation)
        {
            foreach (Player player in players)
            {
                if (player.paddlePos == paddleLocation)
                {
                    return player;
                }
            }

            return null;
        }

        public void GetPlayerInfo()
        {
            players = FindObjectsOfType<Player>();
            //Debug.Log("Players: " + players.Length);
            //foreach (Player player in players)
            //{
            //    Debug.Log($"kristytest: Name: {player.GetPlayerName()}, " +
            //        $"Color: {player.GetPlayerColor()}, " +
            //        $"GameObject: {player.gameObject.name}");
            //}
        }

        public void ResetPlayerInfo()
        {
            players = Array.Empty<Player>();
        }

        public Player GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public void SetCurrentPlayer(Player player)
        {
            currentPlayer = player;
        }
    }
}
