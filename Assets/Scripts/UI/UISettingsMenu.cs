using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UISettingsMenu : MonoBehaviour
{
    public static UISettingsMenu singleton;

    [SerializeField]
    private GameObject settingsPanel;

    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private Slider swingSensitivitySlider;

    [SerializeField]
    private TMP_InputField startLevelInput;

    [SerializeField]
    private Toggle fullScreenToggle;

    [SerializeField]
    private TMP_Text errorTextLabel;

    void Awake() {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(this.gameObject);
            GameSettings.ApplySavedSettings();
            Close();
        } else {
            Destroy(this.gameObject);
        }
    }

    void Initialize() {
        var data = FileManager.LoadData<GameSettingsData>(GameSettingsData.FILENAME, GameSettingsData.Default);
        masterVolumeSlider.value = data.masterVolume;
        swingSensitivitySlider.value = data.swingSensitivity;
        startLevelInput.text = data.startingLevel.ToString();
        masterVolumeSlider.onValueChanged.AddListener(delegate {OnSave();});
        swingSensitivitySlider.onValueChanged.AddListener(delegate {OnSave();});
        startLevelInput.onValueChanged.AddListener(delegate {OnSave();});
        fullScreenToggle.onValueChanged.AddListener(delegate {OnSave();});
    }

    void OnSave() {
        errorTextLabel.enabled = false;
        var errorText = ValidateInput();
        if (errorText == string.Empty) {
            var newSettings = new GameSettingsData() {
                masterVolume = masterVolumeSlider.value,
                swingSensitivity = swingSensitivitySlider.value,
                startingLevel = ulong.Parse(startLevelInput.text),
                fullScreen = fullScreenToggle.isOn
            };
            GameSettings.Apply(newSettings);
            FileManager.SaveData<GameSettingsData>(newSettings, GameSettingsData.FILENAME);
        } else {
            errorTextLabel.enabled = true;
            errorTextLabel.text = errorText;
        }
    }

    string ValidateInput() {
        try {
            var inputStartLevel = ulong.Parse(startLevelInput.text);
            if (inputStartLevel < GameSettings.MIN_STARTING_LEVEL) {
                return string.Format("Start Level must be greater than or equal to {0}!", GameSettings.MIN_STARTING_LEVEL);
            } else if (inputStartLevel > GameSettings.MAX_STARTING_LEVEL) {
                return string.Format("Starting level must be less than {0}!", GameSettings.MAX_STARTING_LEVEL);
            }
        } catch (Exception ex) {
            Debug.Log(ex);
            return "Start Level must be a number!";
        }
        return string.Empty;
    }

    public void SetWindowed() {
        Screen.fullScreen = false;
    }

    public void Open() {
        settingsPanel.SetActive(true);
        errorTextLabel.enabled = false;
        Initialize();
    }

    public void Close() {
        settingsPanel.SetActive(false);
    }
}
