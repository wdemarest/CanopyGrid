using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridGroup : MonoBehaviour
{
	public Vector3 sliceDims;

	[Range(0.01f, 5.0f)]
	public float bias0;
	[Range(0.01f, 5.0f)]
	public float bias1to6;

	[Range(0.01f, 2.0f)]
	[ReadOnly] public float uniformSizeModifier = 1.0f;
	public bool forceUniform;

	float lastBias0 = -1;
	float lastBias1to6 = -1;
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
	public void AdjustBias(string parentName)
	{
		lastBias0 = bias0;
		lastBias1to6 = bias1to6;
		lastUniformSizeModifier = uniformSizeModifier;
		int[] lodQuality = new int[] { 100, 80, 60, 40, 20, 10, 5 };

		gameObject.TraverseChildren<LODGroup>(true, (LODGroup lodGroup) =>
		{
			LOD[] lodList = lodGroup.GetLODs();

			string s = "";
			for (int lodIndex = 0; lodIndex < lodList.Length; ++lodIndex)
			{
				float quality = ((float)lodQuality[0]) / 100.0f * bias0;
				quality -= ((float)(lodQuality[0] - lodQuality[lodIndex])) / 100.0f * bias1to6;

				float cutoff = quality;

				lodList[lodIndex] = new LOD(cutoff, lodList[lodIndex].renderers);
				s += lodIndex + "=" + cutoff + ", ";
			}
			Debug.Log("LODs " + parentName + "." + lodGroup.gameObject.name + " " + s);
			lodGroup.SetLODs(lodList);

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
		if (bias0 != lastBias0 || bias1to6 != lastBias1to6 || uniformSizeModifier != lastUniformSizeModifier)
			AdjustBias("World");
	}
}
