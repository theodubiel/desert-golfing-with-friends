using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public delegate void TerrainUpdatedEvent();

public class Terrain : NetworkBehaviour
{
    public static Terrain singleton;
    public static event TerrainUpdatedEvent TerrainUpdated = delegate { };

    [SerializeField]
    private float width = 10.0f;

    [SerializeField]
    private float margin = 10.0f;

    [SerializeField]
    private float maxHeight = 5.0f;

    [SerializeField]
    private float minHeight = 3.0f;

    [SerializeField]
    private float holeWidth = 1.0f;

    [SerializeField]
    private int segments = 16;

    [SerializeField]
    private float cliffSize = 0.5f;

    [SerializeField]
    private float bottom = -20.0f;

    [SerializeField]
    private BoxCollider2D goalZone;

    [SerializeField]
    private GameObject flag;

    public NetworkVariable<int> levelSeed = new NetworkVariable<int>(1);
    private PolygonCollider2D polygonCollider;
    private MeshFilter meshFilter;

    void Awake() {
        singleton = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        polygonCollider = GetComponent<PolygonCollider2D>();
        meshFilter = GetComponent<MeshFilter>();
        if (IsServer) {
            levelSeed.Value = (int)GameSettings.STARTING_LEVEL;
        }
        OnLevelSeedChange(levelSeed.Value, levelSeed.Value);
        levelSeed.OnValueChanged += OnLevelSeedChange;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        levelSeed.OnValueChanged -= OnLevelSeedChange;
    }

    Vector2[] GenerateHole(Vector2 start, float segmentSize, float depth) {
        var margin = (segmentSize - holeWidth) / 2.0f;
        goalZone.transform.position = new Vector3(start.x + segmentSize / 2.0f, start.y - (depth / 2.0f + 0.25f), 0);
        goalZone.size = new Vector2(holeWidth, depth);
        flag.transform.position = new Vector2(start.x + margin + holeWidth + 0.5f, start.y + 0.5f);
        return new Vector2[] {
            start,
            start + new Vector2(margin, 0),
            start + new Vector2(margin, -depth),
            start + new Vector2(margin + holeWidth / 2f, -(depth + 0.25f)),
            start + new Vector2(margin + holeWidth, -depth),
            start + new Vector2(margin + holeWidth, 0),
            start + new Vector2(margin * 2 + holeWidth, 0)
        };
    }

    void GenerateTerrain(int seed = 0) {
        UnityEngine.Random.InitState(seed);
        var left = -(width / 2) - margin;
        var right = Mathf.Abs(left);
        var segmentSize = width / segments;
        var holeSegment = UnityEngine.Random.Range(segments / 2, segments);
        
        // Generate Right margin
        var points = new List<Vector2>() {
            new Vector2(right - margin + segmentSize, 0), new Vector2(right, 0),
            new Vector2(right, bottom), new Vector2(right - margin + segmentSize, bottom)
        };
        // Adds vertices at bottom to allow for cleaner mesh generation.
        var start = right - margin;
        for(var i = 0; i < segmentSize; i++) { 
            points.Add(new Vector2(start - (segmentSize * i), bottom));
        }
        // Generate left margin
        points.AddRange(new List<Vector2>() {
            new Vector2(left + margin, bottom), new Vector2(left, bottom),
            new Vector2(left, 0), new Vector2(left + margin, 0)
        });

        // Generate course
        var lastHeight = 0f;
        var startPosition = left + margin;
        for(int i = 0; i < segments; i++) {
            var startX = startPosition + i * segmentSize;
            if (i == holeSegment) {
                var holePoints = GenerateHole(new Vector2(startX, lastHeight), segmentSize, 1.0f);
                points.AddRange(holePoints);
            } else {
                var randomTerrain = UnityEngine.Random.Range(0, 3);
                if (randomTerrain == 0) { // Ramp
                    var height = UnityEngine.Random.Range(minHeight, maxHeight);
                    points.Add(new Vector2(startX + segmentSize, height));
                    lastHeight = height;
                } else if (randomTerrain == 1) { // Cliff
                    var height = Mathf.RoundToInt(UnityEngine.Random.Range(minHeight, maxHeight));
                    if (height - lastHeight >= 2.0f) {
                        points.AddRange(new Vector2[] {
                            new Vector2(startX + segmentSize / 2f, lastHeight),
                            new Vector2(startX + segmentSize / 2f, height - cliffSize),
                            new Vector2(startX + segmentSize / 2f - cliffSize, height),
                            new Vector2(startX + segmentSize / 2f + cliffSize, height)
                        });
                    } else if (lastHeight - height >= 2.0f) {
                        points.AddRange(new Vector2[] {
                            new Vector2(startX + segmentSize / 2f + cliffSize, lastHeight),
                            new Vector2(startX + segmentSize / 2f, lastHeight - cliffSize),
                            new Vector2(startX + segmentSize / 2f, height)
                        });
                    } else {
                        points.AddRange(new Vector2[] {
                            new Vector2(startX + segmentSize / 2f, lastHeight),
                            new Vector2(startX + segmentSize / 2f, height)
                        });
                    }
                    lastHeight = height;
                } else if (randomTerrain == 2) { // Flat
                    points.Add(new Vector2(startX + segmentSize, lastHeight));
                }
            }
        }
        polygonCollider.SetPath(0, points);
        var mesh = polygonCollider.CreateMesh(false, false);
        meshFilter.sharedMesh = mesh;
        Terrain.TerrainUpdated();
    }

    public Vector2 GetSpawnPoint() {
        var left = -(width / 2) - margin;
        var segmentSize = width / segments;
        return new Vector2(left + margin - segmentSize / 2, maxHeight + 1);
    }

    private void OnLevelSeedChange(int oldSeed, int newSeed) {
        GenerateTerrain(newSeed);
    }

    [ServerRpc(RequireOwnership = true)]
    public void ChangeLevelServerRpc() {
        levelSeed.Value = levelSeed.Value + 1;
    }
}
