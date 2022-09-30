using BzKovSoft.ObjectSlicer;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/*
[CustomEditor(typeof(LookAtPoint))]
public class SlicerUi : Editor
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			void OnSlice(BzSliceTryResult result)
			{
				Debug.Log("One Done");
			}
			GameObject tree = GameObject.Find("TreeSliceTest").gameObject;
			Plane plane = new Plane(new Vector3(1, 0, 0).normalized, tree.transform.position);
			var sliceable = tree.transform.GetComponent<SingleSlicer>();
			sliceable.Slice(plane, OnSlice);
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			GameObject target = GameObject.Find("TreeSliceTest").gameObject;
			//GameObject target = GameObject.Find("MushroomTest").gameObject;
			target.GetComponent<Dicer>().Dice();
		}
	}
}
*/