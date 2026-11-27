using System;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		ContentView GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task GradientWithUnsetColorsRendersAsTransparent(bool radial, bool stroke)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var bounds = new CGRect(0, 0, 20, 20);
				var shape = new RectangleShapeStub();
				using var layer = new MauiCALayer { Bounds = bounds };
				layer.SetBorderShape(shape);
				layer.LayoutSublayers();

				GradientPaint paint = radial
					? new RadialGradientPaint()
					: new LinearGradientPaint();
				paint.GradientStops = new[]
				{
					new PaintGradientStop(0, Colors.Red),
					new PaintGradientStop(0.5f, null),
					new PaintGradientStop(1, Colors.Blue)
				};

				if (stroke)
				{
					layer.SetBorderWidth(4);
					layer.SetBorderBrush(paint);
				}
				else
				{
					layer.SetBackground(paint);
				}

				using var format = new UIGraphicsImageRendererFormat { Opaque = false, Scale = 1 };
				using var renderer = new UIGraphicsImageRenderer(bounds.Size, format);
				using var unsetImage = renderer.CreateImage(context => layer.DrawInContext(context.CGContext));

				paint.GradientStops[1].Color = Colors.Transparent;
				using var transparentImage = renderer.CreateImage(context => layer.DrawInContext(context.CGContext));
				await unsetImage.AssertEqualAsync(transparentImage);

				foreach (var stop in paint.GradientStops)
				{
					stop.Color = null;
				}

				using var allUnsetImage = renderer.CreateImage(context => layer.DrawInContext(context.CGContext));

				foreach (var stop in paint.GradientStops)
					stop.Color = Colors.Transparent;

				using var allTransparentImage = renderer.CreateImage(context => layer.DrawInContext(context.CGContext));
				await allUnsetImage.AssertEqualAsync(allTransparentImage);
			});
		}

		[Fact(DisplayName = "Presentation Layer Copy Draws Like The Border Layer")]
		public async Task PresentationLayerCopyDrawsLikeTheBorderLayer()
		{
			var border = new BorderStub()
			{
				Content = new LabelStub { Text = "Presentation", TextColor = Colors.Black },
				Shape = new RectangleShapeStub(),
				Background = new SolidPaintStub(Colors.Red),
				Stroke = new SolidPaintStub(Colors.Blue),
				StrokeThickness = 2,
				Height = 100,
				Width = 300
			};

			await AttachAndRun<BorderHandler>(border, handler =>
			{
				var mauiCALayer = GetNativeBorder(handler).Layer.Sublayers.OfType<MauiCALayer>().Single();
				CATransaction.Flush();

				// Core Animation creates the presentation layer by calling initWithLayer: on a new native
				// instance. Screen recorders (e.g. Microsoft Clarity) draw that copy, so it needs a managed
				// peer that carries the same paint and shape as the layer it was copied from.
				var presentationLayer = Assert.IsType<MauiCALayer>(mauiCALayer.PresentationLayer);
				var format = new UIGraphicsImageRendererFormat { Scale = 1 };
				var bitmap = new UIGraphicsImageRenderer(presentationLayer.Bounds.Size, format)
					.CreateImage(context => presentationLayer.DrawInContext(context.CGContext));

				bitmap.AssertColorAtCenter(Colors.Red.ToPlatform());
			});
		}
	}
}
