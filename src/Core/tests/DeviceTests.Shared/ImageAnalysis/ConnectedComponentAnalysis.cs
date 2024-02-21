using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests.ImageAnalysis
{
	/// <summary>
	/// Helper methods for finding pixels connected to each other. Great for finding UI Elements on the screen based on pixel color filter.
	/// </summary>
	/// <remarks>
	/// A good explanation of the Connected Component Analysis can be seen here: https://www.youtube.com/watch?v=ticZclUYy88
	/// Uses a 4-connectivity 2-pass Hoshen-Kopelman algorithm.
	/// </remarks>
	public static class ConnectedComponentAnalysis
	{
		/// <summary>
		/// Finds a set of pixels that are connected to each other, Looks at any pixels that are not black and/or transparent
		/// </summary>
		/// <param name="image"></param>
		/// <returns></returns>
		public static IList<Blob> FindConnectedPixels(RawBitmap image)
		{
			return FindConnectedPixels(image, (c) => (c.Red > 0 || c.Green > 0 || c.Blue > 0) && c.Alpha > 0);
		}

		/// <summary>
		/// Finds a set of pixels that are connected to each other, connecting any pixels that matches <paramref name="includePixelFunction"/>.
		/// </summary>
		/// <param name="element">Visual Element to analyze</param>
		/// <param name="includePixelFunction">Pixel filter</param>
		/// <returns>List of blobs</returns>
		public static async Task<IList<Blob>> FindConnectedPixelsAsync(VisualElement element, Func<Color, bool> includePixelFunction)
		{
			var bitmap = await element.AsRawBitmapAsync();
			return FindConnectedPixels(bitmap, includePixelFunction);
		}

		/// <summary>
		/// Finds a set of pixels that are connected to each other, connecting any pixels of the specific color.
		/// </summary>
		/// <param name="image">Image to analyze</param>
		/// <param name="color">Pixel color filter</param>
		/// <returns>List of blobs</returns>
		public static IList<Blob> FindConnectedPixels(RawBitmap image, Color color)
		{
			return FindConnectedPixels(image, c => c.ToUint() == color.ToUint());
		}

		/// <summary>
		/// Finds a set of pixels that are connected to each other, connecting any pixels that matches <paramref name="includePixelFunction"/>.
		/// </summary>
		/// <param name="image">Image to analyze</param>
		/// <param name="includePixelFunction">Pixel filter</param>
		/// <returns>List of blobs</returns>
		public static IList<Blob> FindConnectedPixels(RawBitmap image, Func<Color, bool> includePixelFunction)
		{
			int width = image.PixelWidth;
			int height = image.PixelHeight;
			bool[] pixels = new bool[width * height];

			int bitsPerPixel = 32;
			int stride = image.PixelWidth * bitsPerPixel / 8;
			byte[] pixelbuffer = image.PixelBuffer;

			for (int row = 0; row < height; row++)
			{
				for (int col = 0; col < width; col++)
				{
					byte b = pixelbuffer[row * stride + col * 4];
					byte g = pixelbuffer[row * stride + col * 4 + 1];
					byte r = pixelbuffer[row * stride + col * 4 + 2];
					byte a = pixelbuffer[row * stride + col * 4 + 3];

					pixels[col + row * width] = includePixelFunction(Color.FromRgba(r, g, b, a));
				}
			}

			Func<Color, bool> ismatch = includePixelFunction;
			int[] labels = new int[pixels.Length];
			int currentLabel = 0;
			// First pass - Label pixels
			UnionFind<int> sets = new UnionFind<int>();
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					var idx = j + i * width;
					bool v = pixels[idx];
					if (v)
					{
						var l1 = i == 0 ? 0 : labels[j + (i - 1) * width];
						var l2 = j == 0 ? 0 : labels[j + i * width - 1];
						if (l1 == 0 && l2 == 0)
						{
							//Assign new label
							currentLabel++;
							labels[idx] = currentLabel;
							sets.MakeSet(currentLabel);
						}
						else if (l1 > 0 && l2 == 0)
							labels[idx] = l1; //Copy label from neighbor
						else if (l1 == 0 && l2 > 0)
							labels[idx] = l2; //Copy label from neighbor
						else
						{
							labels[idx] = l1 < l2 ? l1 : l2; // Both neighbors have values. Grab the smallest label
							if (l1 != l2)
								sets.Union(sets.Find(l1), sets.Find(l2)); //store L1 is equivalent to L2
						}
					}
				}
			}
			// Second pass: Update equivalent labels
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					var idx = j + i * width;
					var lbl = labels[idx];
					if (lbl > 0)
					{
						var l = sets.Find(lbl);
						labels[idx] = l.Value;
					}
				}
			}
			// Generate blobs
			var blobs = new List<Blob>();
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					var idx = j + i * width;
					var lbl = labels[idx];
					if (lbl > 0)
					{
						var blob = blobs.Where(b => b.Id == lbl).FirstOrDefault();
						if (blob != null)
						{
							blob.MinColumn = Math.Min(blob.MinColumn, j);
							blob.MaxColumn = Math.Max(blob.MaxColumn, j);
							blob.MinRow = Math.Min(blob.MinRow, i);
							blob.MaxRow = Math.Max(blob.MaxRow, i);
						}
						else
						{
							blob = new Blob() { Id = lbl, MinColumn = j, MaxColumn = j, MinRow = i, MaxRow = i };
							blobs.Add(blob);
						}
						blob.Pixels.Add(new System.Drawing.Point(j, i));
					}
				}
			}
			foreach (var b in blobs)
			{
				b.MinColumn /= image.Density;
				b.MaxColumn /= image.Density;
				b.MinRow /= image.Density;
				b.MaxRow /= image.Density;
			}
			return blobs;
		}

		private class UnionFind<T>
		{
			// A generic Union Find Data Structure 
			// Based on https://github.com/thomas-villagers/unionfind.cs
			private Dictionary<T, SetElement> dict;

			public class SetElement
			{
				public SetElement Parent { get; internal set; }
				public T Value { get; }
				public int Size { get; internal set; }
				public SetElement(T value)
				{
					Value = value;
					Parent = this;
					Size = 1;
				}
				public override string ToString() => string.Format("{0}, size:{1}", Value, Size);
			}

			public UnionFind()
			{
				dict = new Dictionary<T, SetElement>();
			}

			public SetElement MakeSet(T value)
			{
				SetElement element = new SetElement(value);
				dict[value] = element;
				return element;
			}

			public SetElement Find(T value)
			{
				SetElement element = dict[value];
				SetElement root = element;
				while (root.Parent != root)
				{
					root = root.Parent;
				}
				SetElement z = element;
				while (z.Parent != z)
				{
					SetElement temp = z;
					z = z.Parent;
					temp.Parent = root;
				}
				return root;
			}

			public SetElement Union(SetElement root1, SetElement root2)
			{
				if (root2.Size > root1.Size)
				{
					root2.Size += root1.Size;
					root1.Parent = root2;
					return root2;
				}
				else
				{
					root1.Size += root2.Size;
					root2.Parent = root1;
					return root1;
				}
			}
		}

		/// <summary>
		/// Combines the bounding box of a set of blobs.
		/// </summary>
		/// <param name="blobs"></param>
		/// <returns></returns>
		public static Blob Union(this IEnumerable<Blob> blobs)
		{
			var b = blobs.FirstOrDefault();
			if (b != null)
				foreach (var bl in blobs.Skip(1))
				{
					b.MinColumn = Math.Min(bl.MinColumn, b.MinColumn);
					b.MinRow = Math.Min(bl.MinRow, b.MinRow);
					b.MaxColumn = Math.Max(bl.MaxColumn, b.MaxColumn);
					b.MaxRow = Math.Max(bl.MaxRow, b.MaxRow);
				}
			return b;
		}
	}

	/// <summary>
	/// Represents a blob of connected pixels found by <see cref="ConnectedComponentAnalysis"/>'s <c>FindConnectedPixels</c> methods.
	/// </summary>
	public class Blob
	{
		/// <summary>Blob ID</summary>
		public int Id;

		/// <summary>Starting column in device independent pixels.</summary>
		public double MinColumn;

		/// <summary>Ending columns in device independent pixels.</summary>
		public double MaxColumn;

		/// <summary>Starting row in device independent pixels.</summary>
		public double MinRow;

		/// <summary>Ending row in device independent pixels.</summary>
		public double MaxRow;

		/// <summary>Bounding box width in device independent pixels.</summary>
		public double Width => MaxColumn - MinColumn + 1;

		/// <summary>Bounding box height in device independent pixels.</summary>
		public double Height => MaxRow - MinRow + 1;

		/// <summary>Vertical center in device independent pixels.</summary>
		public double VerticalCenter => (MaxRow + MinRow) / 2;

		/// <summary>Horizontal center in device independent pixels.</summary>
		public double HorizontalCenter => (MaxColumn + MinColumn) / 2;

		/// <summary>List of pixels part of the blob.</summary>
		public List<System.Drawing.Point> Pixels { get; } = new List<System.Drawing.Point>();
	}
}