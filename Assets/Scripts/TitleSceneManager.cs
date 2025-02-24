using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Threading.Tasks;
using System.Reflection.Emit;

public class TitleSceneManager : MonoBehaviour
{
    private float bgm_volume = 1.0f;
    private Slider bgm_slider;
    private TextMeshProUGUI bgm_label;

    private float se_volume = 1.0f;
    private Slider se_slider;
    private TextMeshProUGUI se_label;
    
    private float camera_sensitivity = 1.0f;
    private Slider camera_slider;
    private TextMeshProUGUI camera_label;

    private bool is_setting = false;
    private bool is_starting = false;

    [SerializeField]
    [Tooltip("ウィンドウ表示SE")]
    private AudioClip WINDOW_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("ウィンドウSEボリューム")]
    private float WINDOW_VOLUME;

    [SerializeField]
    [Tooltip("ゲーム開始SE")]
    private AudioClip START_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("ゲーム開始SEボリューム")]
    private float START_VOLUME;

    private void Start()
    {
        GetGameObjects();
        StoreScrollberValues();
        LoadRanking();
        HideSettingAndManualWindows();
        return;
    }

    private void HideSettingAndManualWindows()
    {
        DisplayWindow("SettingUIs", false);
        DisplayWindow("ManualUI", false);
        return;
    }

    private void StoreScrollberValues()
    {
        StoreSingleScrollberValue("BGMVolume", ref bgm_volume, ref bgm_slider, ref bgm_label, 1.0f);
        StoreSingleScrollberValue("SEVolume", ref se_volume, ref se_slider, ref se_label, 1.0f);
        StoreSingleScrollberValue("CameraSensitivity", ref camera_sensitivity, ref camera_slider, ref camera_label, 0.5f);
        return;
    }

    private void StoreSingleScrollberValue(String parameter, ref float value, ref Slider slider, ref TextMeshProUGUI label, float default_value)
    {
        float read_value = Math.Min(Mathf.Round(PlayerPrefs.GetFloat(parameter, default_value) * 100.0f), 100.0f);
        value = read_value;
        slider.value = read_value;
        label.text = $"{read_value}%";
        return;
    }

    private void LoadRanking()
    {
        GameObject ranking_label = GameObject.FindGameObjectWithTag("RankingText");
        int[] score =
        {
            PlayerPrefs.GetInt("Score1st",10),
            PlayerPrefs.GetInt("Score2nd",7),
            PlayerPrefs.GetInt("Score3rd",5),
            PlayerPrefs.GetInt("Score4th",3),
            PlayerPrefs.GetInt("Score5th",1)
        };
        ranking_label.GetComponent<TextMeshProUGUI>().text = $"{score[0]:000}\n{score[1]:000}\n{score[2]:000}\n{score[3]:000}\n{score[4]:000}";
        return;
    }

    private void GetGameObjects()
    {
        GetSingleSliderObject("BGMSlider", ref bgm_slider, ref bgm_label);
        GetSingleSliderObject("SESlider", ref se_slider, ref se_label);
        GetSingleSliderObject("CameraSensitivitySlider", ref camera_slider, ref camera_label);
        return;
    }

    private void GetSingleSliderObject(String tag, ref Slider slider, ref TextMeshProUGUI label)
    {
        GameObject tmp_gameobject = GameObject.FindGameObjectWithTag(tag);
        slider = tmp_gameobject.GetComponent<Slider>();
        foreach (Transform t in tmp_gameobject.transform)
        {
            if (t.gameObject.CompareTag("SliderLabel"))
            {
                label = t.GetComponent<TextMeshProUGUI>();
            }
        }
        return;
    }

    private void UpdateSliders()
    {
        if (is_setting)
        {
            UpdateSingleSlider("BGMVolume", ref bgm_volume, bgm_slider, ref bgm_label);
            UpdateSingleSlider("SEVolume", ref se_volume, se_slider, ref se_label);
            UpdateSingleSlider("CameraSensitivity", ref camera_sensitivity, camera_slider, ref camera_label);
        }
        return;
    }

    private void UpdateSingleSlider(String parameter, ref float value, Slider slider, ref TextMeshProUGUI label)
    {
        value = slider.value;
        label.text = $"{value}%";
        PlayerPrefs.SetFloat(parameter, value / 100.0f);
        PlayerPrefs.Save();
        return;
    }

    private async void StargGame()
    {
        if (!is_starting)
        {
            is_starting = true;
            await AsyncPlaySE(START_SE, START_VOLUME);
            SceneManager.LoadScene("MainGameScene");
        }
        return;
    }

    public void LaunchWindow(String tag)
    {
        if (!is_starting)
        {
            DisplayWindow(tag, true);
            if (tag == "SettingUIs")
            {
                is_setting = true;
            }
            PlaySE(WINDOW_SE, WINDOW_VOLUME);
        }
        return;
    }

    public void ExitWindow(String tag)
    {
        DisplayWindow(tag, false);
        if (tag == "SettingUIs")
        {
            is_setting = false;
        }
        PlaySE(WINDOW_SE, WINDOW_VOLUME);
        return;
    }

    private void PlaySE(AudioClip se_clip, float se_volume)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = Vector3.zero;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat("SEVolume");
        const float SOUND_2D = 0.0f;
        audio_source.spatialBlend = SOUND_2D;
        audio_source.Play();

        Destroy(audio_object, se_clip.length);
        return;
    }

    private async ValueTask AsyncPlaySE(AudioClip se_clip, float se_volume)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = Vector3.zero;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat("SEVolume");
        const float SOUND_2D = 0.0f;
        audio_source.spatialBlend = SOUND_2D;
        audio_source.Play();

        Destroy(audio_object, se_clip.length);
        await Awaitable.WaitForSecondsAsync(se_clip.length);
        return;
    }

    private void DisplayWindow(String tag, bool is_visible)
    {
        GameObject.FindGameObjectWithTag(tag).GetComponent<Canvas>().enabled = is_visible;
        return;
    }

    private void QuitGame()
    {
        Application.Quit();
        return;
    }
}
