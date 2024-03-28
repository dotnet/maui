using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls.Primitives;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SliderHandlerTests
	{
		[Fact(DisplayName = "Thumb Color Initializes Correctly")]
		public async Task ThumbColorInitializesCorrectly()
		{
			var slider = new SliderStub()
			{
				ThumbColor = Colors.Purple
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(slider);

				await handler.PlatformView.AttachAndRun(async () =>
				{
					await AssertEventually(() => handler.PlatformView.IsLoaded());

					await ValidateNativeThumbColor(slider, Colors.Purple);
				}, MauiContext);
			});

		}

		// https://github.com/dotnet/maui/issues/12405
		[Theory(DisplayName = "Platform Slider SmallChange Initializes Correctly")]
		[InlineData(0, 1, 0)]
		[InlineData(0, 1, 0.5)]
		[InlineData(0, 1, 1)]
		[InlineData(0, 100, 0)]
		[InlineData(0, 100, 1)]
		[InlineData(0, 100, 5)]
		[InlineData(0, 100, 50)]
		[InlineData(0, 100, 100)]
		[InlineData(0, 100, 10000)]
		[InlineData(0, 100, -10000)]
		[InlineData(0, 10000, 10000)]
		[InlineData(0, 10000, -10000)]
		public async Task SmallChangeInitializesCorrectly(double min, double max, double value)
		{
			var slider = new SliderStub()
			{
				Maximum = max,
				Minimum = min,
				Value = value
			};

			var expected = await GetValueAsync(slider, GetSmallChange);

			Assert.True(expected != 0);
		}

		[Fact]
		public async Task ThumbImageSourceUpdatesCorrectly()
		{
			var slider = new SliderStub
			{
				Maximum = 10,
				Minimum = 0,
				Value = 5,
				ThumbImageSource = new FileImageSourceStub("black.png"),
			};

			// Update the Slider ThumbImageSource
			slider.ThumbImageSource = new FileImageSourceStub("red.png");

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(slider);

				await AssertEventually(() => ImageSourceLoaded(handler));

				await Task.Delay(100);

				var expectedColor = Color.FromArgb("#FF0000");
				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		[Theory]
		[InlineData("red.png", 100, 100)]
		[InlineData("black.png", 100, 100)]
		public async Task ThumbImageSourceSizeIsCorrect(string filename, double thumbHeight, double thumbWidth)
		{
			var slider = new SliderStub
			{
				Maximum = 10,
				Minimum = 0,
				Value = 5,
				ThumbImageSource = new FileImageSourceStub("black.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(slider);

				await AssertEventually(() => ImageSourceLoaded(handler));

				await handler.PlatformView.AttachAndRun(async () =>
				{
					// Update the Slider ThumbImageSource
					slider.ThumbImageSource = new FileImageSourceStub(filename);

					await Task.Delay(100);

					var nativeThumbSize = GetNativeThumbSize(handler);
					Assert.Equal(nativeThumbSize.Height, thumbHeight);
					Assert.Equal(nativeThumbSize.Width, thumbWidth);
				}, MauiContext);
			});
		}

		UI.Xaml.Controls.Slider GetNativeSlider(SliderHandler sliderHandler) =>
			sliderHandler.PlatformView;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Value;

		double GetNativeMinimum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Minimum;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Maximum;

		double GetSmallChange(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).SmallChange;

		Size GetNativeThumbSize(SliderHandler sliderHandler)
		{
			var nativeSlider = GetNativeSlider(sliderHandler);

			if (nativeSlider.GetFirstDescendant<Thumb>() is Thumb thumb)
			{
				return new Size(thumb.Width, thumb.Height);
			}

			return Size.Zero;
		}

		bool ImageSourceLoaded(SliderHandler sliderHandler)
		{
			return (sliderHandler.PlatformView as MauiSlider)?.ThumbImageSource != null;
		}

		async Task ValidateNativeThumbColor(ISlider slider, Color color)
		{
			var expected = await GetValueAsync(slider, handler =>
			{
				var nativeSlider = GetNativeSlider(handler);

				if (nativeSlider.GetFirstDescendant<Thumb>() is Thumb thumb)
				{
					if (thumb.Background is UI.Xaml.Media.SolidColorBrush solidThumb)
					{
						return solidThumb.Color.ToColor();
					}
				}

				return null;
			});

			Assert.Equal(expected, color);
		}
	}
}