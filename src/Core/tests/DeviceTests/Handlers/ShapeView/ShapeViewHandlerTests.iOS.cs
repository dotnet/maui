using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShapeViewHandlerTests
	{
		[Fact(DisplayName = "Shadow Initializes Correctly on Shapes")]
		public async Task ShadowInitializesCorrectly()
		{
			var xPlatShadow = new ShadowStub
			{
				Offset = new Point(10, 10),
				Opacity = 1.0f,
				Radius = 2.0f
			};

			var rectangle = new RectangleStub
			{
				Height = 50,
				Width = 50
			};

			rectangle.Shadow = xPlatShadow;

			await ValidateHasColor(rectangle, Colors.Red, () => xPlatShadow.Paint = new SolidPaint(Colors.Red));
		}

		MauiShapeView GetPlatformShapeView(ShapeViewHandler shapeViewHandler) =>
			shapeViewHandler.PlatformView;

		Task ValidateNativeFill(IShapeView shapeView, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return GetPlatformShapeView(CreateHandler(shapeView)).AssertContainsColorAsync(color);
			});
		}
	}
}