using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] bgTracks;
    public AudioClip[] ambienceTracks;
    // Start is called before the first frame update
    void Start()
    {
        PlayMusic();
        PlayAmbience();
    }

    // Update is called once per frame
    void PlayMusic()
    {
        CancelInvoke();
        var clipToPlay = bgTracks[Random.Range(0, bgTracks.Length)];
        AudioManager.instance.PlayMusic(clipToPlay);
        Invoke("PlayMusic", clipToPlay.length);
    }

    void PlayAmbience()
    {
        CancelInvoke();
        var clipToPlay = ambienceTracks[Random.Range(0, ambienceTracks.Length)];
        AudioManager.instance.PlayAmbience(clipToPlay);
        Invoke("PlayAmbience", clipToPlay.length);
    }
}
