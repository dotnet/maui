using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		[Theory(DisplayName = "Border render without Stroke")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BorderRenderWithoutStroke(uint color)
		{
			var expected = Color.FromUint(color);

			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Without Stroke", TextColor = Colors.White },
				Shape = new RoundRectangleShapeStub { CornerRadius = new CornerRadius(12) },
				Background = new SolidPaintStub(expected),
				Stroke = null,
				StrokeThickness = 2,
				Height = 100,
				Width = 300
			};

			await ValidateHasColor(border, expected);
		}

		[Theory(DisplayName = "Background Updates Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundUpdatesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Background", TextColor = Colors.White },
				Shape = new RoundRectangleShapeStub { CornerRadius = new CornerRadius(12) },
				Background = new SolidPaintStub(Color.FromUint(0xFF888888)),
				Stroke = null,
				StrokeThickness = 2,
				Height = 100,
				Width = 300
			};

			await ValidateHasColor(border, expected, () => border.Background = new SolidPaintStub(expected), nameof(border.Background));
		}

		[Fact(DisplayName = "Border isn't drawn into a hardware layer")]
		public async Task BorderIsNotDrawnIntoAHardwareLayer()
		{
			// https://github.com/dotnet/maui/issues/40186
			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Layer" },
				Shape = new RectangleShapeStub(),
				Stroke = new SolidPaintStub(Colors.Black),
				StrokeThickness = 2,
				Height = 100,
				Width = 300
			};

			var layerType = await GetValueAsync(border, handler => GetNativeBorder(handler).LayerType);

			// In a hardware layer, a SurfaceView (e.g. a video) isn't visible on Android 10 and 11, and the stroke isn't drawn around it on Android 12
			Assert.Equal(global::Android.Views.LayerType.None, layerType);
		}

		ContentViewGroup GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;
	}
}