using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGamePlayerHUD : MonoBehaviour
{
    [Header("Player UI settings")]
    [SerializeField] private string playerDefaultName;
    [SerializeField] private Color playerColor;

    [Header("Components")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private Image playerColorImage;

    private void Start()
    {
        UpdatePlayerColor();
        ResetPlayerName();
    }

    private void UpdatePlayerColor()
    {
        playerColorImage.color = playerColor;
    }

    public void UpdatePlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void ResetPlayerName()
    {
        playerNameText.text = playerDefaultName;
    }

    public void SetPlayerScore(int score)
    {
        playerScoreText.text = score.ToString();
    }
}
