using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider masterSlider;

    [Header("Percent labels")]
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text masterValueText;

    private void Awake()
    {
        Setup(musicSlider);
        Setup(masterSlider);

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
    }

    private void OnEnable()
    {
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(GameAudioSettings.MusicVolume);
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(GameAudioSettings.MasterVolume);

        UpdateLabels();
    }

    private void OnDisable()
    {
        GameAudioSettings.Save();
    }

    private static void Setup(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnMusicChanged(float value)
    {
        GameAudioSettings.SetMusicVolume(value);
        UpdateLabels();
    }

    private void OnMasterChanged(float value)
    {
        GameAudioSettings.SetMasterVolume(value);
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (musicValueText != null) musicValueText.text = Mathf.RoundToInt(GameAudioSettings.MusicVolume * 100f) + "%";
        if (masterValueText != null) masterValueText.text = Mathf.RoundToInt(GameAudioSettings.MasterVolume * 100f) + "%";
    }
}