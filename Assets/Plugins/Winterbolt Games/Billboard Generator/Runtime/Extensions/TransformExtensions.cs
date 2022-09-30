﻿using System.Collections.Generic;
using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Extensions
{
	/// <summary>
	/// TBA.
	/// </summary>
	public static class TransformExtensions
	{
		/// <summary>
		/// TBA.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IEnumerable<Transform> ChildrenIterator(this Transform transform)
		{
			for (var i = 0; i < transform.childCount; i++) yield return transform.GetChild(i);
		}

		/// <summary>
		/// Get the an array containing the specified <paramref name="transform"/> and all of its children.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns>
		/// An array containing <paramref name="transform"/> and all of its children.
		/// </returns>
		public static Transform[] GetTree(this Transform transform)
		{
			var tree = new List<Transform>
			{
				transform,
			};

			foreach (var child in ChildrenIterator(transform))
			{
				if (child.childCount > 0)
				{
					tree.AddRange(GetTree(child));
				}
				else tree.Add(child);
			}

			return tree.ToArray();
		}
	}
}
