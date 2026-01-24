using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.VisualBasic;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		[Fact, Category(TestCategory.Layout)]
		public async Task LayoutViewHasExpectedSize()
		{
			var layout = new VerticalStackLayout();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			layout.Add(label);

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task LayoutViewRespondsWhenViewAdded()
		{
			var layout = new VerticalStackLayout();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			var secondLabel = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			layout.Add(label);

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler2 = CreateHandler<LabelHandler>(secondLabel);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				layout.Add(secondLabel);

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(200, updatedSize.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task LayoutViewRespondsWhenViewRemoved()
		{
			var layout = new VerticalStackLayout();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			var secondLabel = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			layout.Add(label);
			layout.Add(secondLabel);

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var labelHandler2 = CreateHandler<LabelHandler>(secondLabel);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(200, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				layout.Remove(secondLabel);

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(100, updatedSize.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task LayoutViewRespondsWhenViewUpdated()
		{
			var layout = new VerticalStackLayout();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			layout.Add(label);

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				label.HeightRequest = 300;

				var targetSize = layoutHandler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				layoutHandler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return layoutHandler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(300, updatedSize.Height);
		}

		void ValidateInputTransparentOnPlatformView(IView view)
		{
			var platformView = view.ToPlatform(MauiContext);

			bool value = platformView.UserInteractionEnabled;
			if (platformView is IInputTransparentManagingView lv)
				value = !lv.InputTransparent;

			Assert.True(view.InputTransparent == !value,
				$"InputTransparent: {view.InputTransparent}. UserInteractionEnabled: {platformView.UserInteractionEnabled}");
		}

		async Task WaitForLayout(UIView view, CGRect initialFrame, int timeout)
		{
			int interval = 100;

			while (timeout > 0)
			{
				if (view.Frame != initialFrame)
				{
					return;
				}

				await Task.Delay(interval);
				timeout -= interval;
			}
		}

		[Fact]
		public async Task NestedLayoutsInRTLRemainOnScreen()
		{
			EnsureHandlerCreated((builder) =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler(typeof(Button), typeof(ButtonHandler));
					handler.AddHandler(typeof(Layout), typeof(LayoutHandler));
					handler.AddHandler(typeof(Controls.ContentView), typeof(ContentViewHandler));
				});
			});

			var level0 = new Controls.ContentView() { FlowDirection = FlowDirection.RightToLeft };
			var level1 = new Grid() { HorizontalOptions = LayoutOptions.End, WidthRequest = 200 };
			var level2 = new Grid() { HorizontalOptions = LayoutOptions.Start, WidthRequest = 200 };
			var button = new Button() { Text = "Hello", HorizontalOptions = LayoutOptions.Start, WidthRequest = 100 };

			level0.Content = level1;
			level1.Add(level2);
			level2.Add(button);

			await AttachAndRun(level0, async (handler) =>
			{
				var root = handler.ToPlatform();

				await WaitForLayout(root, CGRect.Empty, 5000);

				UIKit.UIView nativeView = button.Handler.PlatformView as UIKit.UIView;
				Assert.NotNull(nativeView);

				// Work our way up the tree and verify that no one
				// has a negative X position (which would make the view not 
				// show up on screen)
				while (nativeView != null)
				{
					Assert.True(nativeView.Bounds.X >= 0);
					nativeView = nativeView.Superview;
				}
			});
		}
	}
}
