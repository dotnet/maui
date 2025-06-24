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

		ContentViewGroup GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

		[Fact(DisplayName = "BorderHandler sets appropriate layer type")]
		public async Task BorderHandlerSetsLayerType()
		{
			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Test" },
				Height = 100,
				Width = 300
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(border);
				var platformView = handler.PlatformView as ContentViewGroup;

				Assert.NotNull(platformView);

				// Verify that layer type is set appropriately based on hardware acceleration
				// The exact value depends on system configuration, but it should be either
				// Hardware (when acceleration enabled) or Software (when disabled)
				var layerType = platformView.LayerType;
				Assert.True(layerType == Android.Views.LayerType.Hardware || 
				           layerType == Android.Views.LayerType.Software,
				           $"Expected Hardware or Software layer type, but got {layerType}");
			});
		}
	}
}