using UnityEngine;
using Unity.Netcode;
using TMPro;

public class UILoadingMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_Text loadingText;

    public void Toggle(bool state, string text = "Connecting") {
        loadingText.text = text;
        gameObject.SetActive(state);
    }

    public void OnCancel() {
        NetworkManager.Singleton.Shutdown();
        Toggle(false);
    }
}
