using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(NavmeshGeneration))]
public class AStarPathfinding : MonoBehaviour, IPathfinder
{
    [Tooltip("The range at which the agent has to reach to be the goal")]
    [SerializeField]
    private float stopRange = 2f;

    [Range(1f, 15000f)]
    [SerializeField]
    private int maxIterations;

    private List<Vector3> _path = new();

    private NavmeshGeneration navmesh;

    private const float DEFAULT_MOVEMENT_COST = 1f;

    private Coroutine instance = null;
    private void Start()
    {
        navmesh = GetComponent<NavmeshGeneration>();
    }

    public List<Vector3> RunPathfinding(Vector3Int startPos, Vector3Int target)
    {
        instance ??= StartCoroutine(GetPath(startPos, target));

        return _path;
    }

    private IEnumerator GetPath(Vector3Int startPos, Vector3Int target)
    {
        try
        {
            Dictionary<WeightedPosition, float> costSoFar = new();
            Dictionary<Vector3, Vector3> cameFrom = new();
            PriorityQueue<WeightedPosition, float> frontier = new();
            HashSet<Vector3> frontierSet = new();
            HashSet<Vector3> visited = new();

            //Exit early if target is unreachable
            if (ValidatePosition(target, ref visited, ref frontierSet) == null)
            {
                instance = null;
                yield return null;
            }
/*
            if (ValidatePosition(startPos, ref visited, ref frontierSet) == null)
            {
                _path.Clear();

            }*/

            //Initialize start position
            WeightedPosition startWeight = new(0.0f, startPos);
            frontier.Enqueue(startWeight, 0);
            costSoFar.Add(startWeight, 0);
            frontierSet.Add(startWeight.Position);

            WeightedPosition endPoint = null;

            int iterations = 0;

            //While frontier has elements
            while (frontier.Count > 0)
            {
                iterations++;

                if (iterations > maxIterations)
                {
                    instance = null;
                    Debug.Log("Timed out!");
                    yield return null;
                }

                var currentPoint = frontier.Dequeue();
                visited.Add(currentPoint.Position);

                //If current point is end point
                if (Vector3.Distance(currentPoint.Position, target) < stopRange)
                {
                    endPoint = currentPoint;
                    break;
                }

                var neighbors = GetVisitableNeighbors(currentPoint.Position, ref visited, ref frontierSet);
                if (neighbors.Count == 0)
                {
                    continue;
                }

                foreach (var neighbor in neighbors)
                {
                    //Calculate new cost of travel
                    float newCost = costSoFar[currentPoint] + DEFAULT_MOVEMENT_COST + Mathf.Abs(neighbor.Weight - currentPoint.Position.y);

                    //Check if the neighbor exists already in the frontier and if it does, check its old cost
                    if (!frontierSet.Contains(neighbor.Position) || costSoFar[neighbor] > newCost)
                    {
                        costSoFar[neighbor] = newCost;

                        //Use nodes in graph, not position 
                        float priority = newCost + Heuristic(neighbor.Position, target);

                        //Update came from dictionary
                        cameFrom[neighbor.Position] = currentPoint.Position;
                        //Add neighbor to frontier
                        frontier.Enqueue(neighbor, priority);
                        frontierSet.Add(neighbor.Position);
                    }

                }
            }

            //If we find an end point
            if (endPoint != null)
            {
                List<Vector3> path = new();

                path.Add(endPoint.Position);
                cameFrom.TryGetValue(endPoint.Position, out Vector3 current);

                if (current == null)
                {
                    instance = null;
                    yield return null;
                }

                //Recreate path
                while (current != startPos)
                {
                    path.Add(current);

                    //If we can't get pos, break the loop
                    if (!cameFrom.TryGetValue(current, out current))
                    {
                        break;
                    }
                }
                //Reverse path
                path.Reverse();
                _path = path;
            }
            else
            {
                //Empty pathfinding path if no goal is found
                _path.Clear();
            }

            instance = null;
            yield return null;
        }
        finally
        {
            instance = null;
        }
    }

    /// <summary>
    /// Iterate through all connections to a point on the navmesh, check if they are valid
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="visited"></param>
    /// <param name="frontierSet"></param>
    /// <returns></returns>
    private List<WeightedPosition> GetVisitableNeighbors(Vector3 pos, ref HashSet<Vector3> visited, ref HashSet<Vector3> frontierSet)
    {
        List<WeightedPosition> neighbors = new();

        //Ask navmesh what current points we can reach
        var terrainData = navmesh.GetNavmeshValue(new(pos.x, pos.z), pos.y);
        foreach (Vector2 neighbor in terrainData.GetConnections())
        {
            TerrainData neighbor3D = navmesh.GetNavmeshValue(neighbor, pos.y);
            if (neighbor3D == null)
                continue;

            var position = ValidatePosition(RoundVector(neighbor3D.Position), ref visited, ref frontierSet);
            if (position != null)
            {
                neighbors.Add(position);
            }
        }
        return neighbors;
    }
    /// <summary>
    /// Validates that a neighbor meets certain criteria 
    /// </summary>
    /// <param name="neighbor"></param>
    /// <returns>A weighted position if valid, otherwise null</returns>
    private WeightedPosition ValidatePosition(Vector3Int neighbor, ref HashSet<Vector3> visited, ref HashSet<Vector3> frontierSet)
    {
        Vector2 neighborKey = new(neighbor.x, neighbor.z);

        //Get the terrain data from navmesh
        var point = navmesh.GetNavmeshValue(neighborKey, neighbor.y);

        if (point == null)
            return null;

        if (!point.IsWalkable)
            return null;

        if (frontierSet.Contains(neighbor))
            return null;

        if (visited.Contains(neighbor))
            return null;

        if (!IsInBounds(neighbor))
            return null;

        return new WeightedPosition(point.MovementCost, neighbor);
    }

    private bool IsInBounds(Vector3 pos)
    {
        if (pos.x > navmesh.MinimumBoundary.x && pos.z > navmesh.MinimumBoundary.y
            && pos.x < navmesh.MaximumBoundary.x && pos.z < navmesh.MaximumBoundary.y)
            return true;
        return false;
    }

    public float Heuristic(Vector3Int start, Vector3Int end)
    {
        //Use vector3 int

        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y) + Mathf.Abs(start.z - end.z);
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

public class WeightedPosition
{
    public WeightedPosition(float weight, Vector3Int pos)
    {
        Weight = weight;
        Position = pos;
    }

    public float Weight;
    public Vector3Int Position;
}