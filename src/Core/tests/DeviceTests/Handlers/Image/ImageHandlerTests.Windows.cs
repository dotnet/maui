using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests<TImageHandler, TStub>
	{
		WImage GetPlatformImageView(IImageHandler imageHandler) =>
			imageHandler.PlatformView;

		bool GetNativeIsAnimationPlaying(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).Source is BitmapImage bitmapImage && bitmapImage.IsPlaying;

		Aspect GetNativeAspect(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).Stretch switch
			{
				WStretch.Uniform => Aspect.AspectFit,
				WStretch.UniformToFill => Aspect.AspectFill,
				WStretch.Fill => Aspect.Fill,
				WStretch.None => Aspect.Center,
				_ => throw new ArgumentOutOfRangeException("Stretch")
			};

		// https://github.com/dotnet/maui/issues/32393
		// OnImageOpened must call UpdatePlatformMaxConstraints so PlatformView.MaxWidth
		// is capped to the decoded pixel size, preventing AspectFit images from filling the container.
		[Fact]
		public async Task AspectFit_AfterDecode_PlatformMaxWidthIsConstrainedToNaturalPixelSize()
		{
			var image = new TStub
			{
				Aspect = Aspect.AspectFit,
				HorizontalLayoutAlignment = LayoutAlignment.Center,
				VerticalLayoutAlignment = LayoutAlignment.Center,
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);
				var platformView = GetPlatformImageView(handler);

				await AttachAndRun(platformView, async () =>
				{
					await image.WaitUntilDecoded();

					var bitmapSource = platformView.Source as BitmapSource;
					Assert.NotNull(bitmapSource);
					Assert.True(bitmapSource.PixelWidth > 0);
					Assert.True(platformView.MaxWidth <= bitmapSource.PixelWidth + 0.5,
						$"MaxWidth ({platformView.MaxWidth:F0}) should be <= PixelWidth ({bitmapSource.PixelWidth}).");

					var desiredSize = handler.GetDesiredSize(2000d, 2000d);
					Assert.True(desiredSize.Width <= bitmapSource.PixelWidth + 0.5,
						$"GetDesiredSize width={desiredSize.Width:F0} exceeds PixelWidth={bitmapSource.PixelWidth}.");
					Assert.True(desiredSize.Height <= bitmapSource.PixelHeight + 0.5,
						$"GetDesiredSize height={desiredSize.Height:F0} exceeds PixelHeight={bitmapSource.PixelHeight}.");
				});
			});
		}

		// https://github.com/dotnet/maui/issues/32393
		// GetDesiredSize must use _cachedImageSize (set in OnImageOpened) during source transitions.
		// The cache persists while a new source is decoding so layout stays capped to the previous
		// natural size instead of expanding to fill the container (PixelWidth=0 while pending).
		// The race window is frozen by replacing platformView.Source with a blank BitmapImage
		// (PixelWidth=0 always) and setting platformView.Width so base.GetDesiredSize returns large.
		[Fact]
		public async Task AspectFit_GetDesiredSize_DuringSourceTransition_UsesCachedNaturalSize()
		{
			var image = new TStub
			{
				Aspect = Aspect.AspectFit,
				HorizontalLayoutAlignment = LayoutAlignment.Center,
				VerticalLayoutAlignment = LayoutAlignment.Center,
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);
				var platformView = GetPlatformImageView(handler);

				await AttachAndRun(platformView, async () =>
				{
					// wait for initial decode so _cachedImageSize is populated
					await image.WaitUntilDecoded();

					var bitmapSource = platformView.Source as BitmapSource;
					Assert.NotNull(bitmapSource);
					int naturalPixelWidth = bitmapSource.PixelWidth;
					int naturalPixelHeight = bitmapSource.PixelHeight;
					Assert.True(naturalPixelWidth > 0);

					// simulate the race window: MaxWidth reset, source replaced with a
					// blank BitmapImage (PixelWidth=0) before decode completes
					const double largeConstraint = 2000d;
					platformView.MaxWidth = double.PositiveInfinity;
					platformView.MaxHeight = double.PositiveInfinity;
					platformView.Width = largeConstraint;
					platformView.Source = new BitmapImage();

					Assert.Equal(0, ((BitmapImage)platformView.Source).PixelWidth);

					var desiredSize = handler.GetDesiredSize(largeConstraint, largeConstraint);
					Assert.True(desiredSize.Width <= naturalPixelWidth + 0.5,
						$"GetDesiredSize width={desiredSize.Width:F0} exceeds naturalPixelWidth={naturalPixelWidth} during source transition (issue #32393).");
					Assert.True(desiredSize.Height <= naturalPixelHeight + 0.5,
						$"GetDesiredSize height={desiredSize.Height:F0} exceeds naturalPixelHeight={naturalPixelHeight} during source transition.");
				});
			});
		}
	}

	// These tests are only for ImageHandler: ImageButtonHandler does not cap the size of its image.
	public partial class ImageHandlerTests
	{
		// https://github.com/dotnet/maui/issues/34968
		// FileImageSourceService loads a file path with BitmapImage.SetSourceAsync, so the bitmap is
		// already decoded when it is set and WinUI does not raise ImageOpened for it.
		[Fact]
		public async Task AspectFit_ImageFromFilePath_PlatformMaxSizeIsConstrainedToPixelSize()
		{
			var image = new ImageStub
			{
				Aspect = Aspect.AspectFit,
				HorizontalLayoutAlignment = LayoutAlignment.Center,
				VerticalLayoutAlignment = LayoutAlignment.Center,
				Source = new FileImageSourceStub(BaseImageSourceServiceTests.CreateBitmapFile(60, 30, Colors.Red)),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);
				var platformView = handler.PlatformView;

				await AttachAndRun(platformView, async () =>
				{
					await image.WaitUntilLoaded();

					var bitmapSource = Assert.IsAssignableFrom<BitmapSource>(platformView.Source);
					Assert.Equal(60, bitmapSource.PixelWidth);
					Assert.Equal(30, bitmapSource.PixelHeight);
					AssertMaxSizeIsPixelSize(platformView, bitmapSource);
				});
			});
		}

		// https://github.com/dotnet/maui/issues/34968
		// A custom image source service can return a WriteableBitmap. It has its size as soon as it
		// is created and WinUI does not raise ImageOpened for it.
		[Fact]
		public async Task AspectFit_WriteableBitmap_PlatformMaxSizeIsConstrainedToPixelSize()
		{
			var image = new ImageStub
			{
				Aspect = Aspect.AspectFit,
				HorizontalLayoutAlignment = LayoutAlignment.Center,
				VerticalLayoutAlignment = LayoutAlignment.Center,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);
				var platformView = handler.PlatformView;

				await AttachAndRun(platformView, () =>
				{
					// This is what the source loader does with the bitmap returned by such a service.
					var bitmap = BaseImageSourceServiceTests.CreateBitmap(60, 30, Colors.Red);
					handler.SourceLoader.Setter.SetImageSource(bitmap);

					AssertMaxSizeIsPixelSize(platformView, bitmap);
				});
			});
		}

		static void AssertMaxSizeIsPixelSize(WImage platformView, BitmapSource bitmap)
		{
			Assert.True(platformView.MaxWidth <= bitmap.PixelWidth + 0.5,
				$"MaxWidth ({platformView.MaxWidth:F0}) should be <= PixelWidth ({bitmap.PixelWidth}).");
			Assert.True(platformView.MaxHeight <= bitmap.PixelHeight + 0.5,
				$"MaxHeight ({platformView.MaxHeight:F0}) should be <= PixelHeight ({bitmap.PixelHeight}).");
		}
	}
}
