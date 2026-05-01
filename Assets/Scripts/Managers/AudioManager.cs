using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

//https://www.youtube.com/watch?v=DU7cgVsU2rM
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips = new AudioClip[2]; // Slot 0 = Left, Slot 1 = Right
    private int footstepIndex = 0;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayFootstep()
    {
        if (footstepClips.Length >= 2 && footstepClips[footstepIndex] != null)
        {
            PlaySFXRandomPitch(footstepClips[footstepIndex], 0.95f, 1.05f);

            // If it's 0, (1 - 0) = 1. If it's 1, (1 - 1) = 0.
            footstepIndex = 1 - footstepIndex; 
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    private float currentMusicVolume;

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFXRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
            sfxSource.PlayOneShot(clip);
            sfxSource.pitch = 1f; // reset so it doesn’t affect future sounds
        }
    }


    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void FadeOutAndStopMusic(float fadeDuration = 1.5f)
    {
        StartCoroutine(FadeOutCoroutine(fadeDuration));
    }
    
    public void PlaySFXWithExactPitch(AudioClip clip, float exactPitch)
    {
        if (clip != null)
        {
            sfxSource.pitch = exactPitch;
            sfxSource.PlayOneShot(clip);
            sfxSource.pitch = 1f; // reset so it doesn’t affect future sounds
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // Reset for future use
    }

    public void FadeToMusic(AudioClip newClip, float duration)
    {
        StartCoroutine(FadeToMusicCoroutine(newClip, duration));
    }

    private IEnumerator FadeToMusicCoroutine(AudioClip newClip, float duration)
    {
        if (musicSource.clip == newClip)
            yield break; // Already playing this clip, no fade needed

        float startVolume = musicSource.volume;
        float halfDuration = duration / 2f;

        // Fade out current music
        float elapsed = 0f;
        if (musicSource.isPlaying)
        {
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
                yield return null;
            }
        }

        // Switch clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVolume, elapsed / halfDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}