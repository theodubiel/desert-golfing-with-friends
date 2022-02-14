using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// Stores player character settings
public struct PlayerSerializedSettings : INetworkSerializable
{
    public FixedString64Bytes playerName;
    public Color playerColor;
    public byte playerBallType;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerColor);
        serializer.SerializeValue(ref playerBallType);
    }

    public static PlayerSerializedSettings Default = new PlayerSerializedSettings() {
        playerName = PlayerSettings.Default.name,
        playerColor = Utility.FloatsToColor(PlayerSettings.Default.color),
        playerBallType = 1
    };
}

// Stores player score and other game data
public struct PlayerSerializedData : INetworkSerializable
{
    public bool completeCurrentHole;
    public ulong currentHoleStrokes;
    public ulong holesMade;
    public ulong totalStrokes;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref completeCurrentHole);
        serializer.SerializeValue(ref currentHoleStrokes);
        serializer.SerializeValue(ref holesMade);
        serializer.SerializeValue(ref totalStrokes);
    }

    public static PlayerSerializedData Default = new PlayerSerializedData() {
        completeCurrentHole = false,
        currentHoleStrokes = 0,
        holesMade = 0,
        totalStrokes = 0
    };
}
public delegate void ChangeEvent();
public delegate void PlayerDataChangeEvent(PlayerSerializedData newData);
public delegate void PlayerSettingsChangeEvent(PlayerSerializedSettings newData);

public class PlayerData : NetworkBehaviour
{
    public static event ChangeEvent PlayerCountChange = delegate {};
    public static event ChangeEvent AnyPlayerDataChange = delegate {};
    public event PlayerDataChangeEvent PlayerDataChange = delegate {};
    public event PlayerSettingsChangeEvent PlayerSettingsChange = delegate {};
    public NetworkVariable<PlayerSerializedSettings> settings = new NetworkVariable<PlayerSerializedSettings>(PlayerSerializedSettings.Default);
    public NetworkVariable<PlayerSerializedData> data = new NetworkVariable<PlayerSerializedData>(PlayerSerializedData.Default);

    void Awake() {
        data.OnValueChanged += OnDataChanged;
        settings.OnValueChanged += OnSettingChanged;
        PlayerData.PlayerCountChange();
        AnyPlayerDataChange();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        PlayerDataChange(data.Value);
        PlayerSettingsChange(settings.Value);
        if (IsServer) {
            CacheConnectPlayer();
        }
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        data.OnValueChanged -= OnDataChanged;
        settings.OnValueChanged -= OnSettingChanged;
        PlayerData.PlayerCountChange();
        PlayerDataChange(default);
        PlayerSettingsChange(default);
        AnyPlayerDataChange();
        if (IsServer) {
            CacheDisconnectedPlayer();
        }
    }

    void CacheConnectPlayer() {
        if (IsServer && NetworkSettings.singleton.clientGlobalIds.ContainsKey(OwnerClientId)) {
            var ownerNetworkId = NetworkSettings.singleton.clientGlobalIds[OwnerClientId];
            data.Value = GlobalCache.Get<PlayerSerializedData>(ownerNetworkId, data.Value);
        }
    }

    void CacheDisconnectedPlayer() {
        if (IsServer && NetworkSettings.singleton.clientGlobalIds.ContainsKey(OwnerClientId)) {
            var ownerNetworkId = NetworkSettings.singleton.clientGlobalIds[OwnerClientId];
            GlobalCache.Set<PlayerSerializedData>(ownerNetworkId, data.Value);
        }
    }

    void OnDataChanged(PlayerSerializedData oldData, PlayerSerializedData newData) {
        PlayerDataChange(newData);
        AnyPlayerDataChange();
    }

    void OnSettingChanged(PlayerSerializedSettings oldData, PlayerSerializedSettings newData) {
        PlayerSettingsChange(newData);
    }

    [ServerRpc(RequireOwnership = true)]
    public void OnHoleCompleteServerRpc() {
        var oldData = data.Value;
        oldData.completeCurrentHole = true;
        oldData.holesMade += 1;
        oldData.totalStrokes += oldData.currentHoleStrokes;
        data.Value = oldData;
    }

    [ServerRpc(RequireOwnership = true)]
    public void OnSwingServerRpc() {
        var oldData = data.Value;
        oldData.completeCurrentHole = false;
        oldData.currentHoleStrokes += 1;
        data.Value = oldData;
    }

    [ServerRpc(RequireOwnership = true)]
    public void OnPenaltyServerRpc() {
        var oldData = data.Value;
        oldData.completeCurrentHole = false;
        oldData.currentHoleStrokes += 1;
        data.Value = oldData;
    }

    [ServerRpc(RequireOwnership = true)]
    public void OnHoleChangeServerRpc() {
        var oldData = data.Value;
        oldData.completeCurrentHole = false;
        oldData.currentHoleStrokes = 0;
        data.Value = oldData;
    }

    [ServerRpc(RequireOwnership = true)]
    public void SetPlayerSettingsServerRpc(string playerName, Color playerColor, byte playerBallType) {
        var oldData = settings.Value;
        oldData.playerName = playerName;
        oldData.playerColor = playerColor;
        oldData.playerBallType = playerBallType;
        settings.Value = oldData;
    }
}
