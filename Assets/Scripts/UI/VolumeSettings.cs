using UnityEngine;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    // 0.5 slider = 0 dB (no change), 1.0 = +6 dB, 0.0 = silence
    private float ToDb(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f + 6f;
    }

    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("Master", ToDb(value));
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("Music", ToDb(value));
    }

    public void SetEffectsVolume(float value)
    {
        mixer.SetFloat("Effects", ToDb(value));
    }

    public void SetDialogueVolume(float value)
    {
        mixer.SetFloat("Dialogue", ToDb(value));
    }
}
