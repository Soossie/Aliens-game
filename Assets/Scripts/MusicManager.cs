using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;
    private AudioSource audioSource;

    private void Awake()
    {
        if (!_instance)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SetMusicVolume(PlayerPrefs.GetFloat("music_volume"));
    }

    public static void PlayMusic(AudioClip musicClip)
    {
        _instance.audioSource.clip = musicClip;
        _instance.audioSource.loop = true;
        _instance.audioSource.volume = GameManager.MusicVolume;
        _instance.audioSource.Play();
    }
    
    public static void PauseMusic()
    {
        _instance.audioSource.Pause();
    }

    public static void ResumeMusic()
    {
        _instance.audioSource.Play();
    }
    
    public static void StopMusic()
    {
        _instance.audioSource.Stop();
    }
    
    public static void SetMusicVolume(float volume)
    {
        _instance.audioSource.volume = volume;
    }
}
