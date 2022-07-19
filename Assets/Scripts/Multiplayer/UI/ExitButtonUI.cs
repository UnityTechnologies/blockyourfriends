﻿using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// When the main menu's Exit button is selected, send a quit signal.
    /// </summary>
    public class ExitButtonUI : MonoBehaviour
    {
        public void OnExitButton()
        {
            Application.Quit();
        }
    }
}
