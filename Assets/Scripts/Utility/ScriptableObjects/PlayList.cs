using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Utility.SO
{
    //Scriptable Object containing a playlist (list) of music tracks

    [CreateAssetMenu(fileName = "Playlist", menuName = "ScriptableObjects/Audio/Playlist", order = 2)]
    public class PlayList : ScriptableObject
    {
        [Tooltip("the tracks to play as part of this playlist")]
        public MusicTrack[] trackList;

        [Tooltip("Name to display for the track list")]
        public string playListName;
    }
}
