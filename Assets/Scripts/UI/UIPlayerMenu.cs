using UnityEngine;
using TMPro;
using System.Linq;

public class UIPlayerMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField nameInput;

    [SerializeField]
    private UIColorPicker colorPicker;

    [SerializeField]
    private TMP_Dropdown ballTypeInput;

    [SerializeField]
    private TMP_Text errorMessage;

    void OnEnable() {
        var data = FileManager.LoadData<PlayerSettings>(PlayerSettings.FILENAME, PlayerSettings.Default);
        nameInput.text = data.name.ToString();
        colorPicker.SetColorWithFloats(data.color);
        ballTypeInput.value = data.ballType;
        errorMessage.gameObject.SetActive(false);
    }

    public void OnSave() {
        errorMessage.gameObject.SetActive(false);
        var errorMsg = validateInput();
        if (errorMsg == string.Empty) {
            var newPlayerSettings = new PlayerSettings() {
                name = nameInput.text,
                color = colorPicker.asFloatArray,
                ballType = (byte)ballTypeInput.value
            };
            FileManager.SaveData<PlayerSettings>(newPlayerSettings, PlayerSettings.FILENAME);
            UIMainMenu.singleton.GoToMainMenu();
        } else {
            errorMessage.text = errorMsg;
            errorMessage.gameObject.SetActive(true);
        }
    }

    string validateInput() {
        // Check name
        var isNameValidCharacters = nameInput.text.All(c => char.IsLetterOrDigit(c) || Globals.ALLOWED_SPECIAL_CHARACTERS.Contains(c));
        if (!isNameValidCharacters) {
            return "Name can only contain letters, numbers, and spaces!";
        }
        var isNameValidLength = nameInput.text.Length >= Globals.NAME_MIN_LENGTH && nameInput.text.Length <= Globals.NAME_MAX_LENGTH;
        if (!isNameValidLength) {
            return string.Format("Name must be between {0:D} and {1:D} characters!", Globals.NAME_MIN_LENGTH, Globals.NAME_MAX_LENGTH);
        }

        return string.Empty;
    }
}
