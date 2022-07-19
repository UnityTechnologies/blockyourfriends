using System;
using UnityEngine;
using UnityEngine.Events;

namespace BlockYourFriends.Multiplayer.UI
{
    public enum JoinCreateTabs
    {
        Join,
        Create
    }

    /// <summary>
    /// The panel that holds the lobby joining and creation panels.
    /// </summary>
    public class JoinCreateLobbyUI : ObserverPanel<LocalGameState>
    {
        public UnityEvent<JoinCreateTabs> onTabChanged;

        [SerializeField] //Serialized for Visisbility in Editor
        JoinCreateTabs currentTab = JoinCreateTabs.Join;

        public JoinCreateTabs CurrentTab
        {
            get => currentTab;
            set
            {
                currentTab = value;
                onTabChanged?.Invoke(currentTab);
            }
        }

        public void SetJoinTab()
        {
            CurrentTab = JoinCreateTabs.Join;
        }

        public void SetCreateTab()
        {
            CurrentTab = JoinCreateTabs.Create;
        }

        public override void ObservedUpdated(LocalGameState observed)
        {
            if (observed.State == GameState.JoinMenu)
            {
                onTabChanged?.Invoke(currentTab);
                Show(false);
            }
            else
            {
                Hide();
            }
        }
    }
}
