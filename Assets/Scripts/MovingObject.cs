using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public bool ignoreDisplacement;

    public Vector3 displacement;
    Vector3 oldPosition;

    
    void FixedUpdate()
    {
        if (ignoreDisplacement)
        {
            displacement = Vector3.zero;
            return;
        }
        displacement = transform.position - oldPosition;
        oldPosition = transform.position;
    }
}
