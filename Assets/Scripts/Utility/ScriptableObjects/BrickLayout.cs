using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace BlockYourFriends.Utility.SO
{

    //This scriptable object contains the layout information for each brick layout/level
    //A game is composed of a random or sequential selection from a Layout list

    [CreateAssetMenu(fileName = "Layout", menuName = "ScriptableObjects/BrickLayout", order = 1)]
    public class BrickLayout : ScriptableObject
    {
        [Tooltip("Max Level time in seconds")]
        public float levelTime = 90f;

        public string layoutName;

        [Tooltip("Max number of power ups in the level")]
        public int maxPowerUps = 1;

        [Tooltip("odds of a brick containing a power up")]
        public float powerUpChance = .2f;

        public float layoutScale = .6f; //Scale of the layout, default is .6f currently

        [Tooltip("Starting Rotation in Euleur degrees clockwise of the brick assembly")]
        public float zRotation = 45f;

        [Tooltip("Should the assembly actively rotate")]
        public bool rotateAssembly = false;
        [Tooltip("Used if the assmebly rotates to determine degrees/second of rotation")]
        public float rotationRate = 10f;

        [Serializable]
        public struct brickRows
        {
            [Tooltip("5 placements per row, can be null for empty slots")]
            public NetworkObject[] rowBricks;
        }

        public brickRows[] brickLayout;

    }
}