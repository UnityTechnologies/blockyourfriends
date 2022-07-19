using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    public class Explosion : MonoBehaviour
    {
        //This class handles an individual explosion effect (sfx and vfx) on ball collision or appropriate power up deetonation
        //It is generally called from the Explosion Manager class

        [Header("References")]
        [Tooltip("Sprite for the explosion effect")]
        [SerializeField] private GameObject explosionImage;

        [Header("Settings/Parameters")]
        [Tooltip("Start Size vs sprite size of explosion")]
        [SerializeField] private float startSize = 2f;
        [Tooltip("End size of the explosion image, should match the radius of the effect from explosion manager")]
        [SerializeField] private float endSize = 4f;
        [Tooltip("Time in seconds to expand fully")]
        [SerializeField] private float timeToExpand = 1f;
        [Tooltip("Time to decay from start")]
        [SerializeField] private float timeToDecay = 1.5f;

        //local variable to track state and stop after timetoexpand.
        bool expanding = true;

        private void OnEnable()
        {
            explosionImage.transform.localScale = new Vector3(startSize, startSize, startSize);
            StartCoroutine(PlayAndDestroy());

        }

        private IEnumerator PlayAndDestroy()
        {
            //manage the expasion and auto destruction of the explosion ring
            yield return new WaitForSeconds(timeToExpand);
            expanding = false;
            yield return new WaitForSeconds(timeToDecay - timeToExpand);
            Destroy(gameObject);

        }

        private void Expand()
        {
            float newScale = explosionImage.transform.localScale.x + (Time.deltaTime / timeToExpand) * endSize / startSize;
            explosionImage.transform.localScale = new Vector3(newScale, newScale, newScale);


        }

        private void Update()
        {
            if (expanding)
            {
                Expand();
            }
        }

    }
}
