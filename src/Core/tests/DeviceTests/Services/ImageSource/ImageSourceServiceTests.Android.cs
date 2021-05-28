using System;
using System.Reflection;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Java.Util.Concurrent;
using Microsoft.Maui.BumptechGlide;
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
			var result1 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(result1.Value);
			var bitmap1 = bitmapDrawable1.Bitmap;

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.False(collected);

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(result2.Value);
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
			var result1 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(result1.Value);
			var bitmap1 = bitmapDrawable1.Bitmap;

			// release
			result1.Dispose();

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.True(collected);

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(result2.Value);
			var bitmap2 = bitmapDrawable2.Bitmap;

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(bitmap1, bitmap2);

			result2.Dispose();
		}

		[Fact]
		public void GlideStaticEqualsGlideGet()
		{
			var fromGet = Glide.Get(Platform.DefaultContext);

			var manager = Glide.With(Platform.DefaultContext);
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
			var result1 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			Assert.Equal(1, cache.Cache.Count);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			Assert.Equal(1, cache.Cache.Count);
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
			var result1 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			Assert.Equal(1, cache.Cache.Count);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// release
			result1.Dispose();

			// get the image again
			var result2 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			Assert.Equal(1, cache.Cache.Count);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(result1.Value, result2.Value);

			result2.Dispose();
		}

		static async Task<bool> TryCollectFile(string bitmapFile)
		{
			var collected = false;

			for (var i = 0; i < GCCollectRetries; i++)
			{
				GC.Collect();

				try
				{
					// the OnlyRetrieveFromCache means that if it is not already loaded, then throw
					await Glide
						.With(Platform.DefaultContext)
						.Load(bitmapFile, Platform.DefaultContext)
						.SetOnlyRetrieveFromCache(true)
						.SetDiskCacheStrategy(DiskCacheStrategy.None)
						.Submit()
						.AsTask();
				}
				catch (ExecutionException ex) when (ex.Cause is GlideException)
				{
					// no-op becasue we are waiting for this
					collected = true;
				}
			}

			return collected;
		}
	}
}