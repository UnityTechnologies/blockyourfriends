using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility.SO;
using BlockYourFriends;
using Unity.Netcode;

namespace BlockYourFriends.Gameplay
{

    //This script is attached to each brick object in the scene, it tracks HP and handles damage and destruction of the objects as well as spawning power ups if allocated
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public class Brick : NetworkBehaviour
    {
        [Header("Settings/Parameters")]
        [SerializeField] private int hitPoints = 1; //how many hits a block takes to break
        [SerializeField] private float destructionTime = 1f;
        [SerializeField] private SFXClip brickImpact;
        [SerializeField] private bool hasPowerUp = false;
        [Tooltip("Offset for non unit bricks")]
        [SerializeField] private Vector2 offset = new Vector2(0f, 0f);

        [Header("References")]
        [Tooltip("Visual FX for block breaking")]
        [SerializeField] private ParticleSystem blockBreakFX;
        [Tooltip("Parent object for particles")]
        [SerializeField] private GameObject particleParent;
        [SerializeField] private string fxParentName = "FXParent";
        [Tooltip("Power Up List containing power ups possible for this brick type")]
        public PowerUpList powerUpList;
        private BrickSetManager brickSetManager;
        [Tooltip("Animation to play for breaking brick")]
        private Animator myAnimator;
        private Collider2D myCollider;

        private void OnEnable()
        {
            transform.Translate(offset); //move the brick if there is an offset (ex: 2x2 bricks)
        }

        //retrieve required components and assign references

        void Start()
        {
            myCollider = GetComponent<Collider2D>();
            myAnimator = GetComponent<Animator>();
            brickSetManager = FindObjectOfType<BrickSetManager>();
            particleParent = GameObject.Find(fxParentName);
        }

        public void SetPowerUp()
        {
            hasPowerUp = true;
        }

        public void ApplyDamageToBrick(int damageReceived)
        {
            if (damageReceived >= hitPoints)
            {
                myCollider.enabled = false;
                brickSetManager.ReduceBrickCount();
                //short animation of brick breaking, note should not use this for bricks with more HP if displaying multiple states
                if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                {
                    RunBreakingAnimation();
                    PlaySoundFX();
                }
                else
                {
                    if (IsHost)
                    {
                        RunBreakingAnimation();
                        PlaySoundFX();
                        RunBreakingAnimation_ClientRpc();
                        PlaySoundFX_ClientRpc();
                    }
                }

                StartCoroutine(DestroyBrick());
                if (hasPowerUp)
                {
                    SpawnPowerUp();
                }
            }
            else
            {
                hitPoints -= damageReceived;
            }
        }

        public void SpawnPowerUp()
        {
            int randomItem = Random.Range(0, powerUpList.powerUpObjects.Length); //get a random power up from the list
            GameObject powerUpGO = Instantiate(powerUpList.powerUpObjects[randomItem].powerUpBonus, transform.position, Quaternion.identity);
            if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                powerUpGO.GetComponent<NetworkObject>().Spawn();
        }

        private IEnumerator DestroyBrick()
        {
            if (blockBreakFX)
            {
                if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                {
                    RunBreakingFX();
                }
                else
                {
                    if (IsHost)
                        RunBreakingFX();

                    RunBreakingFX_ClientRpc();
                }
            }

            yield return new WaitForSeconds(destructionTime);
            
            Destroy(this.gameObject);
        }

        #region NGO

        [ClientRpc]
        private void RunBreakingAnimation_ClientRpc()
        {
            if (!IsHost)
                RunBreakingAnimation();
        }

        private void RunBreakingAnimation()
        {
            myAnimator.SetBool("breaking", true);
        }

        [ClientRpc]
        private void RunBreakingFX_ClientRpc()
        {
            if (!IsHost)
                RunBreakingFX();
        }

        private void RunBreakingFX()
        {
            var fxSettings = blockBreakFX.main;
            fxSettings.startColor = GetComponent<SpriteRenderer>().color;
            Instantiate(blockBreakFX, transform.position, Quaternion.identity, particleParent.transform);
        }

        [ClientRpc]
        private void PlaySoundFX_ClientRpc()
        {
            if (!IsHost)
                PlaySoundFX();
        }

        private void PlaySoundFX()
        {
            //Debug.Log($"{name}: {brickImpact.clipName}");
            SFXManager.Instance.PlaySFX(brickImpact);
            //float volume = SFXManager.Instance.GetSFXVolume();
            //AudioSource.PlayClipAtPoint(brickImpact.sfxClip, transform.position, brickImpact.defaultVolume * volume);
        }

        #endregion
    }
}
