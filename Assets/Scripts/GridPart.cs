using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    public Vector3 center;
    public float size;

    void Update()
    {
        LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
        size = lodGroup.size;
        center = lodGroup.localReferencePoint;
    }
}
