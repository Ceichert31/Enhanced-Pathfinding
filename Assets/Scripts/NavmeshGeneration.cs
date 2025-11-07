using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class NavmeshGeneration : MonoBehaviour
{
    public Dictionary<Vector2, TerrainData> navMeshGrid = new();

    [SerializeField]
    private Vector2 minBound;
    [SerializeField]
    private Vector2 maxBound;
    [SerializeField]
    private int obstacleLayer;
    public bool EnableDebug
    {
        get => enableDebug; set => enableDebug = value;
    }
    [SerializeField]
    private bool enableDebug;

    [Range(3f, 5f)]
    [SerializeField]
    private int debugResolution = 5;

    [SerializeField]
    private GameObject debugPrefab;
    [SerializeField]
    private GameObject obstaclePrefab;

    [SerializeField]
    private Transform debugParent;

    [SerializeField]
    private Transform obstacleParent;

    private Dictionary<Vector2, GameObject> debugGrid = new();
    private Dictionary<Vector2, HashSet<Vector2>> hasConnection = new();

    private const float NAVMESH_HEIGHT = 100.0f;
    private const float MAX_HEIGHT_DIFFERENCE = 5f;
    public float NavmeshHeight { get { return NAVMESH_HEIGHT; } }
    public Vector2 MinimumBoundary => minBound;
    public Vector2 MaximumBoundary => maxBound;
    public void Awake()
    {
        GenerateNavMesh();
    }

    //Generate edges between nodes, and don't generate edges if there is a big height difference

    /// <summary>
    /// Clears and reconstructs the navmesh
    /// </summary>
    private void GenerateNavMesh()
    {
        //Clear old debug objects
        for (int i = (int)minBound.x; i < maxBound.x; i += debugResolution)
        {
            for (int j = (int)minBound.y; j < maxBound.y; j += debugResolution)
            {
                if (debugGrid.TryGetValue(new Vector2(i, j), out GameObject debugObject))
                {
                    Destroy(debugObject);
                }
            }
        }

        //Clear old navmesh
        debugGrid.Clear();
        navMeshGrid.Clear();
        hasConnection.Clear();

        Vector2 previousKey = new(0,0);

        //Ray-cast and generate navmesh
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                Vector2 key = new(i, j);

                //Check for obstacle layer
                if (Physics.Raycast(new(i, NAVMESH_HEIGHT, j), Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT))
                {
                    if (hitInfo.collider.gameObject.layer == obstacleLayer)
                    {
                        //Add a negative weight for an impassible object
                        navMeshGrid.Add(key, new TerrainData(new(i, hitInfo.point.y, j), 0, false));
                        continue;
                    }

                    AddConnections(key, hitInfo.point.y);

                    //Set cost as the height of the point of contact
                    navMeshGrid.Add(key, new TerrainData(new(i, hitInfo.point.y, j), hitInfo.point.y, true));
                }
                previousKey = key;
            }
        }

        //Add debug visuals
        EnableDebugMode();
    }

    /// <summary>
    /// Adds connections for all four neighbors
    /// </summary>
    /// <param name="key">The current position on navmesh</param>
    /// <param name="hitPointHeight">The height we are comparing to</param>
    private void AddConnections(Vector2 key, float hitPointHeight)
    {
        for (int i = (int)key.x -1; i < (int)key.x + 2; ++i)
        {
            for (int j = (int)key.y -1; j < (int)key.y + 2; ++j)
            {
                if (i == key.x && j == key.y)
                    continue;

                var neighborKey = new Vector2(i, j);

                //Check difference in heights, if the difference is too great,
                //then the AI can't path find
                if (navMeshGrid.TryGetValue(neighborKey, out TerrainData data))
                {
                    float heightDifference = Mathf.Abs(data.Position.y - hitPointHeight);
                    if (heightDifference < MAX_HEIGHT_DIFFERENCE)
                    {
                        //Access hash set if it already has one
                        if (hasConnection.ContainsKey(key))
                        {
                            if (hasConnection.TryGetValue(key, out HashSet<Vector2> set))
                            {
                                set.Add(neighborKey);
                            }
                        }
                        //Otherwise create new hash set and insert it
                        else
                        {
                            var set = new HashSet<Vector2>();
                            set.Add(neighborKey);
                            hasConnection.TryAdd(key, set);
                        }
                    }
                }

            }
        }
    }

    /// <summary>
    /// Shows a debug visual of the navmesh
    /// </summary>
    public void EnableDebugMode()
    {
        if (enableDebug)
        {
            for (int i = (int)minBound.x; i < maxBound.x; i += debugResolution)
            {
                for (int j = (int)minBound.y; j < maxBound.y; j += debugResolution)
                {
                    Vector2 key = new(i, j);

                    if (navMeshGrid.TryGetValue(key, out TerrainData data))
                    {
                        if (!data.IsWalkable)
                        {
                            continue;
                        }

                        //Add debug object
                        debugGrid.Add(key, Instantiate(debugPrefab, data.Position, Quaternion.identity, debugParent));
                    }
                }
            }
        }
        else
        {
            for (int i = (int)minBound.x; i < maxBound.x; i += debugResolution)
            {
                for (int j = (int)minBound.y; j < maxBound.y; j += debugResolution)
                {
                    Vector2 key = new(i, j);
                    if (debugGrid.TryGetValue(key, out GameObject debugObject))
                    {
                        Destroy(debugObject);
                        debugGrid.Remove(key);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds a random obstacle to the navmesh
    /// </summary>
    public void PlaceRandomObject()
    {
        Vector3 position = new Vector3(Random.Range(minBound.x, maxBound.x), 0, Random.Range(minBound.y, maxBound.y));
        Instantiate(obstaclePrefab, position, Quaternion.identity, obstacleParent);
        GenerateNavMesh();
    }

    /// <summary>
    /// Destroys all obstacles and resets the navmesh
    /// </summary>
    public void ResetNavmesh()
    {
        for (int i = 0; i < obstacleParent.childCount; ++i)
        {
            Destroy(obstacleParent.GetChild(i).gameObject);
        }
        GenerateNavMesh();
    }
    /// <summary>
    /// Generate a 3-Dimensional path from a list of 2D coordinates
    /// </summary>
    /// <param name="list2D"></param>
    /// <returns>A 3D path</returns>
    public List<Vector3> TransformPathTo3D(ref List<Vector2> list2D)
    {
        List<Vector3> result = new();
        foreach (var item in list2D)
        {
            if (navMeshGrid.TryGetValue(item, out TerrainData data))
            {
                result.Add(data.Position);
                continue;
            }
            return result;
        }
        return result;
    }

    /// <summary>
    /// Check the connections map for connection between nodes
    /// </summary>
    /// <param name="currentPos">The algorithms current position</param>
    /// <param name="neighborPos">The algorithms next position</param>
    /// <returns>Whether the agent can traverse here</returns>
    public bool CheckForConnection(Vector2 currentPos, Vector2 neighborPos)
    {
        if (hasConnection.TryGetValue(currentPos, out HashSet<Vector2> set))
        {
            if (set.Contains(neighborPos))
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        for (int i = (int)minBound.x; i < maxBound.x; i += debugResolution)
        {
            for (int j = (int)minBound.y; j < maxBound.y; j += debugResolution)
            {
                Vector2 key = new(i, j);

                if (!navMeshGrid.TryGetValue(key, out TerrainData data))
                    continue;

                if (hasConnection.TryGetValue(key, out HashSet<Vector2> connections))
                {
                    foreach (var connection in connections)
                    {
                        if (navMeshGrid.TryGetValue(connection, out TerrainData connectionData))
                            Debug.DrawLine(data.Position, connectionData.Position, Color.red);
                    }
                }
            }
        }
    }
}

public class TerrainData
{
    public TerrainData(Vector3 pos, float cost, bool walkable)
    {
        Position = pos;
        MovementCost = cost;
        IsWalkable = walkable;
    }

    public Vector3 Position;
    public float MovementCost;
    public bool IsWalkable;
}
