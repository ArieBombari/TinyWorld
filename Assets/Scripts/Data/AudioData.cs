using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettings", menuName = "Game/Audio/Audio Settings")]
public class AudioData : ScriptableObject
{
    [Header("Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    
    [Header("Ambient Sounds")]
    public AudioClip oceanWaves;
    [Range(0f, 1f)] public float oceanVolume = 0.3f;
    
    public AudioClip waterfallSound;
    [Range(0f, 1f)] public float waterfallVolume = 0.4f;
    
    public AudioClip birdsChirping;
    [Range(0f, 1f)] public float birdsVolume = 0.2f;
    
    public AudioClip windSound;
    [Range(0f, 1f)] public float windVolume = 0.15f;
    
    [Header("SFX")]
    public AudioClip footstepSound;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.4f;
    
    public AudioClip landSound;
    [Range(0f, 1f)] public float landVolume = 0.3f;
    
    public AudioClip featherCollectSound;
    [Range(0f, 1f)] public float featherCollectVolume = 0.6f;
    
    public AudioClip climbSound;
    [Range(0f, 1f)] public float climbVolume = 0.3f;
    
    public AudioClip glideSound;
    [Range(0f, 1f)] public float glideVolume = 0.25f;
}