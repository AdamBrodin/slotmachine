#pragma warning disable CS0649
using System;
using UnityEngine;

/* 
 * Developed by Adam Brodin
 * https://github.com/AdamBrodin
 */

[System.Serializable]
public class Sound
{
    public string name;
    public bool loopSound;

    public AudioClip clip;
    [HideInInspector] public AudioSource source;
    [Range(0f, 1f)] public float volume;
    [Range(0f, 3f)] public float pitch;
}

public class AudioManager : MonoBehaviour
{
    #region Variables
    #region Singleton
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
            }
            return instance;
        }
    }
    #endregion
    public Sound[] sounds;
    [SerializeField] private bool globalMute;
    [SerializeField] private float globalVolumeMultiplier;
    #endregion

    private void Awake() => UpdateSounds();
    private void UpdateSounds()
    {
        foreach (Sound s in sounds)
        {
            if (s.source == null)
            {
                s.source = gameObject.AddComponent<AudioSource>();
            }

            s.source.clip = s.clip;
            s.source.volume = (s.volume * globalVolumeMultiplier);
            s.source.pitch = s.pitch;
            s.source.loop = s.loopSound;
        }
    }

    public Sound GetSoundInformation(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s != null)
        {
            return s;
        }
        return null;
    }

    public void ModifySound(string name, float volume)
    {
        Sound s = GetSoundInformation(name);
        if (s != null)
        {
            s.source.volume = (volume * globalVolumeMultiplier);
        }
    }

    public void ModifySound(string name, float volume, float pitch)
    {
        Sound s = GetSoundInformation(name);
        if (s != null)
        {
            s.source.volume = (volume * globalVolumeMultiplier);
            s.source.pitch = pitch;
        }
    }

    public void SetState(string name, bool playAudio)
    {
        Sound s = GetSoundInformation(name);
        if (s != null)
        {
            if (playAudio && !globalMute)
            {
                s.source.Play();
            }
            else
            {
                s.source.Stop();
            }
        }
    }

    public void TogglePause(string name)
    {
        Sound s = GetSoundInformation(name);
        if (s != null)
        {
            if (!globalMute)
            {
                if (s.source.isPlaying)
                {
                    s.source.Pause();
                }
                else
                {
                    s.source.UnPause();
                }
            }
        }
    }
}

