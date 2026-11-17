using System;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(Material3FeatureSwitchTestCollection.Name)]
	public partial class ImageButtonHandlerTests
	{
		const string Material3FeatureSwitch = "Microsoft.Maui.RuntimeFeature." + nameof(RuntimeFeature.IsMaterial3Enabled);

		[Fact(DisplayName = "ImageButton uses MauiShapeableImageView with expected activity context")]
		public Task ImageButtonUsesMauiShapeableImageViewWithExpectedActivityContext()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(new ImageButtonStub());

				var platformView = Assert.IsType<MauiShapeableImageView>(handler.PlatformView);
				Assert.IsNotType<MauiMaterialContextThemeWrapper>(platformView.Context);
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

		[Theory(DisplayName = "MauiShapeableImageView uses the requested Material theme")]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public Task MauiShapeableImageViewUsesRequestedMaterialTheme(bool currentMaterial3, bool requestedMaterial3)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var currentContext = MauiMaterialContextThemeWrapper.Create(MauiContext.Context, currentMaterial3);
				var platformView = new MauiShapeableImageView(currentContext, requestedMaterial3);
				var actualContext = Assert.IsType<MauiMaterialContextThemeWrapper>(platformView.Context);

				Assert.Equal(requestedMaterial3, actualContext.UseMaterial3);
				Assert.Same(MauiContext.Context.GetActivity(), actualContext.GetActivity());

				if (currentMaterial3 == requestedMaterial3)
					Assert.Same(currentContext, actualContext);
				else
					Assert.NotSame(currentContext, actualContext);
			});
		}

		[Fact(DisplayName = "ImageButton applies Material 3 theme when enabled")]
		public Task ImageButtonAppliesMaterial3ThemeWhenEnabled()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				bool wasMaterial3Enabled = RuntimeFeature.IsMaterial3Enabled;
				AppContext.SetSwitch(Material3FeatureSwitch, true);
				try
				{
					var handler = CreateHandler(new ImageButtonStub());
					var platformView = Assert.IsType<MauiShapeableImageView>(handler.PlatformView);
					var themedContext = Assert.IsType<MauiMaterialContextThemeWrapper>(platformView.Context);

					Assert.True(themedContext.UseMaterial3);
					Assert.Same(MauiContext.Context.GetActivity(), themedContext.GetActivity());
				}
				finally
				{
					AppContext.SetSwitch(Material3FeatureSwitch, wasMaterial3Enabled);
				}
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

		[Fact(DisplayName = "Background renders when CornerRadius is larger than half the size")]
		public async Task BackgroundRendersWhenCornerRadiusIsLargerThanHalfTheSize()
		{
			Color expected = Colors.Red;

			var imageButton = new ImageButtonStub
			{
				Background = new SolidPaintStub(expected),
				CornerRadius = 100
			};

			await ValidateHasColor(imageButton, expected);
		}
		[Fact(DisplayName = "Custom CornerRadius mapping is kept after layout")]
		public async Task CustomCornerRadiusMappingIsKeptAfterLayout()
		{
			var imageButton = new ImageButtonStub
			{
				Background = new SolidPaintStub(Colors.Red),
				CornerRadius = 100,
				PropertyMapperOverrides = new PropertyMapper<IImageButton, IImageButtonHandler>(ImageButtonHandler.Mapper)
				{
					[nameof(IImageButton.CornerRadius)] = (handler, view) =>
					{
						ImageButtonHandler.MapCornerRadius(handler, view);
						handler.PlatformView.ShapeAppearanceModel = handler.PlatformView.ShapeAppearanceModel
							.ToBuilder()
							.SetAllCornerSizes(0)
							.Build();
					}
				}
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(imageButton);

				await handler.PlatformView.AttachAndRun(() =>
				{
					var cornerSize = handler.PlatformView.ShapeAppearanceModel.TopLeftCornerSize
						.GetCornerSize(new global::Android.Graphics.RectF(0, 0, handler.PlatformView.Width, handler.PlatformView.Height));

					Assert.Equal(0, cornerSize);
				});
			});
		}
		[Fact(DisplayName = "CornerRadius mapping is reapplied only when the size changes")]
		public async Task CornerRadiusMappingIsReappliedOnlyWhenTheSizeChanges()
		{
			var mappingCount = 0;
			var imageButton = new ImageButtonStub
			{
				Background = new SolidPaintStub(Colors.Red),
				CornerRadius = 100,
				PropertyMapperOverrides = new PropertyMapper<IImageButton, IImageButtonHandler>(ImageButtonHandler.Mapper)
				{
					[nameof(IImageButton.CornerRadius)] = (handler, view) =>
					{
						ImageButtonHandler.MapCornerRadius(handler, view);
						mappingCount++;
					}
				}
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(imageButton);

				await handler.PlatformView.AttachAndRun(() =>
				{
					var platformView = handler.PlatformView;

					var countBeforeMove = mappingCount;
					platformView.Layout(platformView.Left + 10, platformView.Top, platformView.Right + 10, platformView.Bottom);
					Assert.Equal(countBeforeMove, mappingCount);

					platformView.Layout(platformView.Left, platformView.Top, platformView.Right + 10, platformView.Bottom);
					Assert.Equal(countBeforeMove + 1, mappingCount);
				});
			});
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

	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class Material3FeatureSwitchTestCollection
	{
		public const string Name = "Material3FeatureSwitch";
	}
}
