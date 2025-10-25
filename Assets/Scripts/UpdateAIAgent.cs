using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateAIAgent : MonoBehaviour
{
    private AIAgent agent;

    private void Awake()
    {
        agent = GetComponent<AIAgent>();
    }
    public void UpdateSpeed(FloatEvent ctx)
    {
        agent.AgentSpeed = ctx.FloatValue;
    }
    public void EnableAgentDebug(BoolEvent ctx)
    {
        agent.EnableDebug = ctx.Value;
    }
    public void EnablePathSmoothing(BoolEvent ctx)
    {
        agent.SmoothingEnabled = ctx.Value;
    }
    public void UpdateLineSegments(FloatEvent ctx)
    {
        agent.SmoothingSegments = (int)ctx.FloatValue;
    }

}
