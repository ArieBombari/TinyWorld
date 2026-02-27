using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Data")]
    [SerializeField] private AudioData audioData;
    
    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource ambientSource1;
    private AudioSource ambientSource2;
    private AudioSource ambientSource3;
    private AudioSource sfxSource;
    
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        SetupAudioSources();
    }
    
    void SetupAudioSources()
    {
        // Create audio source components
        musicSource = gameObject.AddComponent<AudioSource>();
        ambientSource1 = gameObject.AddComponent<AudioSource>();
        ambientSource2 = gameObject.AddComponent<AudioSource>();
        ambientSource3 = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        // Configure music source
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        
        // Configure ambient sources
        ambientSource1.loop = true;
        ambientSource1.playOnAwake = false;
        ambientSource2.loop = true;
        ambientSource2.playOnAwake = false;
        ambientSource3.loop = true;
        ambientSource3.playOnAwake = false;
        
        // Configure SFX source
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }
    
    void Start()
    {
    Debug.Log("AudioManager Start() called!");
    
    if (audioData == null)
    {
        Debug.LogError("AudioManager: No AudioData assigned!");
        return;
    }
    
    Debug.Log("AudioManager: AudioData found, playing sounds...");
    PlayAmbientSounds();
    PlayMusic();
    Debug.Log("AudioManager: Sounds should be playing now!");
    }
    
    void PlayMusic()
    {
    Debug.Log("PlayMusic() called");
    
    if (audioData.backgroundMusic != null)
    {
        Debug.Log("Music clip found: " + audioData.backgroundMusic.name);
        Debug.Log("Music volume: " + audioData.musicVolume);
        
        musicSource.clip = audioData.backgroundMusic;
        musicSource.volume = audioData.musicVolume;
        musicSource.Play();
        
        Debug.Log("musicSource.isPlaying: " + musicSource.isPlaying);
    }
    else
    {
        Debug.LogWarning("AudioManager: No background music assigned!");
    }
    }
    
    void PlayAmbientSounds()
    {
    Debug.Log("PlayAmbientSounds() called");
    
    // Ocean waves
    if (audioData.oceanWaves != null)
    {
        Debug.Log("Ocean clip found: " + audioData.oceanWaves.name);
        Debug.Log("Ocean volume: " + audioData.oceanVolume);
        
        ambientSource1.clip = audioData.oceanWaves;
        ambientSource1.volume = audioData.oceanVolume;
        ambientSource1.Play();
        
        Debug.Log("Ocean isPlaying: " + ambientSource1.isPlaying);
    }
    else
    {
        Debug.Log("No ocean waves assigned");
    }
    
    // Birds chirping
    if (audioData.birdsChirping != null)
    {
        Debug.Log("Birds clip found: " + audioData.birdsChirping.name);
        Debug.Log("Birds volume: " + audioData.birdsVolume);
        
        ambientSource2.clip = audioData.birdsChirping;
        ambientSource2.volume = audioData.birdsVolume;
        ambientSource2.Play();
        
        Debug.Log("Birds isPlaying: " + ambientSource2.isPlaying);
    }
    else
    {
        Debug.Log("No birds assigned");
    }
    
    // Wind sound
    if (audioData.windSound != null)
    {
        Debug.Log("Wind clip found: " + audioData.windSound.name);
        Debug.Log("Wind volume: " + audioData.windVolume);
        
        ambientSource3.clip = audioData.windSound;
        ambientSource3.volume = audioData.windVolume;
        ambientSource3.Play();
        
        Debug.Log("Wind isPlaying: " + ambientSource3.isPlaying);
    }
    else
    {
        Debug.Log("No wind assigned");
    }
    }
    
    // Call these from other scripts
    public void PlayFootstep()
    {
        PlaySFX(audioData.footstepSound, audioData.footstepVolume);
    }
    
    public void PlayJump()
    {
        PlaySFX(audioData.jumpSound, audioData.jumpVolume);
    }
    
    public void PlayLand()
    {
        PlaySFX(audioData.landSound, audioData.landVolume);
    }
    
    public void PlayFeatherCollect()
    {
        PlaySFX(audioData.featherCollectSound, audioData.featherCollectVolume);
    }
    
    public void PlayClimb()
    {
        PlaySFX(audioData.climbSound, audioData.climbVolume);
    }
    
    public void PlayGlide()
    {
        PlaySFX(audioData.glideSound, audioData.glideVolume);
    }
    
    void PlaySFX(AudioClip clip, float volume)
    {
    if (clip != null)
    {
        Debug.Log("Playing SFX: " + clip.name + " at volume: " + volume);
        sfxSource.PlayOneShot(clip, volume);
    }
    else
    {
        Debug.LogWarning("Tried to play SFX but clip was null!");
    }
    }
    
    // Volume controls
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientSource1.volume = volume * audioData.oceanVolume;
        ambientSource2.volume = volume * audioData.birdsVolume;
        ambientSource3.volume = volume * audioData.windVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}