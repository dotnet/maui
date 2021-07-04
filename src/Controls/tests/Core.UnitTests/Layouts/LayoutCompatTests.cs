using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	public class LayoutCompatTests : BaseTestFixture
	{
		[Test]
		public void BasicContentPage()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };
			var button = new Button() { IsPlatformEnabled = true };
			var expectedSize = new Size(100, 100);
			var expectedRect = new Rectangle(Point.Zero, expectedSize);

			var buttonHandler = Substitute.For<IViewHandler>();
			buttonHandler.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = buttonHandler;

			page.Content = button;
			(page as IFrameworkElement).Measure(expectedSize.Width, expectedSize.Height);
			(page as IFrameworkElement).Arrange(expectedRect);

			buttonHandler.Received().NativeArrange(expectedRect);
			Assert.AreEqual(expectedSize, button.Bounds.Size);
		}

		[Test]
		public void VerticalStackLayoutInsideStackLayout()
		{
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			var verticalStackLayout = new VerticalStackLayout() { IsPlatformEnabled = true };
			var button = new Button() { IsPlatformEnabled = true, HeightRequest = 100, WidthRequest = 100 };
			var expectedSize = new Size(100, 100);

			var buttonHandler = Substitute.For<IViewHandler>();
			buttonHandler.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = buttonHandler;

			stackLayout.Children.Add(verticalStackLayout);
			verticalStackLayout.Add(button);

			var rect = new Rectangle(0, 0, 100, 100);
			Layout.LayoutChildIntoBoundingRegion(stackLayout, rect);

			// Normally this would get called from the native platform
			(verticalStackLayout as IFrameworkElement).Arrange(rect);

			Assert.AreEqual(expectedSize, button.Bounds.Size);
		}

		[Test]
		public void StackLayoutInsideVerticalStackLayout()
		{
			var expectedSize = new Size(100, 100);
			var expectedRect = new Rectangle(Point.Zero, expectedSize);

			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			var slHandler = Substitute.For<ILayoutHandler>();
			stackLayout.Handler = slHandler;

			var verticalStackLayout = new VerticalStackLayout() { IsPlatformEnabled = true };
			var vslHandler = Substitute.For<ILayoutHandler>();
			verticalStackLayout.Handler = vslHandler;

			var button = new Button() { IsPlatformEnabled = true, HeightRequest = 100, WidthRequest = 100 };
			var buttonHandler = Substitute.For<IViewHandler>();
			buttonHandler.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = buttonHandler;

			verticalStackLayout.Add(stackLayout);
			stackLayout.Children.Add(button);

			(verticalStackLayout as IFrameworkElement).Measure(expectedRect.Width, expectedRect.Height);
			(verticalStackLayout as IFrameworkElement).Arrange(expectedRect);

			slHandler.Received().NativeArrange(expectedRect);
			Assert.AreEqual(expectedSize, stackLayout.Bounds.Size);
		}

		[Test]
		public void GridInsideStackLayout()
		{
			ContentPage contentPage = new ContentPage();
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			var grid = new Grid() { IsPlatformEnabled = true, HeightRequest = 50 };
			var label = new Label() { IsPlatformEnabled = true };
			var expectedSize = new Size(50, 50);

			var view = Substitute.For<IViewHandler>();
			view.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			label.Handler = view;

			stackLayout.Add(grid);
			grid.Children.Add(label);
			contentPage.Content = stackLayout;

			var rect = new Rectangle(0, 0, 50, 100);
			(contentPage as IFrameworkElement).Measure(expectedSize.Width, expectedSize.Height);
			(contentPage as IFrameworkElement).Arrange(rect);

			// This simulates the arrange call that happens from the native LayoutViewGroup
			(grid as IFrameworkElement).Arrange(rect);

			Assert.AreEqual(expectedSize, grid.Bounds.Size);
		}
	}
}
