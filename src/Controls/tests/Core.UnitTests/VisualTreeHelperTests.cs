using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class VisualTreeHelperTests : BaseTestFixture
	{
		[Test]
		public void VerticalStackLayoutChildren()
		{
			var verticalStackLayout = new VerticalStackLayout() { IsPlatformEnabled = true };
			verticalStackLayout.Children.Add(new Label());
			verticalStackLayout.Children.Add(new Button());
			Assert.AreEqual(verticalStackLayout.Children.Count, (verticalStackLayout as IVisualTreeElement).GetVisualChildren().Count);
		}

		[Test]
		public void HorizontalStackLayoutChildren()
		{
			var horizontalStackLayout = new HorizontalStackLayout() { IsPlatformEnabled = true };
			horizontalStackLayout.Children.Add(new Label());
			horizontalStackLayout.Children.Add(new Button());
			Assert.AreEqual(horizontalStackLayout.Children.Count, (horizontalStackLayout as IVisualTreeElement).GetVisualChildren().Count);
		}

		[Test]
		public void StackLayoutChildren()
		{
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			stackLayout.Children.Add(new Label());
			stackLayout.Children.Add(new Button());
			Assert.AreEqual(stackLayout.Children.Count, (stackLayout as IVisualTreeElement).GetVisualChildren().Count);
		}

		[Test]
		public void ApplicationChildren()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();

			app.MainPage = page;

			var window = iapp.CreateWindow(null);

			var appChildren = (app as IVisualTreeElement).GetVisualChildren();
			var windowChildren = (window as IVisualTreeElement).GetVisualChildren();
			Assert.Greater(appChildren.Count, 0);
			Assert.IsTrue(appChildren[0] is IWindow);
			Assert.AreEqual(windowChildren.Count, 1);
		}

		[Test]
		public void VisualElementParent()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();
			app.MainPage = page;
			var window = iapp.CreateWindow(null);
			var appParent = (app as IVisualTreeElement).GetVisualParent();
			var windowParent = (window as IVisualTreeElement).GetVisualParent();
			Assert.IsNull(appParent);
			Assert.IsNotNull(windowParent);
		}
	}
}
