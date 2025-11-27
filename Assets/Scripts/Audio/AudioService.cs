using UnityEngine;

public class AudioService : MonoBehaviour, IAudioService
{
    [Header("OneShot / SFX")]
    [SerializeField] private AudioSource oneShotSource;

    [Header("Loop Template (BGM / Ambient)")]
    [SerializeField] private AudioSource loopTemplate;

    private class LoopHandle : ILoopHandle
    {
        public AudioSource source;
    }

    private void Awake()
    {
        if (!oneShotSource)
        {
            oneShotSource = GetComponent<AudioSource>();
            if (!oneShotSource)
                oneShotSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (!clip || !oneShotSource) return;
        oneShotSource.PlayOneShot(clip, volume);
    }

    public void PlayOneShotAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (!clip) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    public ILoopHandle PlayLoop(AudioClip clip, float volume = 1f)
    {
        if (!clip) return null;

        AudioSource src;

        if (loopTemplate != null)
        {
            src = Instantiate(loopTemplate, transform);
        }
        else
        {
            var go = new GameObject("LoopAudio");
            go.transform.SetParent(transform);
            src = go.AddComponent<AudioSource>();
            src.loop = true;
        }

        src.clip = clip;
        src.volume = volume;
        src.loop = true;
        src.Play();

        return new LoopHandle { source = src };
    }

    public void StopLoop(ILoopHandle handle)
    {
        if (handle is not LoopHandle h || h.source == null) return;

        if (h.source.isPlaying)
            h.source.Stop();

        Destroy(h.source.gameObject);
    }
}
