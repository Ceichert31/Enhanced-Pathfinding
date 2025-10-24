using System.Collections;
using System.Collections.Generic;
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
    [SerializeField]
    private bool enableDebug;

    [Range(3f, 5f)]
    [SerializeField]
    private int debugResolution = 5;
    [SerializeField]
    private float debugRadius = 0.5f;

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

                //Check for obstacle layer
                if (Physics.Raycast(new(i, NAVMESH_HEIGHT, j), Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT + 3))
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
    private void OnDrawGizmos()
    {
        if (enableDebug)
        {
            for (int i = (int)minBound.x; i < maxBound.x; i += debugResolution)
            {
                for (int j = (int)minBound.y; j < maxBound.y; j += debugResolution)
                {
                    if (navMeshGrid.TryGetValue(new Vector2(i, j), out TerrainData data))
                    {
                        if (!data.IsWalkable)
                        {
                            continue;
                        }
                    }

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(data.Position, debugRadius);
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
