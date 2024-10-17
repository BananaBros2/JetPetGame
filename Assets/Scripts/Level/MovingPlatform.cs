using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Track Control")]
    public Vector3[] stageTrack;
    public int currentPoint = 0;
    public bool loop; // Whether will loop back to start point
    [Range(0.04f, 1)] public float pointSizeDebug = 0.2f; // Size of track points

    [Header("Current State Values")]
    private int flipped = 1;
    private Vector2 offset;


    private void Start()
    {
        offset = transform.position;

        // Change the platform's starting position based on what the currentPoint is set to
        transform.GetChild(0).position = new Vector2(stageTrack[currentPoint].x + offset.x, stageTrack[currentPoint].y + offset.y);
    }

    void FixedUpdate()
    {
        Vector2 targetPosition = new Vector2(stageTrack[currentPoint].x + offset.x, stageTrack[currentPoint].y + offset.y);
        float platformSpeed = (stageTrack[currentPoint].z + stageTrack[Mathf.Clamp(currentPoint - flipped, 0, stageTrack.Length - 1)].z) / 2; // Mmm math

        transform.GetChild(0).position = Vector2.MoveTowards(transform.GetChild(0).position, targetPosition, platformSpeed / 10);

        // If platform has is within 0.001m of the targeted point
        if (Vector2.Distance(transform.GetChild(0).position, new Vector2(stageTrack[currentPoint].x + offset.x, stageTrack[currentPoint].y + offset.y)) < 0.001f)
        {
            currentPoint += flipped;
        }


        if (currentPoint + 1 > stageTrack.Length || currentPoint < 0) // If platform has reached the end of the track
        {
            
            if (loop) { currentPoint = 0; } // Loop back to start point
            else
            {
                flipped *= -1; // Reverse direction
                currentPoint += flipped;
            }

        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos() // Used to visually show route that the platform will travel
    {
        Vector3 lastPoint = new Vector3(0,0,0);
        Vector2 gizmoOffset = transform.position;

        Gizmos.DrawSphere(transform.position, pointSizeDebug); // Will draw a sphere at the starting position located at the platform's placement


        foreach (Vector3 point in stageTrack) // Iterates through all the given vectors and draws a sphere plus a line linking it with the previous point
        {
            Gizmos.DrawSphere(new Vector3(point.x + gizmoOffset.x, point.y + gizmoOffset.y, 0), pointSizeDebug);

            Gizmos.DrawLine(new Vector3(lastPoint.x + gizmoOffset.x, lastPoint.y + gizmoOffset.y, 0), new Vector3(point.x + gizmoOffset.x, point.y + gizmoOffset.y, 0));
            lastPoint = point;
        }

        if(loop) // Draw a line between the start and end positions
        {
            Gizmos.DrawSphere(new Vector3(stageTrack[0].x + gizmoOffset.x, stageTrack[0].y + gizmoOffset.y, 0), pointSizeDebug);
            Gizmos.DrawLine(new Vector3(lastPoint.x + gizmoOffset.x, lastPoint.y + gizmoOffset.y, 0), new Vector3(stageTrack[0].x + gizmoOffset.x, stageTrack[0].y + gizmoOffset.y, 0));
        }
    }
# endif

}
