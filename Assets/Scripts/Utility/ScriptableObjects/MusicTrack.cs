using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Utility.SO
{
    //Scriptable Object to contain the information for a music track: Default Volume for playback, clip

    [CreateAssetMenu(fileName = "Music Track", menuName = "ScriptableObjects/Audio/Music Track", order = 1)]
    public class MusicTrack : ScriptableObject
    {
        [Tooltip("Default volume for this track")]
        [Range(0f, 1f)]
        public float defaultVolume = 1.0f;

        [Tooltip("Default pitch for this track")]
        [Range(-3f, 3f)]
        public float defaultPitch = 1.0f;

        [Tooltip("The track in question to play")]
        public AudioClip musicTrack;
    }
}
