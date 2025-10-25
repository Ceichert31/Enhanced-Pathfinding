using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

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
    private Transform debugParent;

    private Dictionary<Vector2, GameObject> debugGrid = new();

    private const int MAX_DEBUG_OBJECTS = 1000;
    private const float NAVMESH_HEIGHT = 100.0f;

    public float NavmeshHeight { get { return NAVMESH_HEIGHT; } }
    public Vector2 MinimumBoundary => minBound;
    public Vector2 MaximumBoundary => maxBound;
    public void Awake()
    {
        for (int i = (int)minBound.x; i < maxBound.x; ++i)
        {
            for (int j = (int)minBound.y; j < maxBound.y; ++j)
            {
                Vector2 key = new(i, j);

                //Add debug object
                debugGrid.Add(key, Instantiate(debugPrefab,debugParent));

                //Check for obstacle layer
                if (Physics.Raycast(new(i, NAVMESH_HEIGHT, j), Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT))
                {
                    if (hitInfo.collider.gameObject.layer == obstacleLayer)
                    {
                        //Add a negative weight for an impassible object
                        navMeshGrid.Add(key, new TerrainData(new(i, hitInfo.point.y, j), 0, false));
                        continue;
                    }
                    //Set cost as the height of the point of contact
                    navMeshGrid.Add(key, new TerrainData(new(i, hitInfo.point.y, j), hitInfo.point.y, true));
                }
            }
        }
    }

    private void Update()
    {
        if (enableDebug)
        {
            for (int i = (int)minBound.x; i < maxBound.x; i += debugResolution)
            {
                for (int j = (int)minBound.y; j < maxBound.y; j += debugResolution)
                {
                    if (debugGrid.TryGetValue(new Vector2(i, j), out GameObject debugObject) && navMeshGrid.TryGetValue(new Vector2(i, j), out TerrainData data))
                    {
                        if (!data.IsWalkable)
                        {
                            continue;
                        }
                        debugObject.transform.position = data.Position;
                        debugObject.SetActive(true);
                    }
                }
            }
        }
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
