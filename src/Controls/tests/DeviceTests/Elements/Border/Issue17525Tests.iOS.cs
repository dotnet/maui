using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Border)]
	public class Issue17525 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task PolygonContentMaskUsesInsetInnerPathAfterResize()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});

			var border = new Border
			{
				WidthRequest = 280,
				HeightRequest = 220,
				StrokeThickness = 20,
				StrokeShape = new Polygon
				{
					Points = new PointCollection
					{
						new Point(0, 0),
						new Point(100, 0),
						new Point(50, 100)
					}
				},
				Content = new Grid()
			};

			await AttachAndRun<BorderHandler>(border, handler =>
			{
				const double resizedWidth = 140;
				const double resizedHeight = 220;

				border.WidthRequest = resizedWidth;
				border.Arrange(new Rect(0, 0, resizedWidth, resizedHeight));
				handler.PlatformView.Frame = new CGRect(0, 0, resizedWidth, resizedHeight);
				handler.PlatformView.SetNeedsLayout();
				handler.PlatformView.LayoutIfNeeded();

				var contentMask = Assert.IsAssignableFrom<CAShapeLayer>(handler.PlatformView.Subviews[0].Layer.Mask);
				var maskGeometry = contentMask.Path;
				Assert.NotNull(maskGeometry);

				var pointBetweenOuterAndInnerEdges = new CGPoint(25, 25);
				Assert.False(
					maskGeometry.ContainsPoint(pointBetweenOuterAndInnerEdges, false),
					"Polygon border content should be clipped to inset inner geometry.");
			});
		}
	}
}
