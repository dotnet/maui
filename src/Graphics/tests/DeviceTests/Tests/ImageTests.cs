using System;
using System.Threading.Tasks;
#if IOS || MACCATALYST
using CoreFoundation;
using CoreGraphics;
using Foundation;
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
	[InlineData(2f)]
	[InlineData(3f)]
	public void ScaleImageUsesOneXBackingScale(float sourceScale)
	{
		var sourceSize = new CGSize(30, 20);
		using var sourceRenderer = new UIGraphicsImageRenderer(sourceSize, new UIGraphicsImageRendererFormat
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

	[Theory]
	[InlineData(0, 10)]
	[InlineData(10, 0)]
	[InlineData(-1, 10)]
	[InlineData(10, -1)]
	[InlineData(double.NaN, 10)]
	[InlineData(10, double.NaN)]
	[InlineData(double.PositiveInfinity, 10)]
	[InlineData(10, double.PositiveInfinity)]
	public void ScaleImageReturnsOriginalForNonPositiveSize(double width, double height)
	{
		using var source = CreatePatternImage(UIImageOrientation.Up);

		var scaled = source.ScaleImage(new CGSize(width, height), disposeOriginal: true);

		Assert.Same(source, scaled);
		Assert.NotNull(scaled.CGImage);
	}

	[Fact]
	public async Task ScaleImageCanRunOnBackgroundThread()
	{
		var sourceSize = new CGSize(30, 20);
		using var sourceRenderer = new UIGraphicsImageRenderer(sourceSize, new UIGraphicsImageRendererFormat
		{
			Opaque = false,
			Scale = 2,
		});

		using var source = sourceRenderer.CreateImage(context =>
		{
			UIColor.Red.SetFill();
			context.FillRect(new CGRect(CGPoint.Empty, sourceSize));
		});

		using var scaled = await Task.Run(() => source.ScaleImage(new CGSize(10, 5)));

		Assert.Equal(1, (double)scaled.CurrentScale);
		Assert.Equal(10, (int)scaled.CGImage.Width);
		Assert.Equal(5, (int)scaled.CGImage.Height);
	}

	[Fact]
	public async Task ScaleImageDoesNotRequireMainThreadProgress()
	{
		using var source = CreatePatternImage(UIImageOrientation.Up);

		Task<UIImage> scaleTask = null;
		var completedWhileMainThreadWasBlocked = false;

		void ScaleAndWait()
		{
			scaleTask = Task.Run(() => source.ScaleImage(new CGSize(10, 5)));
			completedWhileMainThreadWasBlocked = scaleTask.Wait(TimeSpan.FromSeconds(5));
		}

		if (NSThread.IsMain)
			ScaleAndWait();
		else
			DispatchQueue.MainQueue.DispatchSync(ScaleAndWait);

		Assert.True(completedWhileMainThreadWasBlocked, "ScaleImage must not synchronously depend on main-thread progress.");

		using var scaled = await scaleTask;

		Assert.Equal(10, (int)scaled.CGImage.Width);
		Assert.Equal(5, (int)scaled.CGImage.Height);
	}

	[Theory]
	[InlineData(UIImageOrientation.Up)]
	[InlineData(UIImageOrientation.Down)]
	[InlineData(UIImageOrientation.Left)]
	[InlineData(UIImageOrientation.Right)]
	[InlineData(UIImageOrientation.UpMirrored)]
	[InlineData(UIImageOrientation.DownMirrored)]
	[InlineData(UIImageOrientation.LeftMirrored)]
	[InlineData(UIImageOrientation.RightMirrored)]
	public void ScaleImageMatchesUIKitRendering(UIImageOrientation orientation)
	{
		var targetSize = new CGSize(12.25, 8.75);
		using var source = CreatePatternImage(orientation);
		using var expected = RunOnMainThread(() =>
		{
			using var format = new UIGraphicsImageRendererFormat
			{
				Opaque = false,
				PreferredRange = UIGraphicsImageRendererFormatRange.Standard,
				Scale = 1,
			};
			using var renderer = new UIGraphicsImageRenderer(targetSize, format);
			return renderer.CreateImage(_ => source.Draw(new CGRect(CGPoint.Empty, targetSize)));
		});
		using var actual = source.ScaleImage(targetSize);

		Assert.Equal(UIImageOrientation.Up, actual.Orientation);
		Assert.Equal(GetPixelData(expected), GetPixelData(actual));
		Assert.Equal(expected.CGImage.Width, actual.CGImage.Width);
		Assert.Equal(expected.CGImage.Height, actual.CGImage.Height);
		Assert.Equal(13, (double)actual.Size.Width);
		Assert.Equal(9, (double)actual.Size.Height);
	}

	private static UIImage CreatePatternImage(UIImageOrientation orientation)
	{
		using var image = RunOnMainThread(() =>
		{
			var sourceSize = new CGSize(30, 20);
			using var renderer = new UIGraphicsImageRenderer(sourceSize);
			return renderer.CreateImage(context =>
			{
				UIColor.FromRGBA(1f, 0f, 0f, 0.5f).SetFill();
				context.FillRect(new CGRect(0, 0, 20, 10));
				UIColor.Blue.SetFill();
				context.FillRect(new CGRect(20, 0, 10, 20));
				UIColor.Green.SetFill();
				context.FillRect(new CGRect(0, 10, 10, 10));
			});
		});

		return UIImage.FromImage(image.CGImage, 1, orientation);
	}

	private static byte[] GetPixelData(UIImage image)
	{
		var cgImage = image.CGImage;
		var width = checked((int)cgImage.Width);
		var height = checked((int)cgImage.Height);
		var bytesPerRow = checked(4 * width);
		var pixels = new byte[checked(bytesPerRow * height)];

		using var colorSpace = CGColorSpace.CreateDeviceRGB();
		using var context = new CGBitmapContext(
			pixels,
			width,
			height,
			8,
			bytesPerRow,
			colorSpace,
			CGBitmapFlags.ByteOrder32Little | CGBitmapFlags.PremultipliedFirst);

		context.TranslateCTM(0, height);
		context.ScaleCTM(1, -1);
		context.DrawImage(new CGRect(0, 0, width, height), cgImage);

		return pixels;
	}

	private static T RunOnMainThread<T>(Func<T> action)
	{
		if (NSThread.IsMain)
			return action();

		T result = default!;
		DispatchQueue.MainQueue.DispatchSync(() => result = action());
		return result;
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
