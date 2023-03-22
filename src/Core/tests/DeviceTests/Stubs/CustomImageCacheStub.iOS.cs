using System.Collections.Generic;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class CustomImageCacheStub
	{
		readonly Dictionary<Color, (UIImage Image, int Count)> _cache = new Dictionary<Color, (UIImage, int)>();

		public IReadOnlyDictionary<Color, (UIImage Image, int Count)> Cache => _cache;

		public UIImage Get(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				_cache[color] = (cacheItem.Image, cacheItem.Count + 1);
				return cacheItem.Image;
			}

			var rect = new CGRect(0, 0, 100, 100);

			UIGraphics.BeginImageContextWithOptions(rect.Size, false, 1);
			var context = UIGraphics.GetCurrentContext();

			color.ToPlatform().SetFill();
			context.FillRect(rect);

			var image = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

			_cache[color] = (image, 1);

			return image;
		}

		public void Return(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				if (cacheItem.Count == 1)
					_cache.Remove(color);
				else
					_cache[color] = (cacheItem.Image, cacheItem.Count + 1);
			}
		}
	}
}