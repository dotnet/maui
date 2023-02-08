using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Microsoft.Maui.Dispatching;
using Xunit;

#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
using PlatformImageLoadingService = Microsoft.Maui.Graphics.Win2D.W2DImageLoadingService;
#else
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Microsoft.Maui.Graphics.DeviceTests;

public class ImageLoadingServiceTests
{
	[Fact]
	public async Task CanLoadImageOnMainThread()
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");

		using var image = await TestDispatcher.Current.DispatchAsync(() => service.FromStream(stream));

		Assert.NotNull(image);
	}

	[Fact]
	public async Task CanLoadImageOnBackgroundThread()
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");

		using var image = await Task.Run(() => service.FromStream(stream));

		Assert.NotNull(image);
	}

	[Fact]
	public async Task CanLoadImage()
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");

		using var image = service.FromStream(stream);

		Assert.NotNull(image);
		Assert.Equal(747, image.Width);
		Assert.Equal(840, image.Height);
	}

	[Theory]
	[InlineData(ImageFormat.Png, 1.0f)]
	[InlineData(ImageFormat.Png, 0.8f)]
	[InlineData(ImageFormat.Png, 0.4f)]
	[InlineData(ImageFormat.Jpeg, 1.0f)]
	[InlineData(ImageFormat.Jpeg, 0.8f)]
	[InlineData(ImageFormat.Jpeg, 0.4f)]
	public async Task CanGetBytesFromImage(ImageFormat format, float quality)
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");
		using var image = service.FromStream(stream);

		var bytes = image.AsBytes(format, quality);

		Assert.NotNull(bytes);
		Assert.NotEmpty(bytes);
	}

	[Theory]
	[InlineData(ImageFormat.Png, 1.0f)]
	[InlineData(ImageFormat.Png, 0.8f)]
	[InlineData(ImageFormat.Png, 0.4f)]
	[InlineData(ImageFormat.Jpeg, 1.0f)]
	[InlineData(ImageFormat.Jpeg, 0.8f)]
	[InlineData(ImageFormat.Jpeg, 0.4f)]
	public async Task CanGetStreamFromImage(ImageFormat format, float quality)
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");
		using var image = service.FromStream(stream);

		var stream = image.AsStream(format, quality);

		Assert.NotNull(stream);
		Assert.True(stream.Length > 0);
	}
}
