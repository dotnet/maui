using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// Gaussian-like blur (three box blur passes) over a premultiplied ARGB32 Cairo image surface.
	/// </summary>
	internal static class SurfaceBlur
	{
		public static void Apply(CairoImageSurface surface, double sigma)
		{
			if (sigma <= 0.3)
				return;

			surface.Flush();
			var width = surface.Width;
			var height = surface.Height;
			var stride = surface.Stride;
			var length = stride * height;
			var data = new byte[length];
			Marshal.Copy(surface.DataPtr, data, 0, length);

			var temp = new byte[length];
			var sizes = BoxSizes(sigma);
			for (var pass = 0; pass < 3; pass++)
			{
				var radius = (sizes[pass] - 1) / 2;
				if (radius < 1)
					continue;

				BoxBlurHorizontal(data, temp, width, height, stride, radius);
				BoxBlurVertical(temp, data, width, height, stride, radius);
			}

			Marshal.Copy(data, 0, surface.DataPtr, length);
			surface.MarkDirty();
		}

		// Box sizes whose successive application approximates a Gaussian of the given sigma.
		private static int[] BoxSizes(double sigma)
		{
			const int n = 3;
			var ideal = Math.Sqrt(12 * sigma * sigma / n + 1);
			var wl = (int)Math.Floor(ideal);
			if (wl % 2 == 0)
				wl--;
			var wu = wl + 2;
			var m = Math.Round((12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4d * wl - 4));

			var sizes = new int[n];
			for (var i = 0; i < n; i++)
				sizes[i] = i < m ? wl : wu;
			return sizes;
		}

		private static void BoxBlurHorizontal(byte[] source, byte[] target, int width, int height, int stride, int radius)
		{
			var window = 2 * radius + 1;
			for (var y = 0; y < height; y++)
			{
				var row = y * stride;
				for (var channel = 0; channel < 4; channel++)
				{
					var sum = 0;
					// Pixels outside the surface are transparent.
					for (var x = 0; x <= radius && x < width; x++)
						sum += source[row + x * 4 + channel];

					for (var x = 0; x < width; x++)
					{
						target[row + x * 4 + channel] = (byte)((sum + window / 2) / window);

						var add = x + radius + 1;
						if (add < width)
							sum += source[row + add * 4 + channel];
						var remove = x - radius;
						if (remove >= 0)
							sum -= source[row + remove * 4 + channel];
					}
				}
			}
		}

		private static void BoxBlurVertical(byte[] source, byte[] target, int width, int height, int stride, int radius)
		{
			var window = 2 * radius + 1;
			for (var x = 0; x < width; x++)
			{
				for (var channel = 0; channel < 4; channel++)
				{
					var column = x * 4 + channel;
					var sum = 0;
					for (var y = 0; y <= radius && y < height; y++)
						sum += source[y * stride + column];

					for (var y = 0; y < height; y++)
					{
						target[y * stride + column] = (byte)((sum + window / 2) / window);

						var add = y + radius + 1;
						if (add < height)
							sum += source[add * stride + column];
						var remove = y - radius;
						if (remove >= 0)
							sum -= source[remove * stride + column];
					}
				}
			}
		}
	}
}
