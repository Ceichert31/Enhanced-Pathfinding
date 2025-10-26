using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavmeshGeneration))]
public class AIAgent : MonoBehaviour
{
    [Header("Agent Settings")]

    [SerializeField]
    private Transform target;

    public float AgentSpeed
    {
        get => agentSpeed; set => agentSpeed = value;
    }
    [SerializeField]
    private float agentSpeed = 1f;

    public bool SmoothingEnabled
    {
        get => smoothingEnabled; set => smoothingEnabled = value;
    }
    [SerializeField]
    private bool smoothingEnabled = false;

    public int SmoothingSegments
    {
        get => smoothingSegments; set => smoothingSegments = value;
    }

    [SerializeField]
    private int smoothingSegments = 3;
    public bool EnableDebug
    {
        get => enableDebug; set => enableDebug = value;
    }
    [SerializeField]
    private bool enableDebug;

    private IPathfinder pathfindingAlgorithm;
    private PathSmoothing pathSmoothingAlgorithm;

    private LineRenderer lineRenderer;

    private const float CAPSULE_OFFSET = 1f;
    private void Awake()
    {
        if (!transform.TryGetComponent(out pathfindingAlgorithm))
        {
            throw new NullReferenceException("Please add a pathfinding algorithm to the AI Agent!");
        }
        if (!transform.TryGetComponent(out pathSmoothingAlgorithm))
        {
            throw new NullReferenceException("Please add a path smoothing algorithm to the AI Agent!");
        }
        lineRenderer = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Resets the agents position
    /// </summary>
    public void ResetAgent()
    {
        AgentSpeed = 0f;
        transform.position = new(0, CAPSULE_OFFSET, 0);
    }

    private void Update()
    {
        //Get path to target
        var path = pathfindingAlgorithm.GetPath(RoundVector(new(transform.position.x, transform.position.z)), RoundVector(new(target.position.x, target.position.z)));

        if (path == null) return;

        if (smoothingEnabled)
        {
            path = pathSmoothingAlgorithm.SmoothPath(path, smoothingSegments);
        }

        if (enableDebug)
        {
            lineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; ++i)
            {
                lineRenderer.SetPosition(i, path[i]);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }

        Vector3 moveTo = new(path[0].x, path[0].y + CAPSULE_OFFSET, path[0].z);
        transform.position = Vector3.MoveTowards(transform.position, moveTo, agentSpeed * Time.deltaTime);
    }

    private Vector3 RoundVector(Vector2 vector)
    {
        return new Vector2(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y)
            );
    }

}

public interface IPathfinder
{
    List<Vector3> GetPath(Vector2 startPos, Vector2 target);
}
