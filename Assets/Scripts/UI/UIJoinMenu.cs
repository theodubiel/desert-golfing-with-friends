using UnityEngine;
using TMPro;
using System.Linq;
using Unity.Netcode;

public class UIJoinMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField ipPortInput;

    [SerializeField]
    private TMP_InputField passcodeInput;

    [SerializeField]
    private TMP_Text errorMessage;

    [SerializeField]
    private UILoadingMenu loadingMenu;

    void OnEnable() {
        var data = FileManager.LoadData<ClientInfo>(ClientInfo.FILENAME, ClientInfo.Default);
        ipPortInput.text = data.ipPort;
        passcodeInput.text = data.passcode;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        errorMessage.gameObject.SetActive(false);
    }

    void OnDisable() {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    void OnClientDisconnect(ulong clientId) {
        loadingMenu.Toggle(false);
        errorMessage.text = "Failed to connect to server";
        errorMessage.gameObject.SetActive(true);
    }

    public void OnJoin() {
        errorMessage.gameObject.SetActive(false);
        var errorMsg = validateInput();
        if (errorMsg == string.Empty) {
            loadingMenu.Toggle(true, "Joining Server");
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(
                string.Format("{0},{1}", passcodeInput.text, NetworkSettings.MyNetworkId)
            );
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var ipPort = ipPortInput.text.Split(":");
            transport.ConnectionData.Address = ipPort[0];
            transport.ConnectionData.Port = ushort.Parse(ipPort[1]);
            FileManager.SaveData<ClientInfo>(new ClientInfo() {
                ipPort = ipPortInput.text,
                passcode = passcodeInput.text
            }, ClientInfo.FILENAME);
            NetworkManager.Singleton.StartClient();
        } else {
            errorMessage.text = errorMsg;
            errorMessage.gameObject.SetActive(true);
        }
    }

    string validateInput() {
        // Check ip and port
        var ipPortArray = ipPortInput.text.Split(":");
        if (ipPortArray.Length != 2) {
            return "Ensure proper IP and Port format!";
        }
        ushort port;
        bool canParsePort = ushort.TryParse(ipPortArray[1], out port);
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
