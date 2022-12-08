using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.GraphicsView)]
	public partial class GraphicsViewHandlerTests : CoreHandlerTestBase<GraphicsViewHandler, GraphicsViewStub>
	{
		[Theory(DisplayName = "GraphicsView Initializes Correctly")]
		[InlineData(0xFF0000)]
		[InlineData(0x00FF00)]
		[InlineData(0x0000FF)]
		public async Task GraphicsViewInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var graphicsView = new GraphicsViewStub()
			{
				Drawable = new TestDrawable(expected),
				Height = 100,
				Width = 200
			};

			await ValidateHasColor(graphicsView, expected);
		}
	}

	public class TestDrawable : IDrawable
	{
		public TestDrawable(Color fillColor)
		{
			FillColor = fillColor;
		}

		public Color FillColor { get; set; }

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.FillColor = FillColor;
			canvas.FillRectangle(dirtyRect);
		}
	}
}