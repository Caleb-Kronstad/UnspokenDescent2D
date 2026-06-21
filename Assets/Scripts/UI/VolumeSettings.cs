using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;
    [SerializeField] private Slider dialogueSlider;

    private const float DefaultVolume = 0.5f;

    private void OnEnable()
    {
        float master   = PlayerPrefs.GetFloat("Volume_Master",   DefaultVolume);
        float music    = PlayerPrefs.GetFloat("Volume_Music",    DefaultVolume);
        float effects  = PlayerPrefs.GetFloat("Volume_Effects",  DefaultVolume);
        float dialogue = PlayerPrefs.GetFloat("Volume_Dialogue", DefaultVolume);

        if (masterSlider)   masterSlider.SetValueWithoutNotify(master);
        if (musicSlider)    musicSlider.SetValueWithoutNotify(music);
        if (effectsSlider)  effectsSlider.SetValueWithoutNotify(effects);
        if (dialogueSlider) dialogueSlider.SetValueWithoutNotify(dialogue);

        mixer.SetFloat("Master",   ToDb(master));
        mixer.SetFloat("Music",    ToDb(music));
        mixer.SetFloat("Effects",  ToDb(effects));
        mixer.SetFloat("Dialogue", ToDb(dialogue));
    }

    // 0.5 slider = 0 dB (no change), 1.0 = +6 dB, 0.0 = silence
    private float ToDb(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f + 6f;
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("Master", ToDb(value));
        PlayerPrefs.SetFloat("Volume_Master", value);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("Music", ToDb(value));
        PlayerPrefs.SetFloat("Volume_Music", value);
    }

    public void SetEffectsVolume(float value)
    {
        mixer.SetFloat("Effects", ToDb(value));
        PlayerPrefs.SetFloat("Volume_Effects", value);
    }

    public void SetDialogueVolume(float value)
    {
        mixer.SetFloat("Dialogue", ToDb(value));
        PlayerPrefs.SetFloat("Volume_Dialogue", value);
    }
}
