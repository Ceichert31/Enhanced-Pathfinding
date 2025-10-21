using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private Dictionary<WeightedPosition, float> costSoFar;

}

public struct WeightedPosition
{
    public float Weight;
    public Vector3 Position;
}