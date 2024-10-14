using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public Vector3[] stageTrack;
    //public int startPoint = 0;
    public int currentPoint = 0;

    public bool loop;
    public int flipped = 1;

    Vector2 offset;

    private void Start()
    {
        offset = transform.position;

        //currentPoint = startPoint;
        transform.GetChild(0).position = new Vector2(stageTrack[currentPoint].x + offset.x, stageTrack[currentPoint].y + offset.y);
    }

    void FixedUpdate()
    {
        Vector2 targetPosition = new Vector2(stageTrack[currentPoint].x + offset.x, stageTrack[currentPoint].y + offset.y);
        float platformSpeed = (stageTrack[currentPoint].z + stageTrack[Mathf.Clamp(currentPoint - flipped, 0, stageTrack.Length - 1)].z) / 2;

        transform.GetChild(0).position = Vector2.MoveTowards(transform.GetChild(0).position, targetPosition, platformSpeed / 10);

        if (Vector2.Distance(transform.GetChild(0).position, new Vector2(stageTrack[currentPoint].x + offset.x, stageTrack[currentPoint].y + offset.y)) < 0.1f)
        {
            currentPoint += flipped;

        }


        if (currentPoint + 1 > stageTrack.Length || currentPoint < 0)
        {
            
            if (loop)
            {
                currentPoint = 0;
            }
            else
            {
                flipped *= -1;
                currentPoint += flipped;
            }

        }
    }

    // DEBUGGING 
    void OnDrawGizmos()
    {
        Vector3 lastPoint = new Vector3(0,0,0);
        Vector2 gizmoOffset = transform.position;

        Gizmos.DrawSphere(transform.position, 0.2f);


        foreach (Vector3 point in stageTrack)
        {
            if(lastPoint == null)
            {
                return;
            }
            Gizmos.DrawSphere(new Vector3(point.x + gizmoOffset.x, point.y + gizmoOffset.y, 0), 0.2f);

            Gizmos.DrawLine(new Vector3(lastPoint.x + gizmoOffset.x, lastPoint.y + gizmoOffset.y, 0), new Vector3(point.x + gizmoOffset.x, point.y + gizmoOffset.y, 0));
            lastPoint = point;
        }

        if(loop)
        {
            Gizmos.DrawSphere(new Vector3(stageTrack[0].x + gizmoOffset.x, stageTrack[0].y + gizmoOffset.y, 0), 0.2f);
            Gizmos.DrawLine(new Vector3(lastPoint.x + gizmoOffset.x, lastPoint.y + gizmoOffset.y, 0), new Vector3(stageTrack[0].x + gizmoOffset.x, stageTrack[0].y + gizmoOffset.y, 0));
        }
    }

}
