using UnityEngine;
using UnityEngine.Audio;

public class AudioLoader : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    private void Awake()
    {
        float master   = PlayerPrefs.GetFloat("Volume_Master",   0.5f);
        float music    = PlayerPrefs.GetFloat("Volume_Music",    0.5f);
        float effects  = PlayerPrefs.GetFloat("Volume_Effects",  0.5f);
        float dialogue = PlayerPrefs.GetFloat("Volume_Dialogue", 0.5f);

        mixer.SetFloat("Master",   ToDb(master));
        mixer.SetFloat("Music",    ToDb(music));
        mixer.SetFloat("Effects",  ToDb(effects));
        mixer.SetFloat("Dialogue", ToDb(dialogue));
    }

    private float ToDb(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f + 6f;
    }
}
