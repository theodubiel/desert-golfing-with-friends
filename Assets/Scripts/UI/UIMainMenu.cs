using UnityEngine;
using Unity.Netcode;
using System;

public enum MenuPanelType {
    MainMenu = 0,
    JoinMenu,
    HostMenu,
    PlayerMenu,
    None
}

[Serializable]
struct MenuPanel {
    public GameObject menu;
    public MenuPanelType type;
}

public class UIMainMenu : MonoBehaviour
{
    public static UIMainMenu singleton;

    [SerializeField]
    private MenuPanel[] menuPanels;

    [SerializeField]
    private GameObject loadingPanel;

    void Awake() {
        singleton = this;
    }
    void Start() {
        ShowPanel(MenuPanelType.MainMenu);
    }

    public void ShowPanel(MenuPanelType menuPanelType) {
        foreach(var menuPanel in menuPanels) {
            menuPanel.menu.SetActive(menuPanel.type == menuPanelType);
        }
    }

    public void OnSinglePlayer() {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "127.0.0.1";
        transport.ConnectionData.ServerListenAddress = "127.0.0.1";
        transport.ConnectionData.Port = 7777;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(
            string.Format("{0},{1}", NetworkSettings.singleton.passcode, NetworkSettings.MyNetworkId)
        );
        NetworkManager.Singleton.OnServerStarted += onServerStarted_Server;
        NetworkManager.Singleton.StartHost();
    }

    private void onServerStarted_Server(){
        NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        NetworkManager.Singleton.OnServerStarted -= onServerStarted_Server;
    }

    public void OnHost() {
        ShowPanel(MenuPanelType.HostMenu);
    }

    public void OnJoin() {
        ShowPanel(MenuPanelType.JoinMenu);
    }

    public void OnPlayer() {
        ShowPanel(MenuPanelType.PlayerMenu);
    }

    public void OnSettings() {
        UISettingsMenu.singleton.Open();
    }

    public void GoToMainMenu() {
        ShowPanel(MenuPanelType.MainMenu);
    }

    public void OnQuit() {
        Application.Quit();
    }
}
