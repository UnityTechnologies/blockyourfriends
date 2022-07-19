using UnityEngine;
using UnityEngine.UI;

namespace BlockYourFriends.UI
{
    public class SaveLoadSliderValue : MonoBehaviour
    {
        [SerializeField] private string key;
        [SerializeField] private float defaultValue;

        private Slider slider;

        public void LoadSliderValue()
        {
            slider = gameObject.GetComponent<Slider>();

            if (slider != null)
            {
                slider.value = PlayerPrefs.GetFloat(key, defaultValue);
            }
        }

        public void SaveSliderValue()
        {
            if (slider != null)
            {
                PlayerPrefs.SetFloat(key, slider.value);
            }
        }
    }
}