using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// After all players ready up for the game, this will show the countdown that occurs.
    /// This countdown is purely visual, to give clients a moment if they need to un-ready before entering the game; 
    /// clients will actually wait for a message from the host confirming that they are in the game, instead of assuming the game is ready to go when the countdown ends.
    /// </summary>
    public class CountdownUI : ObserverBehaviour<Countdown.Data>
    {
        [SerializeField]
        TMP_Text countDownText;

        protected override void UpdateObserver(Countdown.Data data)
        {
            base.UpdateObserver(data);
            if (observed.TimeLeft <= 0)
                countDownText.SetText("waiting for all players\nto get ready...");
            else
                countDownText.SetText($"starting in: {observed.TimeLeft:0}"); // Note that the ":0" formatting rounds, not truncates.
        }
    }
}