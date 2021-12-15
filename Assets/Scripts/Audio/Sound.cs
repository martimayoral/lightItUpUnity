using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public bool isMusic = false;

    public AudioManager.eSound name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(-1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
