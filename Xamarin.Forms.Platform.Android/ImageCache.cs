using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Android.Runtime;

namespace Xamarin.Forms.Platform.Android
{
	/// <summary>
	/// I setup the access to all the cache elements to be async because
	/// if I didn't then it was locking up the GC and freezing the entire app
	/// </summary>
	class ImageCache
	{
		readonly FormsLruCache _lruCache;
		readonly ConcurrentDictionary<string, SemaphoreSlim> _waiting;

		public ImageCache() : base()
		{
			_waiting = new ConcurrentDictionary<string, SemaphoreSlim>();
			_lruCache = new FormsLruCache();
		}

		void Put(string key, TimeSpan cacheValidity, global::Android.Graphics.Bitmap cacheObject)
		{
			_lruCache.Put(key, new CacheEntry() { TimeToLive = DateTimeOffset.UtcNow.Add(cacheValidity), Data = cacheObject });
		}

		public Task<Java.Lang.Object> GetAsync(string cacheKey, TimeSpan cacheValidity, Func<Task<Java.Lang.Object>> createMethod)
		{
			return Task.Run(async () =>
			{
				SemaphoreSlim semaphoreSlim = null;
				Java.Lang.Object innerCacheObject = null;

				try
				{
					semaphoreSlim = _waiting.GetOrAdd(cacheKey, (key) => new SemaphoreSlim(1, 1));
					await semaphoreSlim.WaitAsync().ConfigureAwait(false);

					var cacheEntry = _lruCache.Get(cacheKey) as CacheEntry;

					if (cacheEntry?.TimeToLive < DateTimeOffset.UtcNow || cacheEntry?.IsDisposed == true)
						cacheEntry = null;

					if (cacheEntry == null && createMethod != null)
					{
						innerCacheObject = await createMethod().ConfigureAwait(false);
						if (innerCacheObject is global::Android.Graphics.Bitmap bm)
							Put(cacheKey, cacheValidity, bm);
						else if (innerCacheObject is global::Android.Graphics.Drawables.BitmapDrawable bitmap)
							Put(cacheKey, cacheValidity, bitmap.Bitmap);
					}
					else
					{
						innerCacheObject = cacheEntry.Data;
					}
				}
				catch
				{
					//just in case
				}
				finally
				{
					semaphoreSlim?.Release();
				}

				return innerCacheObject;
			});
		}

		internal class CacheEntry : Java.Lang.Object
		{
			bool _isDisposed;

			public CacheEntry(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
			{
			}

			public CacheEntry()
			{
			}

			public bool IsDisposed
			{
				get
				{
					if (Data == null)
						return true;

					if (this.IsDisposed() || Data.IsDisposed())
						return true;

					return false;
				}
			}

			public DateTimeOffset TimeToLive { get; set; }
			public global::Android.Graphics.Bitmap Data { get; set; }

			protected override void Dispose(bool disposing)
			{
				if (!_isDisposed)
				{
					_isDisposed = true;
					Data = null;
				}

				base.Dispose(disposing);
			}
		}

		public class FormsLruCache : global::Android.Util.LruCache
		{

			static int GetCacheSize()
			{
				// https://developer.android.com/topic/performance/graphics/cache-bitmap
				int cacheSize = 4 * 1024 * 1024;
				var maxMemory = Java.Lang.Runtime.GetRuntime()?.MaxMemory();
				if (maxMemory != null)
				{
					cacheSize = (int)(maxMemory.Value / 8);
				}
				return cacheSize;
			}

			public FormsLruCache() : base(GetCacheSize())
			{
			}

			protected override int SizeOf(Java.Lang.Object key, Java.Lang.Object value)
			{
				if (value != null && value is global::Android.Graphics.Bitmap bitmap)
					return bitmap.ByteCount / 1024;

				return base.SizeOf(key, value);
			}
		}

	}
}