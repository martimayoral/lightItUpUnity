using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public enum eSound
    {
        Theme,
        Select,
        Move,
        NoMove,
        Win,
        Lose
    }

    public Sound[] sounds;

    Dictionary<eSound, Sound> dSounds;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        dSounds = new Dictionary<eSound, Sound>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume * (s.isMusic ? UserConfig.musicVolume : UserConfig.soundVolume);
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            if (dSounds.ContainsKey(s.name))
            {
                Debug.LogWarning("Sound " + s.name + " is duplicated!");
                return;
            }
            dSounds.Add(s.name, s);
        }
    }

    public void PlaySound(eSound sound)
    {
        if (!dSounds.ContainsKey(sound))
        {
            Debug.LogWarning("Sound " + sound + " not found!");
            return;
        }

        dSounds[sound].source.Play();
    }

    public void Start()
    {
        PlaySound(eSound.Theme);
    }

    // from 0 to 1
    public void updateMusicVolume()
    {
        foreach (Sound s in sounds)
        {
            if (s.isMusic)
                s.source.volume = UserConfig.musicVolume * s.volume;
        }
    }

    // from 0 to 1
    public void updateSoundVolume()
    {
        foreach (Sound s in sounds)
        {
            if (!s.isMusic)
                s.source.volume = UserConfig.soundVolume * s.volume;
        }
    }
}