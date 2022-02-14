using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    void OnEnable() {
        GlobalCache.Clear();
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += onClientConnected_Server;
            foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds) {
                onClientConnected_Server(clientId);
            }
        }
        if (NetworkManager.Singleton.IsClient) {
            NetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnect_Client;
        }
    }

    void OnDisable() {
        NetworkManager.Singleton.OnClientConnectedCallback -= onClientConnected_Server;
        NetworkManager.Singleton.OnClientDisconnectCallback -= onClientDisconnect_Client;
    }

    void onClientConnected_Server(ulong clientId) {
        var playerObj = Instantiate(PlayerPrefab);
        var playerNetworkObject = playerObj.GetComponent<NetworkObject>();
        playerNetworkObject.SpawnAsPlayerObject(clientId);
    }

    private void onClientDisconnect_Client(ulong clientId) {
        if (clientId == 0) {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
