using UnityEngine;
using System;
using System.Linq;

public enum AudioClips {
    swing, sand
}

[Serializable]
struct SoundEffect {
    public AudioClip audioClip;
    public AudioClips type;
}

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer singleton;

    [SerializeField]
    private SoundEffect[] soundEffects;

    [SerializeField]
    private AudioSource audioSource;

    void Start()
    {
        singleton = this;
    }

    public void PlaySound(AudioClips type) {
        var soundEffect = soundEffects.First(soundEffect => soundEffect.type == type);
        audioSource.PlayOneShot(soundEffect.audioClip, GameSettings.MASTER_VOLUME);
    }
}
