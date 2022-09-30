using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime.Renderers;

namespace WinterboltGames.BillboardGenerator.Runtime
{
	public sealed class BillboardRendererBatcher : MonoBehaviour
	{
		[Tooltip("Maximum number of billboard renderers to update every frame.")]
		public int updatesPerBatch;

		[Tooltip("The billboard renderers to batch-update.")]
		public List<BillboardRendererBase> billboards;

		/// <summary>
		/// The index that the batch will begin updating from.
		/// </summary>
		private int _start;

		private void Start() => billboards.AddRange(GetComponentsInChildren<BillboardRendererBase>());

		private void LateUpdate()
		{
			var end = _start + updatesPerBatch;

			for (var i = _start; i < end && i < billboards.Count; i++) billboards[i].UpdateBillboard();

			_start += updatesPerBatch;

			if (_start > billboards.Count - 1) _start = 0;
		}

#if UNITY_EDITOR

		[UnityEditor.MenuItem("Tools/Billboard Generator/Batch From Selection", priority = 10)]
		private static void CreateBatchFromSelection()
		{
			var billboards = UnityEditor.Selection.gameObjects.Select(selectedGameObject => selectedGameObject.GetComponent<BillboardRendererBase>()).Where(billboard => billboard != null).ToList();

			if (billboards.Count < 1) return;

			var batch = new GameObject("New_Billboard_Batch", typeof(BillboardRendererBatcher)).GetComponent<BillboardRendererBatcher>();

			batch.updatesPerBatch = Mathf.CeilToInt(billboards.Count * 0.1f);

			batch.billboards = new List<BillboardRendererBase>(billboards.Count);

			foreach (var billboard in billboards)
			{
				billboard.update = false;

				batch.billboards.Add(billboard);
			}
		}

#endif
	}
}
