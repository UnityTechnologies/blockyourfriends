using System;
using System.Collections;
using System.Collections.Generic;
using BlockYourFriends.Gameplay;
using UnityEngine;

namespace BlockYourFriends.Utility.SO
{
    //Scriptable Object containing a list of power ups, allows for different types or collections of power ups

    [CreateAssetMenu(fileName = "Power Up List", menuName = "ScriptableObjects/PowerUps/PowerUp List", order = 1)]
    public class PowerUpList : ScriptableObject
    {
        [Serializable]
        public struct PowerUpObject
        {
            [Tooltip("Power Up name from enum type to search for")]
            public PowerUpType powerUpType;
            [Tooltip("Power Up bonus object to spawn from brick")]
            public GameObject powerUpBonus;
            [Tooltip("Power Up ball that object to instantiate at paddle")]
            public GameObject powerUpBall;
        }

        public PowerUpObject[] powerUpObjects;
    }
}
