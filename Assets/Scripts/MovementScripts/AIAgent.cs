using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        if (!transform.TryGetComponent(out pathfindingAlgorithm))
        {
            throw new NullReferenceException("Please add a pathfinding algorithm to the AI Agent!");
        }
    }


    private void Update()
    {
        //Get path to target
        var path = pathfindingAlgorithm.GetPath(RoundVector(transform.position), RoundVector(target.position));

        if (path == null) return;

        if (enableDebug)
        {
            Vector3 previousPosition = new Vector3(path[0].x, 0, path[0].z);
            foreach (Vector3 position in path)
            {
               position.Set(position.x, 0, position.z);
               Debug.DrawLine(previousPosition, position, Color.red);
               previousPosition = position;
            }
        }

        Vector3 moveTo = new Vector3(path[0].x, 1.25f, path[0].z);
        transform.position = Vector3.MoveTowards(transform.position, moveTo, agentSpeed * Time.deltaTime);
    }

    private Vector3 RoundVector(Vector3 vector)
    {
        return new Vector3(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y),
            Mathf.RoundToInt(vector.z)
            );
    }

}

public interface IPathfinder
{
    List<Vector3> GetPath(Vector3 startPos, Vector3 target);
}
