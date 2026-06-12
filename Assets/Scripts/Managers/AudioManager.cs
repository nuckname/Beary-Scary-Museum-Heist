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
    
    [Header("Global Collisions")]
    [SerializeField] private AudioClip[] collisionClips;

    [Header("Guard Sounds")]
    [SerializeField] private AudioClip[] guardShoutClips;
    
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
            footstepIndex = 1 - footstepIndex; 
        }
    }

    public void PlayRandomCollisionSound()
    {
        if (collisionClips != null && collisionClips.Length > 0)
        {
            int randomIndex = Random.Range(0, collisionClips.Length);
            PlaySFXRandomPitch(collisionClips[randomIndex], 0.85f, 1.15f);
        }
    }

    public void PlayRandomGuardShout()
    {
        if (guardShoutClips != null && guardShoutClips.Length > 0)
        {
            int randomIndex = Random.Range(0, guardShoutClips.Length);
            PlaySFXRandomPitch(guardShoutClips[randomIndex], 0.95f, 1.05f);
        }
    }

    public void PlayRandomClipWithExactPitch(AudioClip[] clips, float exactPitch)
    {
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            PlaySFXWithExactPitch(clips[randomIndex], exactPitch);
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
    
    public void PlaySFXWithExactPitch(AudioClip clip, float exactPitch)
    {
        if (clip != null)
        {
            sfxSource.pitch = exactPitch;
            sfxSource.PlayOneShot(clip);
            sfxSource.pitch = 1f; // reset so it doesn’t affect future sounds
        }
    }
}