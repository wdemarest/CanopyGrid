using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using BzKovSoft.ObjectSlicer;
using MantisLODScriptable;

public class GridSlice : MonoBehaviour
{
	public Vector3 center = new Vector3(0, 0, 0);
	public Vector3Int gridPos = new Vector3Int();
}

public class GridSliceMaster : MonoBehaviour
{
	public const int maxDim = 20;
	public int xDim;
	public int yDim;
	public int zDim;
	public GameObject[,,] grid = null;
}

public class Dicer : MonoBehaviour, IBuildNode
{
	public Material capMaterial;
	public float sliceDim = 43.0f;
	public int lodCount = 99;
	public LODFadeMode fadeMode;
	public bool createCap = false;
	public float topLodIsXPercent = 0.773f;
	public bool protectHardEdges = true;
	public bool protectShape = true;
	public float bias0 = 0.85f;
	public float bias1to6 = 0.85f;
	public bool createDebugObjects = false;
	public bool justOneCut = false;
	public bool buildMe = false;

	// WARNING: If you attempt cross fade, you MUST hand-edit the shaders so that the #pragma surface ends with dithercrossfade
	void SaveOne(Mesh mesh, string dirName, string meshName)
	{
		if (!AssetDatabase.IsValidFolder("Assets/Meshes/" + dirName))
			AssetDatabase.CreateFolder("Assets/Meshes", dirName);

		string fileName = "Assets/Meshes/" + dirName + "/" + meshName + ".asset";

		MeshUtility.Optimize(mesh);
		AssetDatabase.CreateAsset(mesh, fileName);
		AssetDatabase.SaveAssets();
	}
	void SaveAll(GameObject go)
	{
		int count = 0;
		Util.TraverseChildren(go,true,(GameObject child)=>
		{
			MeshFilter meshFilter = child.GetComponent<MeshFilter>();
			if (meshFilter != null)
			{
				string meshName = go.name + "_" + child.transform.parent.gameObject.name + "_" + child.name;
				SaveOne(meshFilter.sharedMesh, go.name, meshName);
				++count;
			}
		});
		Debug.Log("Saved " + count + " meshes.");
		AssetDatabase.Refresh();
	}
	void AddLod(GameObject target, GameObject cur, int lodIndex, float distancePercent)
	{
		//Debug.LogWarning("Call to AddLod lodIndex=" + lodIndex);
		LODGroup group = target.GetComponent<LODGroup>();
		LOD[] newLods = new LOD[lodIndex + 1];
		if (group == null)
		{
			group = target.AddComponent<LODGroup>() as LODGroup;
		}
		else
		{
			LOD[] oldLods = group.GetLODs();
			for (int i = 0; i < oldLods.Length; ++i) newLods[i] = oldLods[i];
		}
		if (fadeMode == LODFadeMode.CrossFade)
		{
			group.fadeMode = LODFadeMode.CrossFade;
			group.animateCrossFading = true;
		}
		if (fadeMode == LODFadeMode.SpeedTree)
		{
			group.fadeMode = LODFadeMode.SpeedTree;
			group.animateCrossFading = false;
		}

        void LogGaps()
        {
            String s = cur.transform.parent.gameObject.name + ": ";
            for (int i = 0; i < newLods.Length; ++i)
            {
                s += "L" + i + "=" + newLods[i].screenRelativeTransitionHeight + ", ";
            }
            Debug.Log(s);
        }


        Renderer[] renderers = new Renderer[1];
		renderers[0] = cur.GetComponent<Renderer>();
		newLods[lodIndex] = new LOD(distancePercent, renderers);
		//Debug.Log("lodIndex=" + lodIndex + ", distancePercent=" + distancePercent);

		//LogGaps();

		// WEIRDLY, this keeps complaining as follows:
		// SetLODs: Attempting to set LOD where the screen relative size is greater then or equal to a higher detail LOD level.
		// but that statement is absolutely false. Who know why.

		bool stopMe = false;
        float tinyGap = 0.01f;
        string lodString = "LODs=";
		for (int i = 0; i < newLods.Length-1; ++i)
		{
			if (newLods[i].screenRelativeTransitionHeight == 0)
			{
				stopMe = true;

                float priorSRHT = i ==0 ? 0.1f : newLods[i - 1].screenRelativeTransitionHeight;
				float newSRHT = priorSRHT - tinyGap;
                newLods[i] = new LOD(newSRHT, renderers);
                //Debug.LogWarning("PriorSRHT=" + priorSRHT+", newSRHT="+newSRHT+ ", newLods["+i+"]="+ newLods[i].screenRelativeTransitionHeight);
            }
            lodString += "("+i+"="+newLods[i].screenRelativeTransitionHeight + "),";
        }
        //Debug.LogWarning(lodString);
		//LogGaps();

		try
		{
			if( stopMe)
			{
				Debug.LogWarning("Stopping");
			}
			group.SetLODs(newLods);
		}
		catch
		{
			Debug.LogError("Unacceptable LODs");
		}
	}
	Mesh CopyMesh(Mesh sourceMesh)
	{
		Mesh mesh = new Mesh();
		mesh.vertices = sourceMesh.vertices;
		mesh.triangles = sourceMesh.triangles;
		mesh.uv = sourceMesh.uv;
		mesh.normals = sourceMesh.normals;
		mesh.colors = sourceMesh.colors;
		mesh.tangents = sourceMesh.tangents;
		return mesh;
	}
	public IEnumerator Dice(int lodCount, Vector3 sliceDims, float bias0, float bias1to6, bool justOneCut, bool createDebugObjects)
	{
		int[] lodQuality = new int[] { 100, 80, 60, 40, 20, 10, 5 };
		string name = "Grid" + gameObject.name;
		lodCount = Mathf.Clamp(lodCount, 1, lodQuality.Length);

		// We need to operate from (0,0,0) or the individual LODs aren't positioned correctly.
		GameObject source = Instantiate(gameObject, Vector3.zero, Quaternion.identity);
		source.transform.localScale = Vector3.one;

		GameObject result = new GameObject(name);
		result.transform.localScale = Vector3.one;
		GridGroup gridGroup = result.AddComponent<GridGroup>() as GridGroup;
		gridGroup.sliceDims = sliceDims;
		gridGroup.bias0 = bias0;
		gridGroup.bias1to6 = bias1to6;

		// WARNING: You must parent this first, and THEN set localScale to one.
		GameObject parts = new GameObject("Parts");
		parts.transform.parent = result.transform;
		parts.transform.localScale = Vector3.one;

		GridSliceMaster[] masterList = new GridSliceMaster[lodCount];
		MantisScripter[] mantis = new MantisScripter[lodCount];
		for (int lodIndex = 0; lodIndex < lodCount; ++lodIndex)
		{
			GameObject target = Instantiate(source,source.transform.position,source.transform.rotation);
			target.transform.localScale = Vector3.one;
			target.name = "CopyOf" + name;

			// CHANGE TO APPROPRIATE LOD
			mantis[lodIndex] = new MantisScripter(source,target);
			var param = new MantisParams() {
				protect_normal = protectHardEdges,
				protect_shape = protectShape,
				quality = lodQuality[lodIndex] * Mathf.Clamp(topLodIsXPercent,0,1)
			};
			mantis[lodIndex].Reduce(param);

			// SLICE IT UP
			GridSlicer gridSlicer = new GridSlicer();
			string masterName = "Master" + name + "_" + lodIndex;
			yield return gridSlicer.Slice(target, sliceDims, masterName, name, justOneCut);
			masterList[lodIndex] = GameObject.Find(masterName).GetComponent<GridSliceMaster>();
			int a = lodIndex + 1;
			a++;
		}

		GridSliceMaster mg = masterList[0];
		GameObject[,,] part = new GameObject[mg.xDim, mg.yDim, mg.zDim];

		for (int x = 0; x < mg.xDim; ++x)
		{
			for (int y = 0; y < mg.yDim; ++y)
			{
				for (int z = 0; z < mg.zDim; ++z)
				{
					for (int lodIndex = 0; lodIndex < lodCount; ++lodIndex)
					{
						GameObject cur = masterList[lodIndex].grid[x, y, z];
						if (cur == null) continue;
						GridSlice gridSlice = cur.GetComponent<GridSlice>();

						if (part[x, y, z] == null)
						{
							part[x, y, z] = new GameObject("part" + x + "" + y + "" + z);
							part[x, y, z].transform.parent = parts.transform;
							part[x, y, z].transform.position = gridSlice.center;
							if(createDebugObjects)
								part[x, y, z].AddComponent<Part>();

							// Make a display cube.
							if (createDebugObjects)
							{
								GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
								cube.transform.parent = part[x, y, z].transform;
								cube.transform.localPosition = Vector3.zero;
							}
						}
						cur.transform.parent = part[x, y, z].transform;
						cur.name = "L" + lodIndex;
						float tempQuality = ((float)lodQuality[lodIndex]) / 100.0f;
						//Debug.LogWarning("AddLod cur=" + cur.name + ", lodIndex=" + lodIndex + ", tempQuality=" + tempQuality);
						AddLod(part[x, y, z], cur, lodIndex, tempQuality);

						GameObject.Destroy(cur.GetComponent<BoxCollider>());
						GameObject.Destroy(cur.GetComponent<BzSliceConfiguration>());
						GameObject.Destroy(cur.GetComponent<SingleSlicer>());
						GameObject.Destroy(cur.GetComponent<Dicer>());
						if(!createDebugObjects)
							GameObject.Destroy(cur.GetComponent<GridSlice>());
						yield return null;
					}
				}
			}
		}
		// Deal with bounds.
		source.TraverseChildren<GridGroup>(true,(GridGroup gridGroup) =>
		{
			gridGroup.ForceUniformSizing();
			gridGroup.AdjustBias(source.name);
		});
		for (int lodIndex = 0; lodIndex < lodCount; ++lodIndex)
		{
			GameObject.Destroy(masterList[lodIndex].gameObject);
		}
		GameObject.Destroy(source);

		gameObject.SetActive(false);

		result.transform.localScale = gameObject.transform.localScale;
		SaveAll(result);

		for (int lodIndex = 0; lodIndex < lodCount; ++lodIndex)
			mantis[lodIndex].ReleaseAndRevertMesh();

	}
	public void Build()
	{
		Component MakeCleanComponent(Type type)
		{
			DestroyImmediate(gameObject.GetComponent(type));
			return gameObject.AddComponent(type);
		}
		if (capMaterial == null)
		{
			Debug.Log("Must specify a cap material, even when not capping.");
			return;
		}
		SingleSlicer singleSlicer = MakeCleanComponent(typeof(SingleSlicer)) as SingleSlicer;
		singleSlicer.defaultSliceMaterial = capMaterial;
		var config = MakeCleanComponent(typeof(BzSliceConfiguration)) as BzSliceConfiguration;
		config.SliceMaterial = capMaterial;
		config.CreateCap = createCap;
		DestroyImmediate(gameObject.GetComponent<MantisLODEditorProfessional>());
		DestroyImmediate(gameObject.GetComponent<Pruner>());
		DestroyImmediate(gameObject.GetComponent<MeshCollider>());
		gameObject.AddComponent<BoxCollider>();

		Debug.Assert(gameObject.GetComponent<MeshFilter>() != null);
		Debug.Assert(gameObject.GetComponent<MeshRenderer>() != null);

		Vector3 scale = gameObject.transform.localScale;
		Vector3 sliceDims = new Vector3(sliceDim / scale.x, sliceDim / scale.y, sliceDim / scale.z);

		StartCoroutine(Dice(lodCount, sliceDims, bias0, bias1to6, justOneCut, createDebugObjects));
	}
	void Start()
	{
		buildMe = false;
		LODGroup.crossFadeAnimationDuration = 2.0f;
	}
	void Update()
	{
		if (buildMe)
		{
			buildMe = false;
			Build();
		}
	}
}


