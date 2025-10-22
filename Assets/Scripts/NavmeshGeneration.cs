using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavmeshGeneration : MonoBehaviour
{
    public Dictionary<Vector3, WeightedPosition> navMeshGrid = new();

    [SerializeField]
    private Vector2 minBound;
    [SerializeField]
    private Vector2 maxBound;
    [SerializeField]
    private LayerMask obstacleLayer;
    [SerializeField]
    private bool enableDebug;

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
                Vector3 currentPosition = new(i, NAVMESH_HEIGHT, j);

                //Check for obstacle layer
                if (Physics.Raycast(currentPosition, Vector3.down, out RaycastHit hitInfo, NAVMESH_HEIGHT, obstacleLayer))
                {
                    //Add a negative weight for an impassible object
                    navMeshGrid.Add(currentPosition, new WeightedPosition(-1, currentPosition));
                }
                else
                {
                    float weight = hitInfo.point.y;

                    if (weight <= 0)
                        weight = 1;

                    //Set cost as the height of the point of contact
                    navMeshGrid.Add(currentPosition, new WeightedPosition(weight, currentPosition));
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
                    if (navMeshGrid.TryGetValue(new Vector3(i, NavmeshHeight, j), out WeightedPosition position))
                    {
                        if (position.Weight < 0)
                        {
                            return;
                        }
                    }

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(new Vector3(i, 0, j), debugRadius);
                }
            }
        }
    }
}
