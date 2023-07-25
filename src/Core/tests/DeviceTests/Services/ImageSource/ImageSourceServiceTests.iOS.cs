using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageSourceServiceTests : BaseImageSourceServiceTests
	{
		const int GCCollectRetries = 100;

		// TODO: implement a "native" caching system everywhere on ios and update this
		[Fact(Skip = "iOS has no caching mechanism yet")]
		public async Task TheSameImageSourceReturnsTheSameBitmap()
		{
			var bitmapFile = CreateBitmapFile(100, 100, Colors.Red);
			var imageSource = new FileImageSourceStub(bitmapFile);

			var service = new FileImageSourceService();

			// get an image
			var result1 = await service.GetImageAsync(imageSource);
			var uiimage1 = Assert.IsType<UIImage>(result1.Value);

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.False(collected);

			// get the image again
			var result2 = await service.GetImageAsync(imageSource);
			var uiimage2 = Assert.IsType<UIImage>(result2.Value);

			// make sure it was NOT collected and we got the same image
			Assert.Equal(uiimage1, uiimage2);

			result1.Dispose();
			result2.Dispose();
		}

		// TODO: implement a "native" caching system everywhere on ios and update this
		[Fact(Skip = "iOS has no caching mechanism yet")]
		public async Task ReleasingImageSourceReturnsDifferentBitmap()
		{
			var bitmapFile = CreateBitmapFile(100, 100, Colors.Red);
			var imageSource = new FileImageSourceStub(bitmapFile);

			var service = new FileImageSourceService();

			// get an image
			var result1 = await service.GetImageAsync(imageSource);
			var uiimage1 = Assert.IsType<UIImage>(result1.Value);

			// release
			result1.Dispose();

			// try collect it
			var collected = await TryCollectFile(bitmapFile);
			Assert.True(collected);

			// get the image again
			var result2 = await service.GetImageAsync(imageSource);
			var uiimage2 = Assert.IsType<UIImage>(result2.Value);

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(uiimage1, uiimage2);

			result2.Dispose();
		}

		[Fact]
		public async Task CustomTheSameImageSourceReturnsTheSameBitmap()
		{
			var imageSource = new CustomImageSourceStub(Colors.Red);

			var cache = new CustomImageCacheStub();
			var service = new CustomImageSourceServiceStub(cache);

			// get an image
			var result1 = await service.GetImageAsync(imageSource);
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// get the image again
			var result2 = await service.GetImageAsync(imageSource);
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
			var result1 = await service.GetImageAsync(imageSource);
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// release
			result1.Dispose();

			// get the image again
			var result2 = await service.GetImageAsync(imageSource);
			Assert.Single(cache.Cache);
			Assert.Equal(1, cache.Cache[imageSource.Color].Count);

			// make sure it WAS collected and we got a new image
			Assert.NotEqual(result1.Value, result2.Value);

			result2.Dispose();
		}

		static Task<bool> TryCollectFile(string bitmapFile)
		{
			var collected = false;

			for (var i = 0; i < GCCollectRetries; i++)
			{
				GC.Collect();

				// TODO: implement a "native" caching system everywhere on ios and update this
			}

			return Task.FromResult(collected);
		}
	}
}