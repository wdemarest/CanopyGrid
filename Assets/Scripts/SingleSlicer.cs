using System;
using UnityEngine;
using BzKovSoft.ObjectSlicer;

public class SingleSlicer : BzSliceableObjectBase
{
	protected override BzSliceTryData PrepareData(Plane plane)
	{
		var colliders = gameObject.GetComponentsInChildren<Collider>();
		BzSliceTryData bzSliceTryData = new BzSliceTryData()
		{
			componentManager = new StaticComponentManager(gameObject, plane, colliders),
			plane = plane
		};
		return bzSliceTryData;
	}
	protected override void OnSliceFinished(BzSliceTryResult result)
	{
		//Debug.Log("Sliced");
	}
}
