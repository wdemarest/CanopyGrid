using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal static class GeneratorWindowSettings
	{
		private static string GetSettingsFilePath() => Path.Combine(Environment.CurrentDirectory, "billboard_generator_references.txt");

		public static void Save(IEnumerable<GeneratorWindowEntry> entries)
		{
			using (var stream = File.Create(GetSettingsFilePath()))
			{
				using (var writer = new StreamWriter(stream))
				{
					foreach (var entry in entries) writer.WriteLine($"{entry.gameObject.GetInstanceID()},{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.settingsAsset))}");
				}
			}
		}

		public static List<GeneratorWindowEntry> Load()
		{
			var entries = new List<GeneratorWindowEntry>();

			if (!File.Exists(GetSettingsFilePath())) return entries;
			
			using (var stream = File.OpenRead(GetSettingsFilePath()))
			{
				using (var reader = new StreamReader(stream))
				{
					string line;

					while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
					{
						var data = line.Split(',');

						var gameObject = EditorUtility.InstanceIDToObject(int.Parse(data[0])) as GameObject;

						var guid = data[1];

						GeneratorSettingsAsset settingsAsset = null;

						if (!string.IsNullOrWhiteSpace(guid)) settingsAsset = AssetDatabase.LoadAssetAtPath<GeneratorSettingsAsset>(AssetDatabase.GUIDToAssetPath(guid));

						entries.Add(new GeneratorWindowEntry { gameObject = gameObject, settingsAsset = settingsAsset, });
					}
				}
			}

			return entries;
		}
	}
}
