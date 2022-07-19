﻿using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    [SerializeField]
    public class LobbyUserAudio
    {
        public string ID { get; private set; }
        public bool HasVoice { get; set; }
        public bool Muted { get; set; }

        // We should explicitly ensure that UserVolume is a normalized value, as letting the volume be set too high could be harmful to listeners.
        private float userVolume;
        public float UserVolume
        {
            get => userVolume;
            set => userVolume = Mathf.Clamp01(value);
        }

        public LobbyUserAudio(string userID)
        {
            ID = userID;
            HasVoice = false;
            Muted = false;
            UserVolume = 50/70f; // Begin at what will be neutral volume given the range of min to max volume.
        }
    }
}
