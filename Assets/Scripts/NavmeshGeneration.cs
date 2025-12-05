using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NavmeshGeneration : MonoBehaviour
{
    public Dictionary<Vector2, List<TerrainData>> navMeshGrid = new();

    [SerializeField]
    private Vector2 minBound;
    [SerializeField]
    private Vector2 maxBound;
    [SerializeField]
    private int obstacleLayer;
    [SerializeField]
    private LayerMask hitLayer;
    public bool EnableDebug
    {
        get => enableDebug; set => enableDebug = value;
    }
    [SerializeField]
    private bool enableDebug;

    [Range(3f, 5f)]
    [SerializeField]
    private int debugResolution = 5;
    [Range(1, 5)]
    [SerializeField]
    private int raycastLevels = 3;

    [Tooltip("The maximum height the navmesh can generate a connection between")]
    [SerializeField]
    private float maxTraversableHeight = 0.4f;

    [SerializeField]
    private bool enableDiagonals = true;

    [SerializeField]
    private GameObject debugPrefab;
    [SerializeField]
    private GameObject obstaclePrefab;

    [SerializeField]
    private Transform debugParent;

    [SerializeField]
    private Transform obstacleParent;

    private Dictionary<Vector2, GameObject> debugGrid = new();

    private const float NAVMESH_HEIGHT = 100.0f;
    private const float DIAGONAL_COST = 1f;
    public float NavmeshHeight { get { return NAVMESH_HEIGHT; } }
    public Vector2 MinimumBoundary => minBound;
    public Vector2 MaximumBoundary => maxBound;
    public void Awake()
    {
        GenerateNavMesh();
    }

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

        //Ray-cast and generate navmesh
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                Vector2 key = new(i, j);

                //Check for obstacle layer
                if (Physics.Raycast(new(i, NAVMESH_HEIGHT, j), Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT, hitLayer))
                {
                  /*  if (hitInfo.collider.gameObject.CompareTag("obstacle"))
                    {
                        AddToNavmesh(key, new TerrainData(new(i, hitInfo.point.y, j), 0, false));
                        continue;
                    }*/

                    //Set cost as the height of the point of contact
                    AddToNavmesh(key, new TerrainData(new(i, hitInfo.point.y, j), hitInfo.point.y, true));
                    RecursiveCast(hitInfo.point, raycastLevels, key);
                }
            }
        }

        //Create connections after navmesh has been generated
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                Vector2 key = new(i, j);

                if (navMeshGrid.TryGetValue(key, out List<TerrainData> dataList))
                {
                    foreach (var point in dataList)
                    {
                        AddConnections(key, point.Position.y);
                    }
                }
            }
        }

        //Add debug visuals
        EnableDebugMode();
    }

    /// <summary>
    /// Shoots a raycast down from a previous raycast's hitpoint recursively
    /// </summary>
    /// <param name="castOrigin">Where the last cast hit something</param>
    private void RecursiveCast(Vector3 castOrigin, int castsLeft, Vector2 key)
    {
        if (castsLeft < 0)
            return;

        if (Physics.Raycast(castOrigin, Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT, hitLayer))
        {
            if (Mathf.Abs(castOrigin.y - hitInfo.point.y) < maxTraversableHeight)
                return;

            //Set cost as the height of the point of contact
            AddToNavmesh(key, new TerrainData(new(key.x, hitInfo.point.y, key.y), hitInfo.point.y, true));
            RecursiveCast(hitInfo.point, --castsLeft, key);
        }
    }

    /// <summary>
    /// Adds new terrain data to the navmesh hashmap
    /// </summary>
    /// <param name="key">The 2D position</param>
    /// <param name="data">The terrain data at this position</param>
    private void AddToNavmesh(Vector2 key, TerrainData data)
    {
        //Get list of data to add to end of
        if (navMeshGrid.TryGetValue(key, out List<TerrainData> list))
        {
            list.Add(data);
        }
        //Otherwise, add new list of data
        else
        {
            List<TerrainData> newData = new();
            newData.Add(data);
            navMeshGrid.Add(key, newData);
        }
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
                if (!enableDiagonals)
                {
                    //Skip adding diagonal neighbors
                    if (i != key.x && j != key.y)
                        continue;
                }

                //Skip adding self
                if (i == key.x && j == key.y)
                    continue;

                var neighborKey = new Vector2(i, j);

                //Check difference in heights, if the difference is too great,
                //then the AI can't path find
                var point = GetNavmeshValue(neighborKey, hitPointHeight);

                if (point == null) continue;

                //If Diagonal, set weight higher
                if (i != key.x && j != key.y)
                {
                    point.MovementCost += DIAGONAL_COST;
                }

                float heightDifference = Mathf.Abs(point.Position.y - hitPointHeight);

                //Add twice for two-way path
                AddPointToHashSet(key, neighborKey, heightDifference, hitPointHeight);
            }
        }
    }

    /// <summary>
    /// Checks the height difference of two points
    /// and creates a connection between them
    /// </summary>
    /// <param name="key">The 2D position</param>
    /// <param name="neighborKey">The neighbors 2D position</param>
    /// <param name="heightDifference">The difference in height between neighbor and current point</param>
    private void AddPointToHashSet(Vector2 key, Vector2 neighborKey, float heightDifference, float height)
    {
        if (key == neighborKey) return;

        if (heightDifference < maxTraversableHeight)
        {
            //Access hash set if it already has one
            var point = GetNavmeshValue(key, height);
            var neighborPoint = GetNavmeshValue(neighborKey, height);
            if (point == null || neighborPoint == null) return;

            //Check if wall blocks the connection
            if (Physics.Linecast(point.Position + Vector3.up * 0.1f,
                                neighborPoint.Position + Vector3.up * 0.1f, out RaycastHit info,
                                hitLayer))
            {
                if (info.transform.gameObject.CompareTag("obstacle"))
                    return;
            }

            //Add neighbor to connections list
            point.GetConnections().Add(neighborKey);
        }
    }

    /// <summary>
    /// Shows a debug visual of the navmesh
    /// </summary>
    public void EnableDebugMode()
    {
        if (enableDebug)
        {
            for (int i = (int)minBound.x; i < maxBound.x; i += 1)
            {
                for (int j = (int)minBound.y; j < maxBound.y; j += 1)
                {
                    Vector2 key = new(i, j);

                    if (!navMeshGrid.TryGetValue(key, out var pointList))
                        continue;

                    foreach (var point in pointList)
                    {
                        if (point == null) continue;

                        if (Vector3.Distance(Camera.main.transform.position, point.Position) > 8)
                            continue;

                        if (!point.IsWalkable)
                        {
                            continue;
                        }

                        debugGrid.TryAdd(key, Instantiate(debugPrefab, point.Position, Quaternion.identity, debugParent));
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
    /// Return the point with a connection
    /// </summary>
    /// <param name="key"></param>
    public TerrainData GetNavmeshValue(Vector2 key, float level)
    {
        if (navMeshGrid.TryGetValue(key, out List<TerrainData> terrainDataAtThisPoint))
        {
            TerrainData closest = null;
            float distance = Mathf.Infinity;

            //Check all data points for connection, return first connection
            foreach (var point in terrainDataAtThisPoint)
            {
                //Calculate difference between level and point's position
                float currentHeightDifference = Mathf.Abs(level - point.Position.y);

                //If closer to point than previously, set closest 
                if (currentHeightDifference < distance)
                {
                    distance = currentHeightDifference;
                    closest = point;
                }
            }
            return closest;
        }
        return null;
    }

    /// <summary>
    /// Check the connections map for connection between nodes
    /// </summary>
    /// <param name="currentPos">The algorithms current position</param>
    /// <param name="neighborPos">The algorithms next position</param>
    /// <returns>Whether the agent can traverse here</returns>
    public bool CheckForConnection(Vector2 currentPos, Vector2 neighborPos, float level)
    {
        if (currentPos == neighborPos) return true;

        var data = GetNavmeshValue(currentPos, level);

        if (data == null) return false;

        return data.ContainsKey(neighborPos);
    }

    private void Update()
    {
        RenderConnections();
    }

    /// <summary>
    /// Renders lines between every connection
    /// </summary>
    private void RenderConnections()
    {
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                Vector2 key = new(i, j);

                if (!navMeshGrid.TryGetValue(key, out List<TerrainData> data))
                    continue;

                foreach (var point in data)
                {
                    if (Vector3.Distance(Camera.main.transform.position, point.Position) > 8)
                        continue;

                    var connectionList = point.GetConnections();
                   
                    foreach (var connection in connectionList)
                    {
                        var closestPoint = GetNavmeshValue(connection, point.Position.y);
                        Debug.DrawLine(point.Position, closestPoint.Position, Color.red);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Holds data relating to navmesh 
/// </summary>
public class TerrainData : IEquatable<TerrainData>
{
    public TerrainData(Vector3 pos, float cost, bool walkable)
    {
        Position = pos;
        MovementCost = cost;
        IsWalkable = walkable;
        Connections = new();
    }

    public Vector3 Position;
    public float MovementCost;
    public bool IsWalkable;

    private HashSet<Vector2> Connections;

    public bool ContainsKey(Vector2 key)
    {
        return Connections.Contains(key);
    } 

    public ref HashSet<Vector2> GetConnections()
    {
        return ref Connections;
    }

    public bool Equals(TerrainData other)
    {
        return Position == other.Position;
    }
}
