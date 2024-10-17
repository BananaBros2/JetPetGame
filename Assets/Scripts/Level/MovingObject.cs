using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [Header("Force Transfer")]
    public bool ignoreDisplacement = false; // Do not track movement

    [HideInInspector] public Vector3 displacement; // value for the vector between the current and last frame
    private Vector3 oldPosition; // Stores last frame's position

    
    void FixedUpdate()
    {
        if (ignoreDisplacement) 
        {
            displacement = Vector3.zero; // Player will not be launched and will instead jump/leave the moving object will zero velocity offset
            return;
        }
        displacement = transform.position - oldPosition; // Difference between last and current fixedframe
        oldPosition = transform.position;
    }
}
