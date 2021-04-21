using System;
using System.Collections.Concurrent;

namespace Microsoft.Maui
{
	public static class ImageSourceManager
	{
		static ConcurrentDictionary<string, (object img, int count)> ImageCache = new();

		public static void Add(string source, object img)
		{
			_ = source ?? throw new NullReferenceException();
			_ = img ?? throw new NullReferenceException();

			if (ImageCache.TryGetValue(source, out var item))
			{
				item.count++;
				ImageCache[source] = item;
			}
			else
				ImageCache.TryAdd(source, (img, 1));
		}

		public static void Add(IImageSource imageSource, object img)
		{
			_ = imageSource ?? throw new NullReferenceException();
			if (imageSource is IFileImageSource fileImageSource)
				Add(fileImageSource.File, img);
		}

		public static void Remove(string source)
		{
			_ = source ?? throw new NullReferenceException();

			if (!ImageCache.TryGetValue(source, out var item))
				return;

			item.count--;

			if (item.count >= 0)
				return;

			ImageCache.TryRemove(source, out var removedItem);

			if (removedItem.img is IDisposable dis)
				dis.Dispose();
		}
	}
}
