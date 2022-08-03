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
	using Grid = Microsoft.Maui.Controls.Compatibility.Grid;
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	public class LayoutCompatTests : BaseTestFixture
	{
		[Test]
		public void BasicContentPage()
		{
			var page = new ContentPage() { IsPlatformEnabled = true };
			var button = new Button() { IsPlatformEnabled = true };
			var expectedSize = new Size(100, 100);
			var expectedRect = new Rect(Point.Zero, expectedSize);

			var buttonHandler = Substitute.For<IViewHandler>();
			buttonHandler.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			button.Handler = buttonHandler;

			page.Content = button;
			(page as IContentView).CrossPlatformMeasure(expectedSize.Width, expectedSize.Height);
			(page as IContentView).CrossPlatformArrange(expectedRect);

			buttonHandler.Received().PlatformArrange(expectedRect);
			Assert.AreEqual(expectedSize, button.Bounds.Size);
		}

		[Test]
		public void GridInsideStackLayout()
		{
			ContentPage contentPage = new ContentPage();
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			var grid = new Grid() { IsPlatformEnabled = true, HeightRequest = 50 };
			var label = new Label() { IsPlatformEnabled = true };
			var expectedSize = new Size(100, 50);

			var view = Substitute.For<IViewHandler>();
			view.GetDesiredSize(default, default).ReturnsForAnyArgs(expectedSize);
			label.Handler = view;

			stackLayout.Children.Add(grid);
			grid.Children.Add(label);
			contentPage.Content = stackLayout;

			var rect = new Rect(Point.Zero, expectedSize);

			(contentPage as Microsoft.Maui.IContentView).CrossPlatformMeasure(expectedSize.Width, expectedSize.Height);
			(contentPage as Microsoft.Maui.IContentView).CrossPlatformArrange(rect);

			Assert.AreEqual(expectedSize, grid.Bounds.Size);
		}
	}
}
