using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(NavmeshGeneration))]
public class AStarPathfinding : MonoBehaviour, IPathfinder
{
    private NavmeshGeneration navmesh;

    private void Start()
    {
        navmesh = GetComponent<NavmeshGeneration>();
    }

    public List<Vector3> GetPath(Vector2 startPos, Vector2 target)
    {
        Dictionary<WeightedPosition, float> costSoFar = new();
        Dictionary<Vector2, Vector2> cameFrom = new();
        PriorityQueue<WeightedPosition, float> frontier = new();
        HashSet<Vector2> frontierSet = new();
        HashSet<Vector2> visited = new();

        //Exit early if target is unreachable
        if (ValidatePosition(target, ref visited, ref frontierSet) == null)
        {
            return null;
        }

        //Initialize start position
        WeightedPosition startWeight = new(0.0f, startPos);
        frontier.Enqueue(startWeight, 0);
        costSoFar.Add(startWeight, 0);
        frontierSet.Add(startWeight.Position);

        WeightedPosition endPoint = null;

        int nodesProcessed = 0;
        int maxNodesToProcess = 11000;

        //While frontier has elements
        while (frontier.Count > 0)
        {
            var currentPoint = frontier.Dequeue();
            visited.Add(currentPoint.Position);

            nodesProcessed++;

            if (nodesProcessed > maxNodesToProcess)
            {
                Debug.LogWarning($"Pathfinding gave up after processing {nodesProcessed} nodes. Target may be unreachable.");
                return null;
            }

            //If current point is end point
            if (currentPoint.Position == target)
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
                float newCost = costSoFar[currentPoint] + neighbor.Weight;

                //Check if the neighbor exists already in the frontier and if it does, check its old cost
                if (!frontierSet.Contains(neighbor.Position) || costSoFar[neighbor] > newCost)
                {
                    costSoFar[neighbor] = newCost;

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
        //Convert from 2D space to 3D space with navmesh
        if (endPoint != null)
        {
            List<Vector2> path = new();

            path.Add(endPoint.Position);
            cameFrom.TryGetValue(endPoint.Position, out Vector2 current);

            if (current == null)
            {
                return null;
            }

            //Recreate path
            while (current != startPos)
            {
                if (path.Count > 1000)
                {
                    Debug.LogError($"Path reconstruction exceeded 1000 nodes! Current: {current}, Start: {startPos}");
                    return null;
                }

                path.Add(current);

                //If we can't get pos, break the loop
                if (!cameFrom.TryGetValue(current, out current))
                {
                    break;
                } 
            }
            //Reverse path and convert to 3D
            path.Reverse();
            return navmesh.TransformPathTo3D(ref path);
        }

        return null;
    }

    private List<WeightedPosition> GetVisitableNeighbors(Vector2 pos, ref HashSet<Vector2> visited, ref HashSet<Vector2> frontierSet)
    {
        List<WeightedPosition> neighbors = new();

        Vector2 northPos = new(pos.x+1, pos.y);
        Vector2 eastPos = new(pos.x-1, pos.y);
        Vector2 southPos = new(pos.x, pos.y+1);
        Vector2 westPos = new(pos.x, pos.y-1);

        var position = ValidatePosition(northPos, ref visited, ref frontierSet);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(eastPos, ref visited, ref frontierSet);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(southPos, ref visited, ref frontierSet);
        if (position != null)
        {
            neighbors.Add(position);
        }
        position = ValidatePosition(westPos, ref visited, ref frontierSet);
        if (position != null)
        {
            neighbors.Add(position);
        }

        return neighbors;
    }
    /// <summary>
    /// Validates that a neighbor meets certain criteria 
    /// </summary>
    /// <param name="neighbor"></param>
    /// <returns>A weighted position if valid, otherwise null</returns>
    private WeightedPosition ValidatePosition(Vector2 neighbor, ref HashSet<Vector2> visited, ref HashSet<Vector2> frontierSet)
    {
        //Get navmesh data and check is the neighbor is valid
        if (navmesh.navMeshGrid.TryGetValue(neighbor, out TerrainData data))
        {
            if (!data.IsWalkable)
                return null;

            if (frontierSet.Contains(neighbor))
                return null;

            if (visited.Contains(neighbor))
                return null;

            if (!IsInBounds(neighbor))
                return null;

            return new WeightedPosition(data.MovementCost, new(data.Position.x, data.Position.z));
        }
        return null;
    }

    private bool IsInBounds(Vector2 pos)
    {
        if (pos.x > navmesh.MinimumBoundary.x && pos.y > navmesh.MinimumBoundary.y
            && pos.x < navmesh.MaximumBoundary.x && pos.y < navmesh.MaximumBoundary.y)
            return true;
        return false;
    }

    public float Heuristic(Vector2 start, Vector2 end)
    {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
    }
}

public class WeightedPosition
{
    public WeightedPosition(float weight, Vector2 pos)
    {
        Weight = weight;
        Position = pos;
    }

    public float Weight;
    public Vector2 Position;
}