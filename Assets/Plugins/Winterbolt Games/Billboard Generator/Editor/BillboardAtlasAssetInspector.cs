using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	[CustomEditor(typeof(BillboardAtlasAsset))]
	[CanEditMultipleObjects]
	internal sealed class BillboardAtlasAssetInspector : UnityEditor.Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			var rootVisualElement = new VisualElement();

			var rendererScaleField = new PropertyField(serializedObject.FindProperty(nameof(BillboardAtlasAsset.rendererScale)));

			rootVisualElement.Add(rendererScaleField);

			var uvCoordinatesField = new PropertyField(serializedObject.FindProperty(nameof(BillboardAtlasAsset.uvCoordinates)));

			uvCoordinatesField.SetEnabled(false);

			rootVisualElement.Add(uvCoordinatesField);

			return rootVisualElement;
		}
	}
}
