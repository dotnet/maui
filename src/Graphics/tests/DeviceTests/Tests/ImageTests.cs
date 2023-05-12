using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Xunit;
using System;

#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
using PlatformImageLoadingService = Microsoft.Maui.Graphics.Win2D.W2DImageLoadingService;
#else
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Microsoft.Maui.Graphics.DeviceTests;

public class ImageTests
{
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

		var newStream = image.AsStream(format, quality);

		Assert.NotNull(newStream);
		Assert.True(newStream.Length > 0, "Assert.True(newStream.Length > 0)");
	}

	[Theory]
	[InlineData(ImageFormat.Png, 2.0f)]
	[InlineData(ImageFormat.Png, 80f)]
	[InlineData(ImageFormat.Png, -0.8f)]
	[InlineData(ImageFormat.Jpeg, 2.0f)]
	[InlineData(ImageFormat.Jpeg, 80f)]
	[InlineData(ImageFormat.Jpeg, -0.8f)]
	public async Task AsBytesWithQualityOutOfRangeThrowsArgumentException(ImageFormat format, float quality)
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");
		using var image = service.FromStream(stream);

		Assert.Throws<ArgumentOutOfRangeException>(() => image.AsBytes(format, quality));
	}

	[Theory]
	[InlineData(ImageFormat.Png, 2.0f)]
	[InlineData(ImageFormat.Png, 80f)]
	[InlineData(ImageFormat.Png, -0.8f)]
	[InlineData(ImageFormat.Jpeg, 2.0f)]
	[InlineData(ImageFormat.Jpeg, 80f)]
	[InlineData(ImageFormat.Jpeg, -0.8f)]
	public async Task AsBytesAsyncWithQualityOutOfRangeThrowsArgumentException(ImageFormat format, float quality)
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");
		using var image = service.FromStream(stream);

		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => image.AsBytesAsync(format, quality));
	}

	[Theory]
	[InlineData(ImageFormat.Png, 2.0f)]
	[InlineData(ImageFormat.Png, 80f)]
	[InlineData(ImageFormat.Png, -0.8f)]
	[InlineData(ImageFormat.Jpeg, 2.0f)]
	[InlineData(ImageFormat.Jpeg, 80f)]
	[InlineData(ImageFormat.Jpeg, -0.8f)]
	public async Task AsStreamWithQualityOutOfRangeThrowsArgumentException(ImageFormat format, float quality)
	{
		var service = new PlatformImageLoadingService();

		using var stream = await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");
		using var image = service.FromStream(stream);

		Assert.Throws<ArgumentOutOfRangeException>(() => image.AsStream(format, quality));
	}
}
