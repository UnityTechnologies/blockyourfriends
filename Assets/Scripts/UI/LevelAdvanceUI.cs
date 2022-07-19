using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BlockYourFriends.Utility;

namespace BlockYourFriends.UI
{
    public class LevelAdvanceUI : Singleton<LevelAdvanceUI>
    {
        //UI Class for handling level change UI > Display level name and Unity Logo

        [Header("Settings/Parameters")]
        [Tooltip("Canvas reference for the level countdown")]
        public GameObject countDownCanvas;
        [Tooltip("Text Object for level time left")]
        public TextMeshProUGUI levelTimeText;
        [Tooltip("Canvas reference for the level name caanvas")]
        public GameObject levelAdvanceTextCanvas;
        [Tooltip("Text Object for Level Name")]
        public TextMeshProUGUI levelName;

        void Start()
        {
            countDownCanvas.SetActive(false);
            levelAdvanceTextCanvas.SetActive(false);
        }

        public void SetLevelRemainingTime(int timer)
        {
            //for now just active and set the time remaining, will make this fancier with polish :)
            countDownCanvas.SetActive(true);
            levelTimeText.text = timer.ToString();
        }

        public void SetLevelName(string nextLevelName)
        {
            countDownCanvas.SetActive(false);
            levelAdvanceTextCanvas.SetActive(true);
            levelName.text = nextLevelName;
        }

        public void DisableTransitionUI()
        {
            countDownCanvas.SetActive(false);
            levelAdvanceTextCanvas.SetActive(false);
        }
    }
}
