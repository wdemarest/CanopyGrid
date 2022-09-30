using System;

using UnityEngine;

namespace WinterboltGames.BillboardGenerator.Runtime.Utilities
{
	/// <summary>
	/// A utility class that provides various functions that operate on textures.
	/// </summary>
	public static class TextureUtilities
	{
		private const string NoNonTransparentXAxisPixelsMessage = "Unable to find any non-transparent pixels on the X-axis.";
		private const string NoNonTransparentYAxisPixelsMessage = "Unable to find any non-transparent pixels on the Y-axis.";

		/// <summary>
		/// Trims the supplied <paramref name="original"/> texture.
		/// </summary>
		/// <param name="original">The texture to trim.</param>
		/// <returns>A trimmed version of the <paramref name="original"/> texture or the <paramref name="original"/> texture if it contains no transparent pixels.</returns>
		public static Texture2D Trim(Texture2D original)
		{
			int GetXMin()
			{
				for (var x = 0; x < original.width; x++)
				{
					for (var y = 0; y < original.height; y++)
					{
						if (original.GetPixel(x, y).a != 0.0f) return x;
					}
				}

				throw new Exception(NoNonTransparentXAxisPixelsMessage);
			}

			int GetYMin()
			{
				for (var y = 0; y < original.height; y++)
				{
					for (var x = 0; x < original.width; x++)
					{
						if (original.GetPixel(x, y).a != 0.0f) return y;
					}
				}

				throw new Exception(NoNonTransparentYAxisPixelsMessage);
			}

			int GetXMax()
			{
				for (var x = original.width - 1; x > -1; x--)
				{
					for (var y = original.height - 1; y > -1; y--)
					{
						if (original.GetPixel(x, y).a != 0.0f) return x;
					}
				}

				throw new Exception(NoNonTransparentXAxisPixelsMessage);
			}

			int GetYMax()
			{
				for (var y = original.height - 1; y > -1; y--)
				{
					for (var x = original.width - 1; x > -1; x--)
					{
						if (original.GetPixel(x, y).a != 0) return y;
					}
				}

				throw new Exception(NoNonTransparentYAxisPixelsMessage);
			}

			try
			{
				var xMin = GetXMin();
				var yMin = GetYMin();
				var xMax = GetXMax();
				var yMax = GetYMax();

				var width = xMax - xMin + 1;
				var height = yMax - yMin + 1;

				var trimmed = new Texture2D(width, height);

				trimmed.SetPixels(original.GetPixels(xMin, yMin, width, height));

				trimmed.Apply();

				return trimmed;
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// Pads the supplied <paramref name="original"/> texture by <paramref name="xPadding"/> and <paramref name="yPadding"/>.
		/// </summary>
		/// <param name="original">The texture to pad.</param>
		/// <param name="xPadding">The amount of horizontal padding to apply.</param>
		/// <param name="yPadding">The amount of vertical padding to apply.</param>
		/// <returns>A padded version of the <paramref name="original"/> texture or the <paramref name="original"/> texture if <paramref name="xPadding"/> and <paramref name="yPadding"/> = 0.</returns>
		public static Texture2D Pad(Texture2D original, int xPadding = 0, int yPadding = 0)
		{
			if (xPadding == 0 && yPadding == 0) return original;

			var width = original.width + xPadding * 2;
			var height = original.height + yPadding * 2;

			var padded = new Texture2D(width, height);

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					padded.SetPixel(x, y, Color.clear);
				}					
			}				

			padded.SetPixels32(xPadding, yPadding, original.width, original.height, original.GetPixels32());

			padded.Apply();

			return padded;
		}

		public static int DiagonalLength(Texture2D texture) => (int)Math.Sqrt(Math.Pow(texture.width, 2.0d) + Math.Pow(texture.height, 2.0d));
	}
}
