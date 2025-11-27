using UnityEngine;

public interface IAudioService                 // DIP: gameplay depends on this
{
    void PlayOneShot(AudioClip clip, float volume = 1f);

    void PlayMusic(AudioClip clip, float volume = 1f);

    void StopMusic();
}

public interface ILoopHandle { }
