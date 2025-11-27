using UnityEngine;

public interface IAudioService                 // DIP: gameplay depends on this
{

    void PlayOneShot(AudioClip clip, float volume = 1f);


    void PlayOneShotAtPoint(AudioClip clip, Vector3 position, float volume = 1f);


    ILoopHandle PlayLoop(AudioClip clip, float volume = 1f);


    void StopLoop(ILoopHandle handle);
}

public interface ILoopHandle { }               // tiny token for loops
