using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Gun Sounds")]
    public AudioClip gunShotSound;
    [Range(0f, 1f)] public float gunShotVolume = 1f;

    public AudioClip reloadSound;
    [Range(0f, 1f)] public float reloadVolume = 1f;

    [Header("Master Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayGunShot()
    {
        PlaySound(gunShotSound, gunShotVolume);
    }

    public void PlayReload()
    {
        PlaySound(reloadSound, reloadVolume);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume * masterVolume);
    }
}
