using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavmeshGeneration))]
public class AIAgent : MonoBehaviour
{
    //Handle target and agent related stats:
    //like speed, and whatnot 

    [Header("Agent Settings")]

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float agentSpeed = 1f;

    [SerializeField]
    private bool enableDebug;

    private IPathfinder pathfindingAlgorithm;

    private NavmeshGeneration navmesh;

    private const float CAPSULE_OFFSET = 1f;
    private const float GROUND_RAY_DIST = 7f;

    private void Awake()
    {
        if (!transform.TryGetComponent(out pathfindingAlgorithm))
        {
            throw new NullReferenceException("Please add a pathfinding algorithm to the AI Agent!");
        }

        navmesh = GetComponent<NavmeshGeneration>();
    }


    private void Update()
    {
        //Get path to target
        var path = pathfindingAlgorithm.GetPath(RoundVector(transform.position), RoundVector(target.position));

        if (path == null) return;

        navmesh.navMeshGrid.TryGetValue(path[0], out TerrainData data);

        if (enableDebug)
        {
            Vector3 previousPosition = data.Position;
            foreach (Vector2 position in path)
            {
                navmesh.navMeshGrid.TryGetValue(position, out TerrainData nextPos);
               Debug.DrawLine(previousPosition, nextPos.Position, Color.red);
               previousPosition = nextPos.Position;
            }
        }

        //Raycast down and get point
        //Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, GROUND_RAY_DIST);

        Vector3 moveTo = new Vector3(data.Position.x, data.Position.y + CAPSULE_OFFSET, data.Position.z);
        transform.position = Vector3.MoveTowards(transform.position, moveTo, agentSpeed * Time.deltaTime);
    }

    private Vector3 RoundVector(Vector3 vector)
    {
        return new Vector3(
            Mathf.FloorToInt(vector.x),
            Mathf.FloorToInt(vector.y),
            Mathf.FloorToInt(vector.z)
            );
    }

}

public interface IPathfinder
{
    List<Vector2> GetPath(Vector2 startPos, Vector2 target);
}
