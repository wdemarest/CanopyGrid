using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Renderers
{
	public sealed class IndexBillboardRenderer : SimpleBillboardRenderer
	{
		[SerializeField]
		private BillboardAtlasAsset atlas;

		[SerializeField]
		private int uvIndex;

		public override bool IsValid(bool warn = true)
		{
			if (material == null)
			{
				if (warn) Debug.LogWarning($"{name} has no {nameof(material)} assigned.", this);

				return false;
			}

			if (atlas == null)
			{
				if (warn) Debug.LogWarning($"{name} has no {nameof(atlas)} assigned.", this);

				return false;
			}

			if (uvIndex < 0 || uvIndex > atlas.uvCoordinates.Count - 1)
			{
				if (warn) Debug.LogWarning($"{name}'s {nameof(uvIndex)} is invalid.");

				return false;
			}

			return true;
		}

		protected override Mesh MakeQuad()
		{
			var mesh = new Mesh { name = "Billboard", };

			mesh.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f), new Vector3(0.5f, 0.5f), new Vector3(-0.5f, 0.5f), };

			mesh.triangles = new[] { 2, 1, 0, 3, 2, 0, };

			var uvs = atlas.uvCoordinates[uvIndex];

			mesh.uv = new[] { uvs.bottomLeft, uvs.bottomRight, uvs.topRight, uvs.topLeft, };

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;
		}
	}
}
