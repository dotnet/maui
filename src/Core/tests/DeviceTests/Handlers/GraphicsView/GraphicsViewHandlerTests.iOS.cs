using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		PlatformGraphicsView GetPlatformGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
			graphicsViewHandler.PlatformView;

		[Theory]
		[InlineData(FlowDirection.LeftToRight, 12, 8)]
		[InlineData(FlowDirection.RightToLeft, 12, 8)]
		[InlineData(FlowDirection.LeftToRight, -12, -8)]
		[InlineData(FlowDirection.RightToLeft, -12, -8)]
		public async Task ShadowsRespectFlowDirectionChanges(FlowDirection initialDirection, int offsetX, int offsetY)
		{
			var graphicsView = new GraphicsViewStub
			{
				Drawable = new ShadowDrawable(new SizeF(offsetX, offsetY)),
				FlowDirection = initialDirection,
				Width = 100,
				Height = 100
			};

			var handler = await CreateHandlerAsync(graphicsView);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Frame = new CGRect(0, 0, 100, 100);

				using var format = new UIGraphicsImageRendererFormat { Scale = 1 };
				using var renderer = new UIGraphicsImageRenderer(platformView.Bounds.Size, format);

				AssertShadow(initialDirection);

				var oppositeDirection = initialDirection == FlowDirection.LeftToRight
					? FlowDirection.RightToLeft
					: FlowDirection.LeftToRight;

				graphicsView.FlowDirection = oppositeDirection;
				handler.UpdateValue(nameof(IGraphicsView.FlowDirection));
				AssertShadow(oppositeDirection);

				graphicsView.FlowDirection = initialDirection;
				handler.UpdateValue(nameof(IGraphicsView.FlowDirection));
				AssertShadow(initialDirection);

				void AssertShadow(FlowDirection direction)
				{
					using var image = renderer.CreateImage(context =>
					{
						context.CGContext.SetFillColor(UIColor.White.CGColor);
						context.CGContext.FillRect(platformView.Bounds);
						platformView.Draw(platformView.Bounds);
					});

					int shadowX = 50 + (direction == FlowDirection.RightToLeft ? -offsetX : offsetX);
					int shadowY = offsetY > 0 ? 64 : 36;

					image.AssertColorAtPoint(UIColor.Black, shadowX, shadowY);
					image.AssertColorAtPoint(UIColor.White, 100 - shadowX, shadowY);
					image.AssertColorAtPoint(UIColor.White, shadowX, 100 - shadowY);
					image.AssertColorAtPoint(Colors.CornflowerBlue.ToPlatform(), 50, 50);
				}
			});
		}

		sealed class ShadowDrawable : IDrawable
		{
			readonly SizeF _offset;

			public ShadowDrawable(SizeF offset)
			{
				_offset = offset;
			}

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.SaveState();
				canvas.SetShadow(_offset, 0, Colors.Black);
				canvas.FillColor = Colors.CornflowerBlue;
				canvas.FillRectangle(40, 40, 20, 20);
				canvas.RestoreState();
			}
		}
	}
}