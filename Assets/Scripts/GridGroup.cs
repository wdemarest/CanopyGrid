using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridGroup : MonoBehaviour
{
	public Vector3 sliceDims;
	public float[] cutoffArray;

	[Range(0.01f, 2.0f)]
	[ReadOnly] public float uniformSizeModifier = 1.0f;
	public bool forceUniform;

	float lastUniformSizeModifier = -1;

	public void ForceUniformSizing()
	{
		gameObject.TraverseChildren(true, (GameObject child) =>
		{
			LODGroup lodGroup = child.GetComponent<LODGroup>();
			if (lodGroup != null)
			{
				lodGroup.localReferencePoint = new Vector3(0, 0, 0);
				lodGroup.size = Mathf.Max(sliceDims.x, sliceDims.y, sliceDims.z) * uniformSizeModifier;
			}

		});
	}
	public void AdjustCutoffs(string parentName)
	{
		lastUniformSizeModifier = uniformSizeModifier;
		gameObject.TraverseChildren<LODGroup>(true, (LODGroup lodGroup) =>
		{
			LOD[] lodList = lodGroup.GetLODs();

			string s = "";
			for (int lodIndex = 0; lodIndex < lodList.Length; ++lodIndex)
			{
				lodList[lodIndex] = new LOD(cutoffArray[lodIndex]/100.0f, lodList[lodIndex].renderers);
				s += lodIndex + "=" + cutoffArray[lodIndex] + ", ";
			}
			Debug.Log("LODs " + parentName + "." + lodGroup.gameObject.name + " " + s);
			lodGroup.SetLODs(lodList);
			lodGroup.RecalculateBounds();

			lodGroup.localReferencePoint = new Vector3(0, 0, 0);
			lodGroup.size = Mathf.Max(sliceDims.x, sliceDims.y, sliceDims.z) * uniformSizeModifier;
		});
	}

	void Start()
	{
		// WARNING: assumes that scale (x,y,z) is uniform, and that this thing has a scale that is correct relative
		// to the world, eg scale=10 is correct for both local and world coordinates.
		uniformSizeModifier = 1.0f / gameObject.transform.localScale.x;

		ForceUniformSizing();
	}
	void Update()
	{
		if (forceUniform)
		{
			forceUniform = false;
			ForceUniformSizing();
		}
		if (uniformSizeModifier != lastUniformSizeModifier)
			AdjustCutoffs("World");
	}
}
