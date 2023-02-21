using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class CustomImageCacheStub
	{
		readonly Dictionary<Color, (ImageSource Source, int Count)> _cache = new();

		public IReadOnlyDictionary<Color, (ImageSource Source, int Count)> Cache => _cache;

		public ImageSource Get(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				_cache[color] = (cacheItem.Source, cacheItem.Count + 1);
				return cacheItem.Source;
			}

			var bmp = new WriteableBitmap(100, 100);
			var stream = bmp.PixelBuffer.AsStream();
			var c = color.ToWindowsColor();

			for (int x = 1; x <= bmp.PixelWidth; x++)
			{
				for (int y = 1; y <= bmp.PixelHeight; y++)
				{
					stream.WriteByte(c.B);
					stream.WriteByte(c.G);
					stream.WriteByte(c.R);
					stream.WriteByte(c.A);
				}
			}

			bmp.Invalidate();

			_cache[color] = (bmp, 1);
			return bmp;
		}

		public void Return(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				if (cacheItem.Count == 1)
					_cache.Remove(color);
				else
					_cache[color] = (cacheItem.Source, cacheItem.Count + 1);
			}
		}
	}
}