using BzKovSoft.ObjectSlicer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSlicer
{
	public class Place
	{
		GameObject gameObject;
	}

	public void KillMeshColliders(GameObject go, bool makeBoxCollider)
	{
		MeshCollider[] bc = go.GetComponents<MeshCollider>();
		for (int i = 0; i < bc.Length; ++i) GameObject.Destroy(bc[i]);
		if(makeBoxCollider) go.AddComponent(typeof(BoxCollider));
	}
	public IEnumerator Slice(GameObject originalSource, Vector3 sliceDims, string masterName, string baseName, bool justOneCut)
	{
		GameObject keepNeg = null;
		GameObject keepPos = null;
		GameObject sliceMe = null;

		void OnSlice(BzSliceTryResult result)
		{
			keepNeg = result.outObjectNeg;
			keepPos = result.outObjectPos;
			if(result.outObjectPos!=null)
				sliceMe = result.outObjectPos;
		}
		void SliceOne(GameObject source, Vector3 normal, Vector3 pos)
		{
			Plane plane = new Plane(normal, pos);
			var singleSlicer = source.transform.GetComponent<SingleSlicer>();
			singleSlicer.Slice(plane, OnSlice);
		}
		GameObject Clean(GameObject cleanMe, bool makeBoxCollider= true)
		{
			if (cleanMe != null)
				KillMeshColliders(cleanMe, makeBoxCollider);
			return cleanMe;
		}

		GameObject resultObject = new GameObject(masterName);
		GridSliceMaster resultGrid = (GridSliceMaster)resultObject.AddComponent(typeof(GridSliceMaster));

		if (originalSource.GetComponent<GridSlice>() == null)
		{
			originalSource.AddComponent(typeof(GridSlice));
		}
		Renderer renderer = originalSource.GetComponent<Renderer>();
		Vector3 extents = renderer.bounds.extents;
		Vector3 center = renderer.bounds.center;

		KillMeshColliders(originalSource, true);

		GameObject[,,] grid = new GameObject[GridSliceMaster.maxDim, GridSliceMaster.maxDim, GridSliceMaster.maxDim];

		// CUT ACROSS X
		sliceMe = originalSource;
		int xDim = (int)Mathf.Ceil((extents.x * 2f) / sliceDims.x);
		int yDim = (int)Mathf.Ceil((extents.y * 2f) / sliceDims.y);
		int zDim = (int)Mathf.Ceil((extents.z * 2f) / sliceDims.z);
		int xGrid = 0;
		int yGrid = 0;
		int zGrid = 0;

		if (justOneCut)
		{
			xDim = 2;
			yDim = 2;
			zDim = 2;
		}

		for (xGrid = 0; xGrid < xDim-1; ++xGrid)
		{
			float xCut = center.x - extents.x + (xGrid + 1) * sliceDims.x;
			SliceOne(sliceMe, new Vector3(1, 0, 0), new Vector3(xCut, center.y, center.z));
			yield return null;
			grid[xGrid,0,0] = Clean(keepNeg);
		}
		grid[xGrid,0,0] = Clean(keepPos);
		while (xGrid>=0 && grid[xGrid,0,0] == null) --xGrid;
		grid[xGrid+1,0,0] = Clean(sliceMe);

		// CUT ACROSS Y
		for( xGrid=0; xGrid<xDim; ++xGrid)
		{
			sliceMe = grid[xGrid, 0, 0];
			if (sliceMe == null) continue;
			for (yGrid = 0; yGrid < yDim-1; ++yGrid)
			{
				float yCut = center.y - extents.y + (yGrid + 1) * sliceDims.y;
				SliceOne(sliceMe, new Vector3(0, 1, 0), new Vector3(center.x, yCut, center.z));
				yield return null;
				grid[xGrid, yGrid, 0] = Clean(keepNeg);
			}
			grid[xGrid, yGrid, 0] = Clean(keepPos);
			while (yGrid >= 0 && grid[xGrid, yGrid, 0] == null) --yGrid;
			Debug.Log("x" + xGrid + " y" + yGrid);
			grid[xGrid, yGrid+1, 0] = Clean(sliceMe);
		}

		// CUT ACROSS Z
		List<GameObject> zList = new List<GameObject>();
		for (xGrid = 0; xGrid < xDim; ++xGrid)
		{
			for (yGrid = 0; yGrid < yDim; ++yGrid)
			{
				sliceMe = grid[xGrid, yGrid, 0];
				if (sliceMe == null) continue;
				for (zGrid = 0; zGrid < zDim-1; ++zGrid)
				{
					float zCut = center.z - extents.z + (zGrid + 1) * sliceDims.z;
					SliceOne(sliceMe, new Vector3(0, 0, 1), new Vector3(center.x, center.y, zCut));
					yield return null;
					grid[xGrid, yGrid, zGrid] = Clean(keepNeg,false);
				}
				grid[xGrid, yGrid, zGrid] = Clean(keepPos, false);
				while (zGrid >= 0 && grid[xGrid, yGrid, zGrid] == null) --zGrid;
				grid[xGrid, yGrid, zGrid+1] = Clean(sliceMe, false);
			}
		}

		resultGrid.xDim = xDim;
		resultGrid.yDim = yDim;
		resultGrid.zDim = zDim;
		resultGrid.grid = new GameObject[xDim, yDim, zDim];
		for (xGrid = 0; xGrid < xDim; ++xGrid)
		{
			for (yGrid = 0; yGrid < yDim; ++yGrid)
			{
				for (zGrid = 0; zGrid < zDim; ++zGrid)
				{
					GameObject go = grid[xGrid, yGrid, zGrid];
					if (go == null) continue;
					GridSlice gridSlice = go.GetComponent<GridSlice>();
					gridSlice.gridPos = new Vector3Int(xGrid, yGrid, zGrid);
					gridSlice.center = new Vector3(
						center.x - extents.x + sliceDims.x * xGrid + sliceDims.x * 0.5f,
						center.y - extents.y + sliceDims.y * yGrid + sliceDims.y * 0.5f,
						center.z - extents.z + sliceDims.z * zGrid + sliceDims.z * 0.5f
					);
					go.name = baseName + xGrid + "" + yGrid + "" + zGrid;
					resultGrid.grid[xGrid, yGrid, zGrid] = go;

					MeshCollider[] mc = go.GetComponents<MeshCollider>();
					for (int i = 0; i < mc.Length; ++i) GameObject.Destroy(mc[i]);

					BoxCollider[] bc = go.GetComponents<BoxCollider>();
					for (int i = 0; i < bc.Length; ++i) GameObject.Destroy(bc[i]);
					//float d = 7.0f;
					//go.transform.position = new Vector3(go.transform.position.x + s.gridPos.x * d, go.transform.position.y + s.gridPos.y * d, go.transform.position.z + s.gridPos.z * d);
					go.transform.parent = resultObject.transform;
				}
			}
		}
	}
}
