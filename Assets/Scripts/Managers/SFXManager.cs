using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility.SO;
using BlockYourFriends.Utility;

namespace BlockYourFriends
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXManager : Singleton<SFXManager>
    {
        //This class handles all SoundFX in the game as well as storing and setting the mster and SFX volume for player preferences
        //Note that it is meant to funciton with an Scriptable Object (SFXClip) that contains some data on the clip

        private AudioSource sfxMainSource;   //for non-positional interruptable sfx

        private const string sfxVolumeRef = "SFXVolume";
        private const string masterVolumeRef = "MasterVolume";

        private const float defaultSFXVolume = .5f;
        private const float defaultMasterVolume = 1f;

        private float masterVolume = 1f;
        private float sfxVolume = .5f;

        //NOTE: For one shot audio (non-interruptable, disposable one time sounds, ex brick breaking, use PlayOneShot directly
        //The attached method is intended for audio that should not overlap: eg any voice lines

        private void OnEnable()
        {
            GetAudioSource();
            masterVolume = PlayerPrefs.GetFloat(masterVolumeRef, defaultMasterVolume);
            sfxVolume = PlayerPrefs.GetFloat(sfxVolumeRef, defaultSFXVolume);
            SetMasterVolume(masterVolume);
            SetSFXVolume(sfxVolume);
        }

        public float GetMasterVolume()
        {
            return masterVolume;
        }

        public float GetSFXVolume()
        {
            return sfxVolume;
        }

        public void SetMasterVolume(float newMasterVolume)
        {
            masterVolume = newMasterVolume;
            PlayerPrefs.SetFloat(masterVolumeRef, newMasterVolume);
            if (MusicManager.Instance != null)
                MusicManager.Instance.AdjustMusicVolume();
        }

        public void SetSFXVolume(float newSFXVolume)
        {
            sfxVolume = newSFXVolume;
            PlayerPrefs.SetFloat(sfxVolumeRef, newSFXVolume);
            if (sfxMainSource != null)
                sfxMainSource.volume = newSFXVolume;
        }

        private void GetAudioSource()
        {
            sfxMainSource = GetComponent<AudioSource>();
        }

        public void PlaySFX(SFXClip clipToPlay)
        {
            sfxMainSource.Stop(); //stop any current sounds
            sfxMainSource.clip = clipToPlay.sfxClip;
            sfxMainSource.volume = clipToPlay.defaultVolume * sfxVolume * masterVolume; //audio level for the clip (to normalize audio clips), sfx volume control and master control levels
            sfxMainSource.pitch = clipToPlay.defaultPitch;
            sfxMainSource.Play();
        }
    }
}