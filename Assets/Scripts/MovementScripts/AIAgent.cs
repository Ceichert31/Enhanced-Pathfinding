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
        var path = pathfindingAlgorithm.GetPath(RoundVector(new(transform.position.x, transform.position.z)), RoundVector(new(target.position.x, target.position.z)));

        if (path == null) return;

        if (enableDebug)
        {
            Vector3 previousPosition = path[0];
            foreach (Vector3 position in path)
            {
               Debug.DrawLine(previousPosition, position, Color.red);
               previousPosition = position;
            }
        }

        //Raycast down and get point
        //Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, GROUND_RAY_DIST);

        Vector3 moveTo = new Vector3(path[0].x, path[0].y + CAPSULE_OFFSET, path[0].z);
        transform.position = Vector3.MoveTowards(transform.position, moveTo, agentSpeed * Time.deltaTime);
    }

    private Vector3 RoundVector(Vector2 vector)
    {
        return new Vector3(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y)
            );
    }

}

public interface IPathfinder
{
    List<Vector3> GetPath(Vector2 startPos, Vector2 target);
}
