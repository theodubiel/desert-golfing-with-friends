using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System;

public enum BallType {
    Circle, Square, Hexagon
}

[Serializable]
public struct PlayerModel {
    public GameObject model;
    public BallType ballType;
}

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject impactParticlesPrefab;

    [SerializeField]
    private float impactThreshold = 0.25f;

    [SerializeField]
    private float velocitySleepThreshold = 0.1f;

    [SerializeField]
    private float sleepDuration = 1.0f;

    [SerializeField]
    private LayerMask boundaryLayer;

    [SerializeField]
    private LayerMask goalLayer;

    [SerializeField]
    private float minSwingThreshold = 0.1f;

    [SerializeField]
    private PlayerModel[] playerModels;

    private SwingMeter swingMeter;

    private Rigidbody2D rgbd2d;

    public PlayerData playerData;

    private Vector2 swingStartMousePosition;

    private ClientNetworkMovement clientNetworkMovement;

    private bool swinging = false;

    private float lastInMotion = 0f;

    private bool inGoal = false;

    private bool completeHole = false;

    void Awake() {
        playerData.PlayerSettingsChange += OnPlayerSettingsChange;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rgbd2d = GetComponent<Rigidbody2D>();
        clientNetworkMovement = GetComponent<ClientNetworkMovement>();
        swingMeter = GameObject.FindObjectOfType<SwingMeter>();
        transform.position += new Vector3(0, 0, -((int)OwnerClientId)); // Move to front.
        
        if (IsLocalPlayer) {
            var data = FileManager.LoadData<PlayerSettings>(PlayerSettings.FILENAME, PlayerSettings.Default);
            playerData.SetPlayerSettingsServerRpc(data.name, Utility.FloatsToColor(data.color), data.ballType);
            Terrain.TerrainUpdated += OnTerrainUpdated;
            ResetPosition();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerData.PlayerSettingsChange -= OnPlayerSettingsChange;
        Terrain.TerrainUpdated -= OnTerrainUpdated;
    }

    void OnPlayerSettingsChange(PlayerSerializedSettings newData) {
        foreach(var playerModel in playerModels) {
            if (playerModel.ballType == (BallType)newData.playerBallType) {
                var spriteRenderer = playerModel.model.GetComponent<SpriteRenderer>();
                spriteRenderer.color = newData.playerColor;
                spriteRenderer.sortingOrder = (IsOwner) ? 1 : 0;
            }
            playerModel.model.SetActive(playerModel.ballType == (BallType)newData.playerBallType);
        }
    }

    void Update()
    {
        if (IsOwner) {
            if (rgbd2d.velocity.magnitude > velocitySleepThreshold) {
                lastInMotion = Time.time;
            }
            if (!inGoal) {
                Swing();
            } else if (inGoal && CheckCanSwing() && !completeHole) {
                completeHole = true;
                playerData.OnHoleCompleteServerRpc();
            }
        }
    }

    void OnTerrainUpdated() {
        playerData.OnHoleChangeServerRpc();
        ResetPosition();
    }

    void ResetPosition() {
        transform.position = Terrain.singleton.GetSpawnPoint();
        transform.rotation = Quaternion.identity;
        var clientRigidbody2d = GetComponent<ClientNetworkRigidbody2D>();
        rgbd2d.velocity = Vector2.zero;
        rgbd2d.angularVelocity = 0f;
        clientNetworkMovement.PushUpdate(true);
        inGoal = false;
        completeHole = false;
    }

    [ServerRpc]
    private void OnSwingServerRPC() {
        OnSwingClientRPC();
    }

    [ClientRpc]
    private void OnSwingClientRPC() {
        if (!IsOwner) {
            AudioPlayer.singleton.PlaySound(AudioClips.swing);
        }
    }

    bool CheckCanSwing() {
        return Time.time - lastInMotion >= sleepDuration;
    }

    void Swing() {
        var canSwing = CheckCanSwing();

        if(Input.GetMouseButtonDown(1) || !canSwing) {
            swinging = false;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && canSwing) {
            swinging = true;
            swingStartMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && swinging) {
            Vector2 swingStart = (Vector2)Camera.main.ScreenToWorldPoint(swingStartMousePosition);
            Vector2 swingEnd = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 swingDir = swingEnd - swingStart;
            swingMeter.Set(swingStart, swingEnd);
        } else {
            swingMeter.Clear();
        }

        if (Input.GetMouseButtonUp(0) && swinging) {
            swinging = false;
            Vector2 swingStart = (Vector2)Camera.main.ScreenToWorldPoint(swingStartMousePosition);
            Vector2 swingEnd = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var direction = swingStart - swingEnd;
            if (direction.magnitude >= minSwingThreshold) {
                var multiplier = Mathf.Sqrt(direction.magnitude * 2f * 9.81f);
                rgbd2d.velocity = direction.normalized * multiplier * GameSettings.SWING_SENSITIVITY;
                lastInMotion = Time.time;
                AudioPlayer.singleton.PlaySound(AudioClips.swing);
                OnSwingServerRPC();
                playerData.OnSwingServerRpc();
                clientNetworkMovement.PushUpdate();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        var terrain = other.gameObject.GetComponent<Terrain>();
        if (terrain && other.relativeVelocity.magnitude > impactThreshold) {
            var particles = GameObject.Instantiate(impactParticlesPrefab, other.contacts[0].point, Quaternion.identity);
            AudioPlayer.singleton.PlaySound(AudioClips.sand);
            Destroy(particles, 1.0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (IsOwner) {
            if (boundaryLayer == (boundaryLayer | (1 << other.gameObject.layer))) {
                playerData.OnPenaltyServerRpc();
                ResetPosition();
            }
            if (goalLayer == (goalLayer | (1 << other.gameObject.layer))) {
                inGoal = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (IsOwner) {
            inGoal = false;
        }
    }
}
