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
		[InlineData(-1, true)]
		[InlineData(0, true)]
		[InlineData(1, true)]
		[InlineData(-1, false)]
		[InlineData(0, false)]
		[InlineData(1, false)]
		public async Task ChangingWidthRequestRedrawsBoxView(double initialWidth, bool useBackground)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<BoxView, RecordingBoxViewHandler>();
				handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
			}));

			var boxView = new BoxView
			{
				HeightRequest = 5,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start
			};

			if (initialWidth >= 0)
				boxView.WidthRequest = initialWidth;

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
					boxView.Width == Math.Max(0, initialWidth),
					message: $"BoxView did not reach initial width {initialWidth} (background={useBackground}).");

				foreach (var width in new[] { 100, 200, 100 })
				{
					var previousDrawCount = platformView.DrawCount;
					boxView.WidthRequest = width;

					await AssertHelpers.AssertEventually(() =>
						boxView.Width == width && platformView.Bounds.Width == width,
						message: $"BoxView did not reach width {width} (initial={initialWidth}, background={useBackground}).");

					// Observe UIKit's draw pass before taking a snapshot, which can itself force drawing.
					await AssertHelpers.AssertEventually(() =>
						platformView.DrawCount > previousDrawCount &&
						platformView.LastDrawBounds is CGRect bounds &&
						bounds.Width == width && bounds.Height == 5,
						message: $"BoxView did not redraw at width {width} (initial={initialWidth}, background={useBackground}).");

					await platformView.AssertContainsColor(Colors.Red, MauiContext);
				}
			});
		}

		sealed class RecordingBoxViewHandler : BoxViewHandler
		{
			protected override MauiShapeView CreatePlatformView() => new RecordingShapeView();
		}

		sealed class RecordingShapeView : MauiShapeView
		{
			public CGRect? LastDrawBounds { get; private set; }
			public int DrawCount { get; private set; }

			public override void Draw(CGRect dirtyRect)
			{
				base.Draw(dirtyRect);
				LastDrawBounds = Bounds;
				DrawCount++;
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