using UnityEngine;

public class BGMBootstrap : MonoBehaviour
{
    private static AudioSource src;
    private const string PREF_BGM = "BGMOnOff";

    void Awake()
    {
        src = GetComponent<AudioSource>();
        if (!src) return;

        bool on = PlayerPrefs.GetInt(PREF_BGM, 1) == 1;
        Apply(on);
    }

    public static void SetBGM(bool on)
    {
        if (!src) return;

        PlayerPrefs.SetInt(PREF_BGM, on ? 1 : 0);
        PlayerPrefs.Save();

        Apply(on);
    }

    private static void Apply(bool on)
    {
        if (!src) return;

        if (on)
        {
            src.mute = false;
            if (!src.isPlaying && src.clip != null)
                src.Play();
        }
        else
        {
            src.mute = true;
            src.Stop();
        }
    }
}