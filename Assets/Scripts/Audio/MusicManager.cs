using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioMixer audioMixer;
    public AudioSource audioSource;
    public string volumeParam = "MyExposedParam";
    public float defaultVolume = 0.7f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;

     
        if (!audioSource.isPlaying)
            audioSource.Play();

        float volume = PlayerPrefs.GetFloat(volumeParam, defaultVolume);
        SetVolume(volume);
    }

    public void SetVolume(float linear)
    {
        float dB = (linear <= 0.0001f) ? -80f : 20f * Mathf.Log10(linear);
        audioMixer.SetFloat(volumeParam, dB);
        PlayerPrefs.SetFloat(volumeParam, linear);
    }

    public float GetVolume()
    {
        if (audioMixer.GetFloat(volumeParam, out float dB))
        {
            if (dB <= -79f) return 0f;
            return Mathf.Pow(10f, dB / 20f);
        }
        return 1f;
    }
}
