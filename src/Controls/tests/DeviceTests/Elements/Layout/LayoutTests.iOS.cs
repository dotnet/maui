using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
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
	}
}
