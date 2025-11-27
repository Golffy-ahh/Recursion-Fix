using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioService : MonoBehaviour, IAudioService
{
    [Header("SFX (Effect)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Music / BGM")]
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (!sfxSource)
            sfxSource = GetComponent<AudioSource>();
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (!clip || !sfxSource) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (!clip || !musicSource) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }
}
