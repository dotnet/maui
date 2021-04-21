using System;
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
			var drawable1 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(drawable1.Value);
			var bitmap1 = bitmapDrawable1.Bitmap;

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.False(collected);

			// get the image again
			var drawable2 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(drawable2.Value);
			var bitmap2 = bitmapDrawable2.Bitmap;

			// make sure it was NOT collected and wegot the same image
			Assert.Equal(bitmap1, bitmap2);
		}

		[Fact]
		public async Task ReleasingImageSourceReturnsDifferentBitmap()
		{
			var bitmapFile = CreateBitmapFile(100, 100, Colors.Red);
			var imageSource = new FileImageSourceStub(bitmapFile);

			var service = new FileImageSourceService();

			// get an image
			var drawable1 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(drawable1.Value);
			var bitmap1 = bitmapDrawable1.Bitmap;

			// release
			drawable1.Dispose();

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.True(collected);

			// get the image again
			var drawable2 = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);
			var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(drawable2.Value);
			var bitmap2 = bitmapDrawable2.Bitmap;

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(bitmap1, bitmap2);
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