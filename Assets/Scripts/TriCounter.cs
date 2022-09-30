using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TriCounter : MonoBehaviour
{
    float timer = 0;
    bool counting = false;
    public int triangleCount = 0;

    int CountTriangles(GameObject go)
    {
        if (go.GetComponent<Renderer>() == null || !go.GetComponent<Renderer>().isVisible)
            return 0;

        if (go.GetComponent<MeshFilter>() != null)
        {
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            return mesh == null ? 0 : mesh.triangles.Length / 3;
        }
        return 0;
    }
    void Update()
    {
        if (timer <= 0 && !counting)
        {
            counting = true;
            timer = 0.25f;
            int count = CountTriangles(gameObject);
            Util.TraverseChildren(gameObject, true, (GameObject child) =>
            {
                count += CountTriangles(child);
            });
            triangleCount = count;
            counting = false;
        }
        else
        {
            timer -= Time.deltaTime;
        }

    }
}
