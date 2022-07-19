using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    //Class to control the nice little graphic that spawns the ball.
    //NOTE This class has been deprecated currently as the effect to launch the ball
    public class BallLanchFX : MonoBehaviour
    {
        [Header("Settings/Parameters")]
        [Tooltip("rotation degrees per second Euler")]
        public float rotationRate = 90f;
        [Tooltip("Time to shrink then time to grow (double for full cycle)")]
        public float pulseTime = 2f;
        [Tooltip("Target shrink size (percentage) for the image")]
        public float pulseScale = .75f;

        private bool started = false;
        private bool shrinking = false;

        private void OnEnable()
        {
            ResetPositionAndSize();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            started = false;
            shrinking = false;
        }

        private void ResetPositionAndSize()
        {
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            gameObject.transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (enabled)
            {
                Pulsate();
            }
        }

        private IEnumerator Shrink()
        {
            shrinking = true;
            yield return new WaitForSeconds(pulseTime);
            shrinking = false;
            StartCoroutine(Grow());
        }

        private IEnumerator Grow()
        {
            shrinking = false;
            yield return new WaitForSeconds(pulseTime);
            shrinking = true;
            StartCoroutine(Shrink());
        }

        private void Pulsate()
        {
            gameObject.transform.Rotate(0, 0, rotationRate * Time.deltaTime);
            if (!started)
            {
                started = true;
                StartCoroutine(Shrink());
            }
            else if (shrinking)
            {
                float newScale = transform.localScale.x - (1 - pulseScale) * Time.deltaTime; // rate of shrinnking per frame
                gameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
            else //growing
            {
                float newScale = transform.localScale.x + (1 - pulseScale) * Time.deltaTime; // rate of growing per frame
                gameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
    }
}