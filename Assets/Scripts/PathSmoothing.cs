using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathSmoothing : MonoBehaviour
{
    Vector3 quadraticBezierCurve(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, float interpolationVal)
    {
        //opposite of interpolation value within the clamped range of 1
        float complementaryValue = 1.0f - interpolationVal;

        float interpolationSquared = interpolationVal * interpolationVal;
        float complementarySquared = complementaryValue * complementaryValue;

        Vector3 result;
        //Quadratic Bezier Formulae
        result.x = (complementarySquared * startPoint.x) + (2 * complementaryValue * interpolationVal * controlPoint.x) + (interpolationSquared * endPoint.x);
        result.y = (complementarySquared * startPoint.y) + (2 * complementaryValue * interpolationVal * controlPoint.y) + (interpolationSquared * endPoint.y);
        result.z = (complementarySquared * startPoint.z) + (2 * complementaryValue * interpolationVal * controlPoint.z) + (interpolationSquared * endPoint.z);

        return result;
    }

    public List<Vector3> SmoothPath(List<Vector3> path, int samplesPerSegment)
    {
        //cannot interpolate path with less than 3 points
        if(path.Count <= 2)
        {
            return path;
        }
        //should not happen but just in case, set the number of control points to 1 so that bezier curve can happen
        if(samplesPerSegment < 1)
        {
            samplesPerSegment = 1;
        }

        List<Vector3> interpolatedPath = new List<Vector3>();
        interpolatedPath.Add(path[0]);

        for(int segmentIndex = 0; segmentIndex < path.Count - 1; segmentIndex++)
        {
            Vector3 currentPoint = path[segmentIndex];
            Vector3 nextPoint = path[segmentIndex + 1];
            //if overflow, reset last point to next point
            Vector3 nextNextPoint = (segmentIndex + 2 < path.Count) ? path[segmentIndex + 2] : path[segmentIndex + 1];
            for(int sampleIndex = 1; sampleIndex < samplesPerSegment; sampleIndex++)
            {
                float t = (float)sampleIndex / samplesPerSegment;
                Vector3 interpolatedPoint = quadraticBezierCurve(currentPoint, nextPoint, nextNextPoint, t);
                interpolatedPath.Add(interpolatedPoint);
            }
        }

        return interpolatedPath;
    }
}
