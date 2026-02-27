using UnityEngine;

public class WaterfallAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip waterfallSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private float maxDistance = 30f;
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Create 3D audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = waterfallSound;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = volume;
        
        // 3D spatial settings
        audioSource.spatialBlend = 1f; // Full 3D
        audioSource.minDistance = 5f;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        
        audioSource.Play();
    }
}