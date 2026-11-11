using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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

				await handler.PlatformView.AttachAndRun(async () =>
				{
					await Task.Delay(100);

					var expectedColor = Color.FromArgb("#FF0000");
					await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
				}, MauiContext);
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
					handler.UpdateValue(nameof(ISlider.ThumbImageSource));

					await Task.Delay(100);

					var nativeThumbSize = GetNativeThumbSize(handler);
					Assert.Equal(nativeThumbSize.Height, thumbHeight);
					Assert.Equal(nativeThumbSize.Width, thumbWidth);
				}, MauiContext);
			});
		}

		[Theory]
		[InlineData(null)]
		[InlineData("black.png")]
		public async Task ChangingThumbImageSourceDoesNotApplyPendingFont(string filename)
		{
			var slider = new SliderStub();

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(slider);
				var nativeSlider = Assert.IsType<MauiSlider>(handler.PlatformView);
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

				await nativeSlider.AttachAndRun(async () =>
				{
					slider.ThumbImageSource = new FontImageSourceStub
					{
						Font = Font.OfSize("Dokdo", 24),
						Glyph = "X",
						Color = Colors.Red
					};
					var pending = nativeSlider.UpdateThumbImageSourceAsync(slider, provider, null);
					Assert.False(pending.IsCompleted);

					slider.ThumbImageSource = filename == null ? null : new FileImageSourceStub(filename);
					await nativeSlider.UpdateThumbImageSourceAsync(slider, provider, null);
					var currentImage = nativeSlider.ThumbImageSource;
					if (filename == null)
						Assert.Null(currentImage);
					else
						Assert.NotNull(currentImage);

					await pending;
					Assert.Same(currentImage, nativeSlider.ThumbImageSource);
				}, MauiContext);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RebindingOrDisconnectingDoesNotApplyPendingThumbFont(bool disconnect)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var slider = new SliderStub();
				var handler = CreateHandler(slider);
				var nativeSlider = Assert.IsType<MauiSlider>(handler.PlatformView);
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				slider.ThumbImageSource = new FontImageSourceStub
				{
					Font = Font.OfSize("Dokdo", 24),
					Glyph = "X",
					Color = Colors.Red
				};
				var pending = nativeSlider.UpdateThumbImageSourceAsync(slider, provider, null);
				Assert.False(pending.IsCompleted);
				if (disconnect)
					((IElementHandler)handler).DisconnectHandler();
				else
					handler.SetVirtualView(new SliderStub());
				var current = nativeSlider.ThumbImageSource;
				await pending;
				Assert.Same(current, nativeSlider.ThumbImageSource);
			});
		}

		[Theory]
		[InlineData("current")]
		[InlineData("replace")]
		[InlineData("reload")]
		[InlineData("clear")]
		[InlineData("rebind")]
		[InlineData("disconnect")]
		public async Task LateBitmapImageOpenedOnlyResizesCurrentThumb(string update)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var slider = new SliderStub();
				var handler = CreateHandler(slider);
				var nativeSlider = Assert.IsType<MauiSlider>(handler.PlatformView);
				await nativeSlider.AttachAndRun(async () =>
				{
					await AssertEventually(() => nativeSlider.GetFirstDescendant<Thumb>() != null);
					var thumb = nativeSlider.GetFirstDescendant<Thumb>();
					var bitmap = new BitmapImage();
					var result = new ImageSourceServiceResult(bitmap);
					var provider = new ControlledImageSourceService
					{
						Result = Task.FromResult<IImageSourceServiceResult<ImageSource>>(result)
					};
					slider.ThumbImageSource = new FileImageSourceStub("red.png");
					await nativeSlider.UpdateThumbImageSourceAsync(slider, provider, new Size(20, 20));
					Assert.Equal(0, bitmap.PixelWidth);

					switch (update)
					{
						case "replace":
						case "reload":
							if (update == "replace")
								slider.ThumbImageSource = new FileImageSourceStub("black.png");
							provider.Result = Task.FromResult<IImageSourceServiceResult<ImageSource>>(
								new ImageSourceServiceResult(new WriteableBitmap(20, 20)));
							await nativeSlider.UpdateThumbImageSourceAsync(slider, provider, new Size(20, 20));
							break;
						case "clear":
							slider.ThumbImageSource = null;
							await nativeSlider.UpdateThumbImageSourceAsync(slider, provider, new Size(20, 20));
							break;
						case "rebind":
							handler.SetVirtualView(new SliderStub());
							break;
						case "disconnect":
							((IElementHandler)handler).DisconnectHandler();
							break;
					}

					thumb.Width = 20;
					thumb.Height = 20;
					var current = nativeSlider.ThumbImageSource;
					var opened = new TaskCompletionSource();
					void OnOpened(object sender, RoutedEventArgs args)
					{
						bitmap.ImageOpened -= OnOpened;
						opened.SetResult();
					}
					bitmap.ImageOpened += OnOpened;
					// Decode only after the replacement/clear, and observe the real WinUI event.
					using var stream = BaseImageSourceServiceTests.CreateBitmapStream(37, 43, Colors.Red);
					await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
					await opened.Task.WaitAsync(TimeSpan.FromSeconds(10));
					Assert.Equal(37, bitmap.PixelWidth);
					Assert.Equal(43, bitmap.PixelHeight);
					Assert.Equal(update == "current" ? 37 : 20, thumb.Width);
					Assert.Equal(update == "current" ? 43 : 20, thumb.Height);
					Assert.Same(current, nativeSlider.ThumbImageSource);
					Assert.Equal(update != "current", result.IsDisposed);
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