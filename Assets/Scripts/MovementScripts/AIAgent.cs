using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavmeshGeneration))]
public class AIAgent : MonoBehaviour
{
    [Header("Agent Settings")]

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float rotationSpeed = 2f;

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

    [SerializeField]
    private float recalculatePathDistance = 2f;

    private IPathfinder pathfindingAlgorithm;
    private PathSmoothing pathSmoothingAlgorithm;

    private LineRenderer lineRenderer;

    private List<Vector3> path = new();

    private const float CAPSULE_OFFSET = 1f;
    private const float DEBUG_LINE_OFFSET = 0.5f;
    private void Start()
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

        pathfindingAlgorithm.RunPathfinding(RoundVector(transform.position), RoundVector(target.position));
    }

    /// <summary>
    /// Resets the agents position
    /// </summary>
    public void ResetAgent()
    {
        transform.position = new(0, CAPSULE_OFFSET, 0);
    }

    private void Update()
    {
        //Issue: agent position sometimes moves off navmesh, and must move to nearest navmesh point to proceed again
        path = pathfindingAlgorithm.RunPathfinding(RoundVector(transform.position), RoundVector(target.position));

        if (path == null || path.Count <= 1)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        if (smoothingEnabled)
        {
            path = pathSmoothingAlgorithm.SmoothPath(path, smoothingSegments);
        }

        if (enableDebug)
        {
            lineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; ++i)
            {
                lineRenderer.SetPosition(i, new(path[i].x, path[i].y + DEBUG_LINE_OFFSET, path[i].z));
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }

        Vector3 moveTo = new(path[0].x, path[0].y + CAPSULE_OFFSET, path[0].z);
        
        Vector3 direction = moveTo - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed); 

        transform.position = Vector3.MoveTowards(transform.position, moveTo, agentSpeed * Time.deltaTime);
    }

    private Vector3Int RoundVector(Vector3 vector)
    {
        return new Vector3Int(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y),
            Mathf.RoundToInt(vector.z)
            );
    }
}

public interface IPathfinder
{
    public List<Vector3> RunPathfinding(Vector3Int startPos, Vector3Int target);
}
