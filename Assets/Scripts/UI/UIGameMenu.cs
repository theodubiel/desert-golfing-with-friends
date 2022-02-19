using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class UIGameMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject escapeMenu;

    [SerializeField]
    private GameObject forceChangeLevelButton;

    [SerializeField]
    private TMP_Text holeLabel;

    [SerializeField]
    private float levelChangeDelay = 3.0f;
    
    private float lastTimePlayerNotFinished = 0f;

    private bool inTransition = false;

    void OnEnable() {
        Terrain.TerrainUpdated += UpdateHoleNumber;
        PlayerData.AnyPlayerDataChange += UpdateChangeLevelButton;
    }

    void OnDisable() {
        Terrain.TerrainUpdated -= UpdateHoleNumber;
        PlayerData.AnyPlayerDataChange -= UpdateChangeLevelButton;
    }

    void Start() {
        escapeMenu.SetActive(false);
        forceChangeLevelButton.SetActive(NetworkManager.Singleton.IsHost);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ToggleEscapeMenu();
        }
        UpdateChangeLevelButton();
    }

    void UpdateHoleNumber() {
        holeLabel.text = string.Format("Hole {0}", Terrain.singleton.levelSeed.Value.ToString());
        inTransition = false;
    }

    void UpdateChangeLevelButton() {
        if (NetworkManager.Singleton.IsHost) {
            var playerDatas = FindObjectsOfType<PlayerData>();
            var allPlayersFinished = playerDatas.All(playerData => playerData.data.Value.completeCurrentHole);
            if (!allPlayersFinished) {
                lastTimePlayerNotFinished = Time.time;
            }
            if (Time.time - lastTimePlayerNotFinished >= levelChangeDelay && !inTransition) {
                ChangeLevel();
            }
        }
    }

    public void ToggleEscapeMenu() {
        escapeMenu.SetActive(!escapeMenu.activeSelf);
    }

    public void OnSettings() {
        UISettingsMenu.singleton.Open();
    }

    public void OnDisconnect() {
        if (NetworkManager.Singleton.IsServer) {
            List<ulong> clientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
            foreach(var clientId in clientIds) {
                NetworkManager.Singleton.DisconnectClient(clientId);
            }
        }
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void ChangeLevel() {
        inTransition = true;
        Terrain.singleton.ChangeLevelServerRpc();
    }
}
