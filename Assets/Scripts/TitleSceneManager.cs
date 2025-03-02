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
    private float bgm_volume_percentage = 1.0f;
    private Slider bgm_volume_slider;
    private TextMeshProUGUI bgm_volume_label;

    private float se_volume_percentage = 1.0f;
    private Slider se_volume_slider;
    private TextMeshProUGUI se_volume_label;
    
    private float camera_sensitivity_percentage = 1.0f;
    private Slider camera_sensitivity_slider;
    private TextMeshProUGUI camera_sensitivity_label;

    private bool is_setting = false;
    private bool is_starting = false;

    private const string BGM_SLIDER_TAG = "BGMSlider";
    private const string SE_SLIDER_TAG = "SESlider";
    private const string CAMERA_SENSITIVITY_SLIDER_TAG = "CameraSensitivitySlider";
    private const string SLIDER_LABEL_TAG = "SliderLabel";
    private const string SETTING_WINDOW_TAG = "SettingWindow";
    private const string RULE_WINDOW_TAG = "RuleWindow";
    private const string RANKING_TEXT_TAG = "RankingScoreText";

    private const string BGM_PREFS_KEY = "BGMVolume";
    private const string SE_PREFS_KEY = "SEVolume";
    private const string CAMERA_PREFS_KEY = "CameraSensitivity";

    [Header("ウィンドウ表示効果音")]
    [Tooltip("ウィンドウ表示効果音ファイル")]
    [SerializeField]
    private AudioClip WINDOW_SE_CLIP;

    [Tooltip("ウィンドウ表示効果音ベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float WINDOW_BASE_VOLUME;


    [Header("ゲーム開始効果音")]
    [Tooltip("ゲーム開始効果音ファイル")]
    [SerializeField]
    private AudioClip START_SE_CLIP;

    [Tooltip("ゲーム開始効果音ベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float START_BASE_VOLUME;


    private void Start()
    {
        FindAllSliderObjects();
        SetupAllScrollberValues();
        LoadScoreRanking();
        HideSettingAndRuleWindows();
        return;
    }

    private void FindAllSliderObjects()
    {
        FindSliderObject(BGM_SLIDER_TAG, ref bgm_volume_slider, ref bgm_volume_label);
        FindSliderObject(SE_SLIDER_TAG, ref se_volume_slider, ref se_volume_label);
        FindSliderObject(CAMERA_SENSITIVITY_SLIDER_TAG, ref camera_sensitivity_slider, ref camera_sensitivity_label);
        return;
    }

    private void FindSliderObject(string tag, ref Slider slider, ref TextMeshProUGUI label)
    {
        GameObject tmp_gameobject = GameObject.FindGameObjectWithTag(tag);
        slider = tmp_gameobject.GetComponent<Slider>();
        foreach (Transform t in tmp_gameobject.transform)
        {
            if (t.gameObject.CompareTag(SLIDER_LABEL_TAG))
            {
                label = t.GetComponent<TextMeshProUGUI>();
            }
        }
        return;
    }

    private void SetupAllScrollberValues()
    {
        SetupScrollberValue(BGM_PREFS_KEY, ref bgm_volume_percentage, ref bgm_volume_slider, ref bgm_volume_label, 1.0f);
        SetupScrollberValue(SE_PREFS_KEY, ref se_volume_percentage, ref se_volume_slider, ref se_volume_label, 1.0f);
        SetupScrollberValue(CAMERA_PREFS_KEY, ref camera_sensitivity_percentage, ref camera_sensitivity_slider, ref camera_sensitivity_label, 0.5f);
        return;
    }

    private void SetupScrollberValue(string prefs_key, ref float percentage_variable, ref Slider slider, ref TextMeshProUGUI label, float default_value)
    {
        float percentage = Math.Min(Mathf.Round(PlayerPrefs.GetFloat(prefs_key, default_value) * 100.0f), 100.0f);
        percentage_variable = percentage;
        slider.value = percentage;
        label.text = $"{percentage}%";
        return;
    }

    private void LoadScoreRanking()
    {
        int[] scores =
        {
            PlayerPrefs.GetInt("Score1st",0),
            PlayerPrefs.GetInt("Score2nd",0),
            PlayerPrefs.GetInt("Score3rd",0),
            PlayerPrefs.GetInt("Score4th",0),
            PlayerPrefs.GetInt("Score5th",0)
        };
        GameObject ranking_label = GameObject.FindGameObjectWithTag(RANKING_TEXT_TAG);
        ranking_label.GetComponent<TextMeshProUGUI>().text = $"{scores[0]:000}\n"
                                                            +$"{scores[1]:000}\n"
                                                            +$"{scores[2]:000}\n"
                                                            +$"{scores[3]:000}\n"
                                                            +$"{scores[4]:000}";
        return;
    }

    private void HideSettingAndRuleWindows()
    {
        DisplayWindow(SETTING_WINDOW_TAG, false);
        DisplayWindow(RULE_WINDOW_TAG, false);
        return;
    }

    private void DisplayWindow(string tag, bool is_visible)
    {
        GameObject.FindGameObjectWithTag(tag).GetComponent<Canvas>().enabled = is_visible;
        return;
    }


    public void LoadAllSliderValues()
    {
        if (is_setting)
        {
            LoadSliderValue(BGM_PREFS_KEY, ref bgm_volume_percentage, bgm_volume_slider, ref bgm_volume_label);
            LoadSliderValue(SE_PREFS_KEY, ref se_volume_percentage, se_volume_slider, ref se_volume_label);
            LoadSliderValue(CAMERA_PREFS_KEY, ref camera_sensitivity_percentage, camera_sensitivity_slider, ref camera_sensitivity_label);
        }
        return;
    }

    private void LoadSliderValue(string prefs_key, ref float percentage_variable, Slider slider, ref TextMeshProUGUI label)
    {
        percentage_variable = slider.value;
        label.text = $"{percentage_variable}%";
        PlayerPrefs.SetFloat(prefs_key, percentage_variable / 100.0f);
        PlayerPrefs.Save();
        return;
    }


    private void PlaySE(AudioClip se_clip, float se_volume)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = Vector3.zero;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat(SE_PREFS_KEY,1.0f);
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
        audio_source.volume = se_volume * PlayerPrefs.GetFloat(SE_PREFS_KEY, 1.0f);
        const float SOUND_2D = 0.0f;
        audio_source.spatialBlend = SOUND_2D;
        audio_source.Play();

        Destroy(audio_object, se_clip.length);
        await Awaitable.WaitForSecondsAsync(se_clip.length);
        return;
    }


    public void OpenWindow(string tag)
    {
        if (!is_starting)
        {
            DisplayWindow(tag, true);
            if (tag == SETTING_WINDOW_TAG)
            {
                is_setting = true;
            }
            PlaySE(WINDOW_SE_CLIP, WINDOW_BASE_VOLUME);
        }
        return;
    }

    public void ExitWindow(string tag)
    {
        DisplayWindow(tag, false);
        if (tag == SETTING_WINDOW_TAG)
        {
            is_setting = false;
        }
        PlaySE(WINDOW_SE_CLIP, WINDOW_BASE_VOLUME);
        return;
    }


    public async void StargGame()
    {
        if (!is_starting)
        {
            is_starting = true;
            await AsyncPlaySE(START_SE_CLIP, START_BASE_VOLUME);
            SceneManager.LoadScene("MainGameScene");
        }
        return;
    }

    public void QuitGame()
    {
        Application.Quit();
        return;
    }
}
