#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.Graphics;

namespace Microsoft.Maui.Platform
{
	internal class ShadowCache : IDisposable
	{
		static ShadowCache _instance;
		readonly Dictionary<string, BitmapReference> _cache;
		readonly object _lock;

		public ShadowCache()
		{
			_cache = new Dictionary<string, BitmapReference>();
			_lock = new object();
		}

		public static ShadowCache Instance
		{
			get
			{
				_instance ??= new ShadowCache();

				return _instance;
			}
		}

		public void Dispose()
		{
			lock (_lock)
			{
				foreach (var bitmapReference in _cache.Values)
				{
					if (bitmapReference.Bitmap != null && !bitmapReference.Bitmap.IsDisposed())
					{
						bitmapReference.Bitmap.Recycle();
						bitmapReference.Bitmap.Dispose();
					}
				}

				_cache?.Clear();
			}
		}

		public Bitmap Add(string id, Func<Bitmap> create)
		{
			lock (_lock)
			{
				if (_cache.TryGetValue(id, out var bitmapReference))
				{
					bitmapReference.Reference++;
					return bitmapReference.Bitmap;
				}

				var bitmap = create();

				if (bitmap == null)
				{
					return null;
				}

				_cache.Add(id, new BitmapReference(bitmap, 1));

				return bitmap;
			}
		}

		public bool Remove(string id)
		{
			lock (_lock)
			{
				if (_cache.TryGetValue(id, out var bitmapReference))
				{
					bitmapReference.Reference--;

					if (bitmapReference.Reference <= 0)
					{
						_cache.Remove(id);

						if (bitmapReference.Bitmap != null && !bitmapReference.Bitmap.IsDisposed())
						{
							bitmapReference.Bitmap.Recycle();
							bitmapReference.Bitmap.Dispose();
						}
					}

					return true;
				}

				return false;
			}
		}

		public class BitmapReference
		{
			public BitmapReference(Bitmap value, int reference)
			{
				Bitmap = value;
				Reference = reference;
			}

			public Bitmap Bitmap { get; }
			public int Reference { get; set; }
		}
	}
}
