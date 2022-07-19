using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Gameplay
{
    public class FXHandler : MonoBehaviour
    {
        //A routine to place on instantiated FX Prefabs to auto destruct after a set time
        //Self-destruct script
        [Header("Settings/Parameters")]
        public float destructionTime = 1f; //self destruct timer for fx
                                           // Start is called before the first frame update
        void OnEnable()
        {
            StartCoroutine(DestructionCountDown());
        }

        private IEnumerator DestructionCountDown()
        {
            yield return new WaitForSeconds(destructionTime);
            Destroy(this.gameObject);
        }

    }
}
