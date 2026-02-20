using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Subviews.Length;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Subviews[0].Subviews.Length;
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewHasExpectedSize()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			contentView.Content = label;

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewRespondsWhenViewAdded()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(0, size.Width);
			Assert.Equal(0, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ContentViewHandler>(contentView);

				contentView.Content = label;

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(100, updatedSize.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewRespondsWhenViewRemoved()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			contentView.Content = label;

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ContentViewHandler>(contentView);

				contentView.Content = null;

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(0, updatedSize.Width);
			Assert.Equal(0, updatedSize.Height);
		}

		[Fact, Category(TestCategory.Layout)]
		public async Task ContentViewRespondsWhenViewUpdated()
		{
			var contentView = new ContentView();
			var label = new Label { Text = "", HeightRequest = 100, WidthRequest = 100 };
			contentView.Content = label;

			var size = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var handler = CreateHandler<ContentViewHandler>(contentView);

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);

			var updatedSize = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ContentViewHandler>(contentView);

				label.HeightRequest = 300;

				var targetSize = handler.VirtualView.Measure(double.PositiveInfinity, double.PositiveInfinity);
				handler.VirtualView.Arrange(new Rect(Point.Zero, targetSize));

				return handler.PlatformView.Bounds.Size.ToSize();
			});

			Assert.Equal(100, updatedSize.Width);
			Assert.Equal(300, updatedSize.Height);
		}
	}
}
