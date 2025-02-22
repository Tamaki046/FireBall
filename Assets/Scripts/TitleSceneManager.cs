using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

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


    private void Start()
    {
        GetGameObjects();
        StoreScrollberValues();
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
        StoreSingleScrollberValue("BGMVolume", ref bgm_volume, ref bgm_slider, ref bgm_label);
        StoreSingleScrollberValue("SEVolume", ref se_volume, ref se_slider, ref se_label);
        StoreSingleScrollberValue("CameraSensitivity", ref camera_sensitivity, ref camera_slider, ref camera_label);
        return;
    }

    private void StoreSingleScrollberValue(String parameter, ref float value, ref Slider slider, ref TextMeshProUGUI label)
    {
        float read_value = Mathf.Round(PlayerPrefs.GetFloat(parameter, 100.0f));
        value = read_value;
        slider.value = read_value;
        label.text = $"{read_value}%";
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
        UpdateSingleSlider("BGMVolume", ref bgm_volume, bgm_slider, ref bgm_label);
        UpdateSingleSlider("SEVolume", ref se_volume, se_slider, ref se_label);
        UpdateSingleSlider("CameraSensitivity", ref camera_sensitivity, camera_slider, ref camera_label);
        PlayerPrefs.Save();
        return;
    }

    private void UpdateSingleSlider(String parameter, ref float value, Slider slider, ref TextMeshProUGUI label)
    {
        value = slider.value;
        label.text = $"{value}%";
        PlayerPrefs.SetFloat(parameter, value);
        //PlayerPrefs.SetFloat(parameter, value / 100.0f);
        return;
    }

    private void StargGame()
    {
        SceneManager.LoadScene("MainGameScene");
        return;
    }

    private void LaunchSetting()
    {
        DisplayWindow("SettingUIs", true);
        return;
    }

    private void ExitSetting()
    {
        DisplayWindow("SettingUIs", false);
        return;
    }

    private void LaunchManual()
    {
        DisplayWindow("ManualUI", true);
        return;
    }

    private void ExitManual()
    {
        DisplayWindow("ManualUI", false);
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
