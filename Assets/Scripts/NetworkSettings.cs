using UnityEngine;
using Unity.Netcode;
using System;

using System.Collections.Generic;

[Serializable]
class NetworkID
{
    public static string FILENAME = "network-id";
    public string networkId;

    public static readonly NetworkID Default = new NetworkID() {
        networkId = Utility.GenerateId()
    };
}

public class NetworkSettings: MonoBehaviour
{
    public static NetworkSettings singleton;

    [SerializeField]
    private bool refreshNetworkIdOnStart = false;

    public Dictionary<ulong, string> clientGlobalIds = new Dictionary<ulong, string>();

    private string _passcode = "";

    public string passcode {
        get  => _passcode;
        set => _passcode = value;
    }

    private ushort _maxPlayers = 8;

    public ushort maxPlayers {
        get  => _maxPlayers;
        set => _maxPlayers = value;
    }

    public static string MyNetworkId = "";

    void Awake() {
        if (singleton == null)
        {
            singleton = this;
            LoadNetworkID(refreshNetworkIdOnStart);
        } else {
            Destroy(this.gameObject);
        }
    }

    void Start() {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    void OnServerStarted() {
        clientGlobalIds.Clear();
    }

    void ConnectionApprovalCallback(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        if (NetworkManager.Singleton.IsServer) {
            if (NetworkManager.Singleton.ConnectedClients.Count >= _maxPlayers) {
                callback(false, null, false, null, null);
            } else {
                try {
                    var passcodeAndNonce = System.Text.Encoding.ASCII.GetString(connectionData).Split(",");
                    if (passcode == passcodeAndNonce[0]) {
                        clientGlobalIds.Add(clientId, passcodeAndNonce[1]);
                        callback(false, null, true, null, null);
                    } else {
                        callback(false, null, false, null, null);
                    }
                } catch (Exception ex) {
                    Debug.Log(ex);
                    callback(false, null, false, null, null);
                }
            }
        }
    }

    public static string LoadNetworkID(bool refresh = false) {
        var networkId = FileManager.LoadData<NetworkID>(NetworkID.FILENAME);
        if (networkId == null || refresh) {
            networkId = NetworkID.Default;
            FileManager.SaveData<NetworkID>(networkId, NetworkID.FILENAME);
        }
        MyNetworkId = networkId.networkId;
        return MyNetworkId;
    }
}
