using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    public bool InfiniteDash;
    public bool GodMode;
    public float Volume = 1.0f;

    private void Awake()
    {
        // 如果已經有實例了，直接銷毀自己，確保只留一個
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetVolume(float volume)
    {
        Volume = volume;
        AudioListener.volume = volume;
    }

}