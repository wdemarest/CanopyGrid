using System;
using System.IO;
using System.Linq;

namespace WinterboltGames.BillboardGenerator.Editor.Utilities
{
	internal static class PathUtilities
	{
		public static string ToProjectPath(string path)
		{
			var parts = path.Split(new[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);

			var index = Array.IndexOf(parts, "Assets");

			if (index == -1) throw new ArgumentException($"No assets folder found in the supplied {nameof(path)}.");

			return Path.Combine(parts.Skip(index).ToArray());
		}
	}
}
