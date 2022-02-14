using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class UIHostMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField portInput;

    [SerializeField]
    private TMP_InputField passcodeInput;

    [SerializeField]
    private TMP_Text errorMessage;

    [SerializeField]
    private UILoadingMenu loadingMenu;

    void OnEnable() {
        var data = FileManager.LoadData<HostInfo>(HostInfo.FILENAME, HostInfo.Default);
        portInput.text = data.port;
        passcodeInput.text = data.passcode;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        errorMessage.gameObject.SetActive(false);
    }

    void OnDisable() {
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    void OnServerStarted() {
        loadingMenu.Toggle(false);
        NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnHost() {
        errorMessage.gameObject.SetActive(false);
        var errorMsg = validateInput();
        if (errorMsg == string.Empty) {
            loadingMenu.Toggle(true, "Starting Server");
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(
                string.Format("{0},{1}", passcodeInput.text, NetworkSettings.MyNetworkId)
            );
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = "127.0.0.1";
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";
            transport.ConnectionData.Port = ushort.Parse(portInput.text);
            FileManager.SaveData<HostInfo>(new HostInfo() {
                port = portInput.text,
                passcode = passcodeInput.text
            }, HostInfo.FILENAME);
            NetworkSettings.singleton.passcode = passcodeInput.text;
            NetworkManager.Singleton.StartHost();
        } else {
            errorMessage.text = errorMsg;
            errorMessage.gameObject.SetActive(true);
        }
    }

    string validateInput() {
        // Check ip and port
        ushort port;
        bool canParsePort = ushort.TryParse(portInput.text, out port);
        if (!canParsePort && port >= Globals.PORT_MIN && port <= Globals.PORT_MAX) {
            return string.Format("Port must be a number from {0}-{1}!", Globals.PORT_MIN, Globals.PORT_MAX);
        }

        // Check passcode
        var isPasscodeValidCharacters = passcodeInput.text.All(c => char.IsDigit(c));
        if (!isPasscodeValidCharacters) {
            return "Passcodes only contain digits!";
        }
        var isPasscodeValidLength = passcodeInput.text.Length >= Globals.PASSCODE_MIN_LENGTH && passcodeInput.text.Length <= Globals.PASSCODE_MAX_LENGTH;
        if (!isPasscodeValidLength) {
            return string.Format("Passcodes are between {0:D} and {1:D} digits!", Globals.PASSCODE_MIN_LENGTH, Globals.PASSCODE_MAX_LENGTH);
        }

        return string.Empty;
    }
}
