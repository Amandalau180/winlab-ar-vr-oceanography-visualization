using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PivotAdjustment : MonoBehaviour
{
    // Offset from the current local position to set the pivot
    public Vector3 pivotOffset;

    void Start()
    {
        // Adjust the pivot by moving the GameObject's local position
        transform.localPosition += pivotOffset;
    }
}
