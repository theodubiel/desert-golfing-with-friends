using UnityEngine;
using Unity.Netcode;

public class ClientNetworkRigidbody2D : NetworkBehaviour
{
    private Rigidbody2D rgbd2d;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rgbd2d = GetComponent<Rigidbody2D>();
        if (IsOwner)
        {
            rgbd2d.bodyType = RigidbodyType2D.Dynamic;
            rgbd2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        } else {
            rgbd2d.bodyType = RigidbodyType2D.Kinematic;
            rgbd2d.interpolation = RigidbodyInterpolation2D.None;
        }
    }
}
