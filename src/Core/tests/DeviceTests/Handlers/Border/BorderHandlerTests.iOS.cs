using System;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
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
