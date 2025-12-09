using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class CustomImageCacheStub
	{
		readonly Dictionary<Color, (UIImage Image, int Count)> _cache = new Dictionary<Color, (UIImage, int)>();

		public IReadOnlyDictionary<Color, (UIImage Image, int Count)> Cache => _cache;


		public async Task<UIImage> Get(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				_cache[color] = (cacheItem.Image, cacheItem.Count + 1);
				return cacheItem.Image;
			}

			var rect = new CGRect(0, 0, 100, 100);

			return await MainThread.InvokeOnMainThreadAsync(() =>
			{

				var renderer = new UIGraphicsImageRenderer(rect.Size, new UIGraphicsImageRendererFormat()
				{
					Opaque = false,
					Scale = 1,
				});

				var image = renderer.CreateImage((context) =>
					{
						color.ToPlatform().SetFill();
						context.FillRect(rect);
					}).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				_cache[color] = (image, 1);

				return image;
			});
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