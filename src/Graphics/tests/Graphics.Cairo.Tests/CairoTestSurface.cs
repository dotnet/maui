using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Graphics.Cairo.Tests
{
	/// <summary>
	/// An in-memory image surface with a <see cref="CairoCanvas"/> on it and pixel accessors for assertions.
	/// </summary>
	internal sealed class CairoTestSurface : IDisposable
	{
		public CairoTestSurface(int width = 100, int height = 100)
		{
			Width = width;
			Height = height;
			Surface = new CairoImageSurface(CairoFormat.Argb32, width, height);
			Context = new CairoContext(Surface);
			Canvas = new CairoCanvas(Context);
		}

		public int Width { get; }

		public int Height { get; }

		public CairoImageSurface Surface { get; }

		public CairoContext Context { get; }

		public CairoCanvas Canvas { get; }

		/// <summary>Reads a pixel as straight (non-premultiplied) ARGB bytes.</summary>
		public (byte A, byte R, byte G, byte B) Pixel(int x, int y)
		{
			Surface.Flush();
			var value = (uint)Marshal.ReadInt32(Surface.DataPtr, y * Surface.Stride + x * 4);
			var a = (byte)(value >> 24);
			var r = (byte)(value >> 16);
			var g = (byte)(value >> 8);
			var b = (byte)value;
			if (a != 0 && a != 255)
			{
				r = (byte)Math.Min(255, (r * 255 + a / 2) / a);
				g = (byte)Math.Min(255, (g * 255 + a / 2) / a);
				b = (byte)Math.Min(255, (b * 255 + a / 2) / a);
			}

			return (a, r, g, b);
		}

		public byte Alpha(int x, int y) => Pixel(x, y).A;

		public bool IsTransparent(int x, int y) => Pixel(x, y).A == 0;

		/// <summary>Counts the pixels that are not fully transparent inside the given box.</summary>
		public int CountPainted(int left, int top, int right, int bottom)
		{
			var count = 0;
			for (var y = Math.Max(top, 0); y < Math.Min(bottom, Height); y++)
				for (var x = Math.Max(left, 0); x < Math.Min(right, Width); x++)
					if (Pixel(x, y).A != 0)
						count++;
			return count;
		}

		/// <summary>Gets the painted bounding box (right/bottom exclusive), or <see langword="null"/> when nothing was painted.</summary>
		public (int Left, int Top, int Right, int Bottom)? PaintedBounds()
		{
			int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					if (Pixel(x, y).A == 0)
						continue;
					left = Math.Min(left, x);
					top = Math.Min(top, y);
					right = Math.Max(right, x + 1);
					bottom = Math.Max(bottom, y + 1);
				}
			}

			return right < 0 ? ((int, int, int, int)?)null : (left, top, right, bottom);
		}

		public void Dispose()
		{
			Canvas.Dispose();
			Context.Dispose();
			Surface.Dispose();
		}
	}
}
