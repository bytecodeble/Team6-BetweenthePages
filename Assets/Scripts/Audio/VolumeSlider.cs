using UnityEngine;
using UnityEngine.UI;

namespace Game.Audio
{
    public class VolumeSliderUI : MonoBehaviour
    {
        public Slider slider;

        void Start()
        {
            if (slider == null) slider = GetComponent<Slider>();


            if (MusicManager.instance != null)
            {
                float current = MusicManager.instance.GetVolume();
                slider.value = current;
            }


            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        void OnSliderChanged(float value)
        {
            Debug.Log("Volume slider changed: " + value);
            if (MusicManager.instance != null)
            {
                MusicManager.instance.SetVolume(value);
            }
        }

        void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }

}