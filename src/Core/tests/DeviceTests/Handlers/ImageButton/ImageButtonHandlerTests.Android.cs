using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
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
}