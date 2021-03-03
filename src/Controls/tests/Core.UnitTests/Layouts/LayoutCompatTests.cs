using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			var layout = new VerticalStackLayout() { IsPlatformEnabled = true };
			var button = new Button() { IsPlatformEnabled = true };
			var expectedSize = new Size(100, 100);

			var view = Substitute.For<IViewHandler>();
			view.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = view;

			layout.Add(button);
			page.Content = layout;
			(page as IFrameworkElement).Arrange(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(expectedSize, button.Bounds.Size);
		}

		[Test]
		public void VerticalStackLayoutInsideStackLayout()
		{
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			var verticalStackLayout = new VerticalStackLayout() { IsPlatformEnabled = true };
			var button = new Button() { IsPlatformEnabled = true, HeightRequest = 100, WidthRequest = 100 };
			var expectedSize = new Size(100, 100);

			var view = Substitute.For<IViewHandler>();
			view.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = view;

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
			ContentPage contentPage = new ContentPage();
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			var verticalStackLayout = new VerticalStackLayout() { IsPlatformEnabled = true };
			var button = new Button() { IsPlatformEnabled = true, HeightRequest = 100, WidthRequest = 100 };
			var expectedSize = new Size(100, 100);

			var view = Substitute.For<IViewHandler>();
			view.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = view;

			verticalStackLayout.Add(stackLayout);
			stackLayout.Children.Add(button);
			contentPage.Content = verticalStackLayout;

			var rect = new Rectangle(0, 0, 100, 100);
			(contentPage as IFrameworkElement).Measure(expectedSize.Width, expectedSize.Height);
			(contentPage as IFrameworkElement).Arrange(rect);
			Assert.AreEqual(expectedSize, button.Bounds.Size);
		}
	}
}
