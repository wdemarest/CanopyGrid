using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using MantisLODScriptable;

//
// I can't remember what this was for, but we don't seem to use it anymore anyway.
//
//
public class Pruner : MonoBehaviour, IBuildNode
{
	public int submeshToExclude = 1;
	public bool buildCollider = false;
/*
	// WARNING: If you attempt cross fade, you MUST hand-edit the shaders so that the #pragma surface ends with #pragma surface ends with dithercrossfade
	void SaveOne(Mesh mesh, string dirName, string meshName)
	{
		if (!AssetDatabase.IsValidFolder("Assets/Meshes/" + dirName))
			AssetDatabase.CreateFolder("Assets/Meshes", dirName);

		string fileName = "Assets/Meshes/" + dirName + "/" + meshName + ".asset";

		MeshUtility.Optimize(mesh);
		AssetDatabase.CreateAsset(mesh, fileName);
		AssetDatabase.SaveAssets();
	}
    Mesh CopyMeshAndPrune(Mesh sourceMesh, int submeshToExclude)
	{
		Mesh mesh = new Mesh();
		mesh.vertices = sourceMesh.vertices;
		mesh.triangles = sourceMesh.triangles;
		mesh.uv = sourceMesh.uv;
		mesh.normals = sourceMesh.normals;
		mesh.colors = sourceMesh.colors;
		mesh.tangents = sourceMesh.tangents;
		mesh.bindposes = sourceMesh.bindposes;
		mesh.boneWeights = sourceMesh.boneWeights;

		mesh.subMeshCount = sourceMesh.subMeshCount; // - 1;
		if (mesh.subMeshCount > 0)
		{
			int targetSubMeshIndex = 0;
			for (int i = 0; i < sourceMesh.subMeshCount; ++i)
			{
				SubMeshDescriptor desc = sourceMesh.GetSubMesh(i);
				Debug.Log("Submesh " + i + " = " + desc);
				if (i == submeshToExclude)
				{
					continue;

				}
				mesh.SetSubMesh(targetSubMeshIndex, desc, MeshUpdateFlags.Default);
				targetSubMeshIndex += 1;
			}
		}

		return mesh;
	}
	public void Prune(float topLodIsXPercent, bool protectHardEdges, bool protectShape, int submeshToExclude)
	{
		string name = "Grid" + gameObject.name;

		// We need to operate from (0,0,0) or the individual LODs aren't positioned correctly.
		GameObject source = Instantiate(gameObject, Vector3.zero, Quaternion.identity);
		source.transform.localScale = Vector3.one;
		MeshFilter sourceMf = source.GetComponent<MeshFilter>();
		sourceMf.mesh = sourceMf.mesh;

		GameObject target = Instantiate(source, source.transform.position, source.transform.rotation);
		target.transform.localScale = Vector3.one;
		target.name = name + "InProcess";
		//DestroyImmediate(target.GetComponent<MeshRenderer>());
		//target.AddComponent<MeshRenderer>();


		MantisScripter mantis = new MantisScripter(source, target);
		var param = new MantisParams()
		{
			protect_normal = protectHardEdges,
			protect_shape = protectShape,
			quality = 100 * Mathf.Clamp(topLodIsXPercent, 0, 1)
		};
        mantis.Reduce(param);

		MeshFilter mf = target.GetComponent<MeshFilter>();

		MeshUtility.Optimize(mf.sharedMesh);

		Mesh tempMesh = CopyMeshAndPrune(mf.sharedMesh, submeshToExclude);

		target.name = name + "Collider";
		mf.sharedMesh = mf.mesh = tempMesh;
		SaveOne(tempMesh, name, target.name);
		AssetDatabase.Refresh();

		//target.GetComponent<MeshRenderer>().sharedMaterials.Length = target.GetComponent<MeshRenderer>().sharedMaterials.Length-1;
	
		GameObject.Destroy(source);

		Debug.LogWarning("Writing collider " + name + "Collider using "+ topLodIsXPercent);

		//mantis.ReleaseAndRevertMesh();

	}
	public void Build()
	{
		Debug.Assert(gameObject.GetComponent<MeshFilter>() != null);
		Debug.Assert(gameObject.GetComponent<MeshRenderer>() != null);
		Debug.Assert(gameObject.GetComponent<Dicer>() != null);

		Dicer dicer = gameObject.GetComponent<Dicer>();
		Prune(dicer.topLodIsXPercent, dicer.protectHardEdges, dicer.protectShape, submeshToExclude);
	}
	void Start()
	{
		buildCollider = false;
	}
	void Update()
	{
		if (buildCollider)
		{
			buildCollider = false;
			Build();
		}
	}
*/
	public void Build()
	{}

}


