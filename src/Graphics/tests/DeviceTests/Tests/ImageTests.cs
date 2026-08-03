using System;
using System.Threading.Tasks;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;
using Xunit;

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

#if IOS || MACCATALYST
	[Theory]
	[InlineData(1f)]
	[InlineData(3f)]
	public void ScaleImageUsesOneXBackingScale(float sourceScale)
	{
		var sourceSize = new CGSize(30, 20);
		var sourceRenderer = new UIGraphicsImageRenderer(sourceSize, new UIGraphicsImageRendererFormat
		{
			Opaque = false,
			Scale = sourceScale,
		});

		using var source = sourceRenderer.CreateImage(context =>
		{
			UIColor.Red.SetFill();
			context.FillRect(new CGRect(CGPoint.Empty, sourceSize));
		});

		using var scaled = source.ScaleImage(new CGSize(10, 5));

		Assert.Equal(1, (double)scaled.CurrentScale);
		Assert.Equal(10, (double)scaled.Size.Width);
		Assert.Equal(5, (double)scaled.Size.Height);
		Assert.NotNull(scaled.CGImage);
		Assert.Equal(10, (int)scaled.CGImage.Width);
		Assert.Equal(5, (int)scaled.CGImage.Height);
	}
#endif

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
