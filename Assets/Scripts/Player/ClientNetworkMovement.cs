using UnityEngine;
using Unity.Netcode;

struct ClientMovement : INetworkSerializable
{
    public Vector2 Position;
    public float Rotation;
    public float AngularVelocity;
    public Vector2 Velocity;
    public float DirtyTime;
    public bool Teleport;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
        serializer.SerializeValue(ref AngularVelocity);
        serializer.SerializeValue(ref Velocity);
        serializer.SerializeValue(ref DirtyTime);
        serializer.SerializeValue(ref Teleport);
    }

    public static ClientMovement Default = new ClientMovement() {
        Position = Vector2.zero,
        Rotation = 0f,
        AngularVelocity = 0f,
        Velocity = Vector2.zero,
        DirtyTime = 0f,
        Teleport = true
    };
}

public class ClientNetworkMovement : NetworkBehaviour
{
    private NetworkVariable<ClientMovement> movement = new NetworkVariable<ClientMovement>(ClientMovement.Default);
    private Rigidbody2D rgbd2d;
    private float lastUpdate = 0f;

    [SerializeField]
    private float updateInterval = 0.1f;

    void Awake() {
        rgbd2d = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) {
            movement.OnValueChanged += (ClientMovement oldMovement, ClientMovement newMovement) => {
                if (newMovement.Teleport) {
                    transform.position = newMovement.Position;
                    transform.rotation = Quaternion.Euler(0, 0, newMovement.Rotation);
                    rgbd2d.velocity = newMovement.Velocity;
                    rgbd2d.angularVelocity = newMovement.AngularVelocity;
                } else {
                    var timeDifference = Mathf.Max(newMovement.DirtyTime - oldMovement.DirtyTime, 0.001f);
                    var offsetVelocity = (1 / timeDifference) * (newMovement.Position - (Vector2)transform.position);
                    rgbd2d.velocity = newMovement.Velocity + offsetVelocity;
                    transform.rotation = Quaternion.Euler(0, 0, newMovement.Rotation);
                    rgbd2d.angularVelocity = newMovement.AngularVelocity;
                }
            };
        }
    }
    
    [ServerRpc]
    private void SetMovementServerRpc(ClientMovement newMovement) {
        movement.Value = newMovement;
        movement.SetDirty(true);
    }

    void Update() {
        if (IsOwner && Time.time - lastUpdate >= updateInterval) {
            PushUpdate();
        }
    }

    public void PushUpdate(bool teleport = false) {
        SetMovementServerRpc(new ClientMovement {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles.z,
            AngularVelocity = rgbd2d.angularVelocity,
            Velocity = rgbd2d.velocity,
            DirtyTime = NetworkManager.Singleton.LocalTime.TimeAsFloat,
            Teleport = teleport
        });
        lastUpdate = Time.time;
    }
}
