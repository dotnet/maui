using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewHandler) =>
			boxViewHandler.PlatformView;

		Task<float> GetPlatformOpacity(ShapeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeBoxView(handler);
				return (float)nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(ShapeViewHandler boxViewHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeBoxView(boxViewHandler);
				return !nativeView.Hidden;
			});
		}

		[Theory]
		[InlineData(-1, true, true)]
		[InlineData(0, true, true)]
		[InlineData(1, true, true)]
		[InlineData(-1, false, true)]
		[InlineData(0, false, true)]
		[InlineData(1, false, true)]
		[InlineData(-1, true, false)]
		[InlineData(0, true, false)]
		[InlineData(1, true, false)]
		[InlineData(-1, false, false)]
		[InlineData(0, false, false)]
		[InlineData(1, false, false)]
		public async Task ChangingRequestedSizeRedrawsBoxView(double initialSize, bool changeWidth, bool useBackground)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<BoxView, RecordingBoxViewHandler>();
				handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
			}));

			var boxView = new BoxView
			{
				WidthRequest = changeWidth ? initialSize : 100,
				HeightRequest = changeWidth ? 5 : initialSize,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start
			};

			if (useBackground)
				boxView.Background = Brush.Red;
			else
				boxView.Color = Colors.Red;

			var layout = new VerticalStackLayout { Children = { boxView } };

			await AttachAndRun<LayoutHandler>(layout, async _ =>
			{
				var handler = (ShapeViewHandler)boxView.Handler;
				var platformView = (RecordingShapeView)GetNativeBoxView(handler);
				await AssertHelpers.AssertEventually(() =>
					(changeWidth ? boxView.Width : boxView.Height) == Math.Max(0, initialSize));

				if (changeWidth)
					boxView.WidthRequest = 100;
				else
					boxView.HeightRequest = 100;

				// Observe UIKit's draw pass before taking a snapshot, which can itself force drawing.
				await AssertHelpers.AssertEventually(() =>
					platformView.LastDrawBounds is CGRect bounds &&
					bounds.Width == 100 && bounds.Height == (changeWidth ? 5 : 100),
					message: "BoxView did not redraw at its updated size.");

				Assert.Equal(100, changeWidth ? boxView.Width : boxView.Height);
				await platformView.AssertContainsColor(Colors.Red, MauiContext);
			});
		}

		sealed class RecordingBoxViewHandler : BoxViewHandler
		{
			protected override MauiShapeView CreatePlatformView() => new RecordingShapeView();
		}

		sealed class RecordingShapeView : MauiShapeView
		{
			public CGRect? LastDrawBounds { get; private set; }

			public override void Draw(CGRect dirtyRect)
			{
				LastDrawBounds = dirtyRect;
				base.Draw(dirtyRect);
			}
		}

		[Fact(DisplayName = "ShapeView Parts Keep Around")]
		public async Task ShapeViewPartsKeepAround()
		{
			var boxView = new BoxView()
			{
				HeightRequest = 100,
				WidthRequest = 200
			};

			await AttachAndRun<ShapeViewHandler>(boxView, async handler =>
			{
				var shapeView = GetNativeBoxView(handler);
				var renderer = (DirectRenderer)shapeView.Renderer;

				GC.Collect();
				GC.WaitForPendingFinalizers();
				await Task.Yield();

				GC.Collect();
				GC.WaitForPendingFinalizers();
				await Task.Yield();

				Assert.NotNull(shapeView.Renderer);
				Assert.NotNull(shapeView.Drawable);
				Assert.NotNull(renderer.Drawable);

				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var graphicsView = renderer.GetType().GetField("_graphicsView", flags)?.GetValue(renderer) as PlatformGraphicsView;
				Assert.NotNull(graphicsView);
			});
		}
	}
}