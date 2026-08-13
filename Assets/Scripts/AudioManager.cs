using UnityEngine;
using System;

public enum SoundType
{
    UIClickIn,
    UIClickOut,
    UINewGame,
    UIPaused,
    UIUnpaused,
    SelectLemming,
    SpawnLemming,
    LemmingDie,
    ReachGoal,
    ReachScore,
    LevelWin,
    LevelFail,
    RoleUnlock,
    GameWin,
    Quit
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static AudioManager _instance;
    private AudioSource audioSource;
    

    void Awake()
    {
        if (!Application.isPlaying) return;
        
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
    
    private void Start()
    { 
        if (!Application.isPlaying) return;
        audioSource = GetComponent<AudioSource>();
        _instance.audioSource.volume = PlayerPrefs.GetFloat("sfx_volume");
    }
    
    public static void PlaySound(SoundType sound, Vector3 sourcePosition = default)
    {
        AudioClip[] clips = _instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        
        if (sourcePosition != default)
            AudioSource.PlayClipAtPoint(randomClip, sourcePosition, GameManager.SfxVolume);
        else
            _instance.audioSource.PlayOneShot(randomClip, GameManager.SfxVolume);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
            soundList[i].name = names[i];
    } 
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds => sounds;
    public string name;
    [SerializeField] private AudioClip[] sounds;
}
