using System.Collections.Generic;
using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class CustomImageCacheStub
	{
		readonly Dictionary<Color, (ColorDrawable Drawable, int Count)> _cache = new Dictionary<Color, (ColorDrawable, int)>();

		public IReadOnlyDictionary<Color, (ColorDrawable Drawable, int Count)> Cache => _cache;

		public ColorDrawable Get(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				_cache[color] = (cacheItem.Drawable, cacheItem.Count + 1);
				return cacheItem.Drawable;
			}

			var drawable = new ColorDrawable(color.ToPlatform());
			_cache[color] = (drawable, 1);
			return drawable;
		}

		public void Return(Color color)
		{
			if (_cache.TryGetValue(color, out var cacheItem))
			{
				if (cacheItem.Count == 1)
					_cache.Remove(color);
				else
					_cache[color] = (cacheItem.Drawable, cacheItem.Count - 1);
			}
		}
	}
}