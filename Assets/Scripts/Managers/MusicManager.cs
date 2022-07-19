using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility.SO;
using BlockYourFriends.Utility;

namespace BlockYourFriends
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : Singleton<MusicManager>
    {
        //Put class on AudioSource @ Camera
        //The Music Manager handles the music playlist (Scriptable Object) and the music output for the game

        [Header("Default Music Player Settings")]
        public bool randomize = false;
        public bool repeat = true;

        List<int> playorder = new List<int>();

        [Header("PlayList settings")]
        public int playListToPlay = 0;
        public PlayList[] playLists;
        private AudioSource musicSource;
        [SerializeField] private float musicVolume = .5f;
        private float currentTrackVolume = .5f; //store the track setting for default volume as the track is updated
        private const float defaultMusicVolume = .5f;
        private const string musicVolumeRef = "MusicVolume"; //for player prefs

        private void OnEnable()
        {
            GetSource();
            musicVolume = PlayerPrefs.GetFloat(musicVolumeRef, defaultMusicVolume);
        }

        public float GetMusicVolume()
        {
            return musicVolume;
        }

        public void SetMusicVolume(float newMusicVolume)
        {
            PlayerPrefs.SetFloat(musicVolumeRef, newMusicVolume);
            musicVolume = newMusicVolume;
            musicSource.volume = newMusicVolume * currentTrackVolume * SFXManager.Instance.GetMasterVolume();
        }

        private void GetSource()
        {
            musicSource = GetComponent<AudioSource>();
            transform.position = Camera.main.transform.position; //move this object and the audio source attached to the camera loc for consistency
        }

        public void SetRandom(bool randomOn)
        {
            randomize = randomOn;
        }

        public void PlayTrack(MusicTrack trackToPlay)
        {
            if (musicSource == null)
            {
                GetSource();
            }

            musicSource.Stop();
            musicSource.clip = trackToPlay.musicTrack;
            musicSource.volume = trackToPlay.defaultVolume * musicVolume * SFXManager.Instance.GetMasterVolume();
            currentTrackVolume = trackToPlay.defaultVolume; //assign to global variable for use if the volume changes mid track
            musicSource.pitch = trackToPlay.defaultPitch;
            musicSource.Play();
        }

        public void AdjustMusicVolume()
        {
            if (musicSource != null)
                musicSource.volume = currentTrackVolume * musicVolume * SFXManager.Instance.GetMasterVolume();
        }

        public void PlayPlayList(string playListName)
        {
            if (musicSource == null)
            {
                GetSource();
            }
            StopAllCoroutines(); //stop any existing playlists playing
            playListToPlay = -1;
            for (int i = 0; i < playLists.Length; i++) //locate the playlist by name
            {
                if (playLists[i].playListName == playListName)
                {
                    playListToPlay = i;
                }
            }
            if (playListToPlay == -1)
            {
                Debug.LogError("Playlist " + playListName + " not found.");
                return;
            }
            SetOrder();
            StartCoroutine(PlayNext(playorder[0]));
        }

        public void PlayPlayList()
        {
            if (musicSource == null)
            {
                GetSource();
            }
            StopAllCoroutines(); //stop any existing playlists playing
            if (playListToPlay == -1)
            {
                Debug.LogError("No PlayList Set");
                return;
            }
            SetOrder();
            StartCoroutine(PlayNext(playorder[0]));
        }

        public void SetPlayList(string playListName)
        {
            playListToPlay = -1;
            for (int i = 0; i < playLists.Length; i++)
            {
                if (playLists[i].playListName == playListName)
                {
                    playListToPlay = i;
                }
            }
            if (playListToPlay == -1)
            {
                Debug.LogError("Playlist " + playListName + " not found.");
            }
        }


        private void SetOrder() //Set the track play order
        {
            playorder.Clear();
            for (int i = 0; i < playLists[playListToPlay].trackList.Length; i++)
            {
                playorder.Add(i);
            }

            if (randomize)
            {
                for (int i = 0; i < playorder.Count; i++)
                {
                    int temp = playorder[i];
                    int randomIndex = Random.Range(i, playorder.Count);
                    playorder[i] = playorder[randomIndex];
                    playorder[randomIndex] = temp;
                }
            }
        }

        private IEnumerator PlayNext(int nextUp)
        {
            PlayTrack(playLists[playListToPlay].trackList[playorder[nextUp]]);
            yield return new WaitForSeconds(playLists[playListToPlay].trackList[playorder[nextUp]].musicTrack.length);
            nextUp++;
            if (nextUp < playLists[playListToPlay].trackList.Length)
            {
                PlayNext(nextUp);
            }
            else if (repeat)
            {
                PlayNext(0); //restart the playlist
            }
            else
            {
                Debug.Log("End of the music, friends");
            }
        }

        public void StopPlayingMusic()
        {
            musicSource.Stop();
            StopAllCoroutines(); //stop existing playlists
        }
    }
}
