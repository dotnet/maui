using System;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
		[Fact(DisplayName = "ImageButton uses MauiShapeableImageView with expected activity context")]
		public Task ImageButtonUsesMauiShapeableImageViewWithExpectedActivityContext()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(new ImageButtonStub());

				var platformView = Assert.IsType<MauiShapeableImageView>(handler.PlatformView);
				Assert.Same(MauiContext.Context.GetActivity(), platformView.Context.GetActivity());
			});
		}

		[Fact(DisplayName = "MauiShapeableImageView preserves public constructor context behavior")]
		public Task MauiShapeableImageViewPreservesPublicConstructorContextBehavior()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformView = new MauiShapeableImageView(MauiContext.Context);

				Assert.Same(MauiContext.Context.GetActivity(), platformView.Context.GetActivity());
				Assert.IsNotType<MauiMaterialContextThemeWrapper>(platformView.Context);
			});
		}

		[Fact(DisplayName = "MauiShapeableImageView applies Material 3 theme when requested")]
		public Task MauiShapeableImageViewAppliesMaterial3ThemeWhenRequested()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformView = new MauiShapeableImageView(MauiContext.Context, useMaterial3: true);

				Assert.IsType<MauiMaterialContextThemeWrapper>(platformView.Context);
				Assert.Same(MauiContext.Context.GetActivity(), platformView.Context.GetActivity());
			});
		}

		[Fact(DisplayName = "MauiShapeableImageView clears combined padding after measure")]
		public Task MauiShapeableImageViewClearsCombinedPaddingAfterMeasure()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformView = new MauiShapeableImageView(MauiContext.Context);
				platformView.SetContentPadding(1, 2, 3, 4);
				platformView.SetPadding(5, 6, 7, 8);

				var measureSpec = View.MeasureSpec.MakeMeasureSpec(100, MeasureSpecMode.Exactly);
				platformView.Measure(measureSpec, measureSpec);

				Assert.Equal(0, platformView.PaddingLeft);
				Assert.Equal(0, platformView.PaddingTop);
				Assert.Equal(0, platformView.PaddingRight);
				Assert.Equal(0, platformView.PaddingBottom);
			});
		}

		[Fact(DisplayName = "Clip ImageButton with Background works Correctly")]
		public async Task ClipImageButtonWithBackgroundWorks()
		{
			Color expected = Colors.Yellow;

			var brush = new SolidPaintStub(expected);

			var imageButton = new ImageButtonStub
			{
				Background = brush,
				Clip = new EllipseShapeStub()
			};

			await ValidateHasColor(imageButton, expected);
		}

		Google.Android.Material.ImageView.ShapeableImageView GetPlatformImageButton(ImageButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task PerformClick(IImageButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetPlatformImageButton(CreateHandler(button)).PerformClick();
			});
		}

		bool ImageSourceLoaded(ImageButtonHandler imageButtonHandler) =>
			imageButtonHandler.PlatformView.Drawable != null;
	}
}