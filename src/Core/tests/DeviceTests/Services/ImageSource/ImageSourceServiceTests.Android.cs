using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request.Transition;
using Java.Util.Concurrent;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageSourceServiceTests : BaseImageSourceServiceTests
	{
		const int GCCollectRetries = 100;

		[Fact]
		public async Task TheSameImageSourceReturnsTheSameBitmap()
		{
			var bitmapFile = CreateBitmapFile(100, 100, Colors.Red);
			var imageSource = new FileImageSourceStub(bitmapFile);

			var service = new FileImageSourceService();

			// get an image
			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var drawable1 = result1.Value;
			var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(drawable1);
			var bitmap1 = bitmapDrawable1.Bitmap;

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.False(collected);

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var drawable2 = result2.Value;
			var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(drawable2);
			var bitmap2 = bitmapDrawable2.Bitmap;

			// make sure it was NOT collected and we got the same image
			Assert.Equal(bitmap1, bitmap2);

			result1.Dispose();
			result2.Dispose();
		}

		[Fact]
		public async Task ReleasingImageSourceReturnsDifferentBitmap()
		{
			var bitmapFile = CreateBitmapFile(100, 100, Colors.Red);
			var imageSource = new FileImageSourceStub(bitmapFile);

			var service = new FileImageSourceService();

			// get an image
			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var drawable1 = result1.Value;
			var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(drawable1);
			var bitmap1 = bitmapDrawable1.Bitmap;

			// release
			result1.Dispose();

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.True(collected);

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var drawable2 = result2.Value;
			var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(drawable2);
			var bitmap2 = bitmapDrawable2.Bitmap;

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(bitmap1, bitmap2);

			result2.Dispose();
		}

		[Fact]
		public void GlideStaticEqualsGlideGet()
		{
			var fromGet = Glide.Get(MauiProgram.DefaultContext);

			var manager = Glide.With(MauiProgram.DefaultContext);
			var glideField = manager.GetType().GetProperty("Glide", BindingFlags.NonPublic | BindingFlags.Instance);
			var fromField = glideField.GetValue(manager);

			Assert.Equal(fromGet, fromField);
		}

		[Fact]
		public async Task CustomTheSameImageSourceReturnsTheSameBitmap()
		{
			var imageSource = new CustomImageSourceStub(Colors.Red);

			var cache = new CustomImageCacheStub();
			var service = new CustomImageSourceServiceStub(cache);

			// get an image
			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			Assert.Single(cache.Cache);
			Assert.Equal(2, cache.Cache[imageSource.Color].Count);

			// make sure it was NOT collected and we got the same image
			Assert.Equal(result1.Value, result2.Value);

			result1.Dispose();
			result2.Dispose();
		}

		[Fact]
		public async Task CustomReleasingImageSourceReturnsDifferentBitmap()
		{
			var imageSource = new CustomImageSourceStub(Colors.Red);

			var cache = new CustomImageCacheStub();
			var service = new CustomImageSourceServiceStub(cache);

			// get an image
			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var drawable1 = result1.Value;
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// release
			result1.Dispose();

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var drawable2 = result2.Value;
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(drawable1, drawable2);

			result2.Dispose();
		}

		[Fact]
		public async Task LoadDrawableAsyncCorrectlyFetchesDrawable()
		{
			var imageSource = new CustomImageSourceStub(Colors.Red);

			var cache = new CustomImageCacheStub();
			var service = new LoadDrawableAsyncImageSourceServiceStub(cache);

			var imageView = new ImageView(MauiProgram.DefaultContext);

			// get an image
			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// get the image again as a load
			var result2 = await service.LoadDrawableAsync(imageSource, imageView);
			Assert.Single(cache.Cache);
			Assert.Equal(2, cache.Cache[imageSource.Color].Count);

			// make sure it was NOT collected and we got the same image
			Assert.Equal(result1.Value, imageView.Drawable);

			result1.Dispose();
			result2.Dispose();
		}

		[Fact]
		public async Task DisposingLoadDrawableAsyncDoesNotDisposeRealBitmap()
		{
			var imageSource = new CustomImageSourceStub(Colors.Red);

			var cache = new CustomImageCacheStub();
			var service = new LoadDrawableAsyncImageSourceServiceStub(cache);

			var imageView = new ImageView(MauiProgram.DefaultContext);

			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var result2 = await service.LoadDrawableAsync(imageSource, imageView);

			// dispose proxy
			result2.Dispose();

			// ensure the count went down
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			result1.Dispose();
		}

		[Fact]
		public async Task DisposingLoadDrawableAsyncBaseDoesNotDisposeRealBitmap()
		{
			var imageSource = new CustomImageSourceStub(Colors.Red);

			var cache = new CustomImageCacheStub();
			var service = new LoadDrawableAsyncImageSourceServiceStub(cache);

			var imageView = new ImageView(MauiProgram.DefaultContext);

			var result1 = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var result2 = await service.LoadDrawableAsync(imageSource, imageView);

			// dispose drawable
			result1.Dispose();

			// ensure the count went down
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			result2.Dispose();
		}

		async Task<bool> TryCollectFile(string bitmapFile)
		{
			var collected = false;

			for (var i = 0; i < GCCollectRetries && !collected; i++)
			{
				await WaitForGC();

				var target = new CacheCheckTarget();

				try
				{
					// the OnlyRetrieveFromCache means that if it is not already loaded, then throw
					Glide
						.With(MauiProgram.DefaultContext)
						.Load(bitmapFile)
						.SetOnlyRetrieveFromCache(true)
						.SetDiskCacheStrategy(DiskCacheStrategy.None)
						.Into(target);

					var loadedFromCache = await target.DidLoadFromCache.ConfigureAwait(false);
					collected = !loadedFromCache;
				}
				catch (ExecutionException ex) when (ex.Cause is GlideException)
				{
					// no-op becasue we are waiting for this
					collected = true;
				}
				catch (GlideException)
				{
					// no-op becasue we are waiting for this
					collected = true;
				}
			}

			return collected;
		}

		class CacheCheckTarget : Bumptech.Glide.Request.Target.CustomTarget
		{
			public Task<bool> DidLoadFromCache
				=> tcsResult.Task;

			TaskCompletionSource<bool> tcsResult = new();

			public override void OnLoadFailed(Drawable errorDrawable)
			{
				base.OnLoadFailed(errorDrawable);

				tcsResult.SetResult(false);
			}
			public override void OnLoadCleared(Drawable p0)
			{
			}

			public override void OnResourceReady(Java.Lang.Object resource, ITransition transition)
			{
				tcsResult.TrySetResult(true);
			}
		}

		class LoadDrawableAsyncImageSourceServiceStub : ImageSourceService
		{
			readonly CustomImageCacheStub _cache;

			public LoadDrawableAsyncImageSourceServiceStub(CustomImageCacheStub cache)
			{
				_cache = cache;
			}

			public override Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
			{
				if (imageSource is not ICustomImageSourceStub imageSourceStub)
					return Task.FromResult<IImageSourceServiceResult<Drawable>>(null);

				var color = imageSourceStub.Color;

				var drawable = _cache.Get(color);

				var result = new ImageSourceServiceResult(drawable, () => _cache.Return(color));

				return Task.FromResult<IImageSourceServiceResult<Drawable>>(result);
			}
		}
	}
}