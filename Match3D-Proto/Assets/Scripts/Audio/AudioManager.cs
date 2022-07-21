using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    AudioSource[] musicSources;
    AudioSource[] ambienceSources;
    AudioSource sfxSources;
    AudioSource collisionSFXSource;

    int activeMusicSourceIndex;
    int activeAmbienceSourceIndex;
    float volume = 0.7f;

    SoundLibrary library;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        library = GetComponent<SoundLibrary>();

        SetupMusicSources();
        SetupAmbienceSources();
        SetupSFXSources();
        SetupCollisionSFXSource();

    }

    void SetupMusicSources()
    {
        musicSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject newMusicSources = new GameObject("Music Sources " + (i + 1));
            musicSources[i] = newMusicSources.AddComponent<AudioSource>();
            musicSources[i].volume = 0.7f;
            musicSources[i].loop = true;
            newMusicSources.transform.parent = transform;
        }
    }

    void SetupAmbienceSources()
    {
        ambienceSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject newMusicSources = new GameObject("Ambience Source " + (i + 1));
            ambienceSources[i] = newMusicSources.AddComponent<AudioSource>();
            ambienceSources[i].loop = true;
            newMusicSources.transform.parent = transform;
        }
    }

    void SetupSFXSources()
    {
        GameObject newSFXSource = new GameObject("SFX Source");
        sfxSources = newSFXSource.AddComponent<AudioSource>();
        newSFXSource.transform.parent = transform;
    }

    void SetupCollisionSFXSource()
    {
        GameObject newCollisionSFXSource = new GameObject("Collision SFX Source");
        collisionSFXSource = newCollisionSFXSource.AddComponent<AudioSource>();
        newCollisionSFXSource.transform.parent = transform;
    }

    public void PlayMusic(AudioClip clip, float volume = 0.7f, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; //The active music source will switch between 0 and 1

        musicSources[activeMusicSourceIndex].clip = clip;
        this.volume = volume;
        if (clip != null)
            musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    //public void StopMusic(float fadeDuration = 1)
    //{
    //    StartCoroutine(FadeOutMusic(fadeDuration));
    //}

    public void PlayAmbience(AudioClip clip, float volume = 0.7f, float fadeDuration = 0.5f)
    {
        activeAmbienceSourceIndex = 1 - activeAmbienceSourceIndex; //The active music source will switch between 0 and 1

        ambienceSources[activeAmbienceSourceIndex].clip = clip;
        this.volume = volume;
        if (clip != null)
            ambienceSources[activeAmbienceSourceIndex].Play();

        StartCoroutine(AnimateAmbienceCrossfade(fadeDuration));
    }




    public void PlaySFX(string name, float volume)
    {
        sfxSources.PlayOneShot(library.GetClipFromName(name), volume);
    }

    public void StopSFX()
    {
        sfxSources.Stop();
    }

    public void PlayCollisionSFX(string name, float volume)
    {
        collisionSFXSource.clip = library.GetClipFromName(name);
        collisionSFXSource.volume = volume;
        collisionSFXSource.Play();
    }

    public void StopCollisionSFX()
    {
        collisionSFXSource.Stop();
    }


    //IEnumerator FadeInMusic(float duration)
    //{
    //    float percent = 0;

    //    while (percent < 1)
    //    {
    //        percent += Time.deltaTime * 1 / duration;
    //        musicSources.volume = Mathf.Lerp(0, 1, percent);
    //        yield return null;
    //    }
    //}

    //IEnumerator FadeOutMusic(float duration)
    //{
    //    float percent = 0;

    //    while (percent < 1)
    //    {
    //        percent += Time.deltaTime * 1 / duration;
    //        musicSources.volume = Mathf.Lerp(1, 0, percent);
    //        yield return null;
    //    }
    //}

    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, 1, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(1, 0, percent);
            yield return null;
        }
    }

    IEnumerator AnimateAmbienceCrossfade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            ambienceSources[activeAmbienceSourceIndex].volume = Mathf.Lerp(0, volume, percent);
            ambienceSources[1 - activeAmbienceSourceIndex].volume = Mathf.Lerp(1, volume, percent);
            yield return null;
        }
    }
}
