using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Utility.SO
{
    //Scriptable Object Containing SFX clip information including default playback volume and clip.

    [CreateAssetMenu(fileName = "SFX Clip", menuName = "ScriptableObjects/Audio/SFX Clip", order = 2)]
    public class SFXClip : ScriptableObject
    {
        [Tooltip("Default volume to play the clip")]
        public float defaultVolume = 1.0f;

        [Tooltip("Default pitch of the clip")]
        public float defaultPitch = 1.0f;

        [Tooltip("The clip in question to play")]
        public AudioClip sfxClip;
    }
}
