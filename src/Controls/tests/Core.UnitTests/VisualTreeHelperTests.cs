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
			Assert.AreEqual(verticalStackLayout.Children.Count, verticalStackLayout.GetVisualChildren().Count);
		}

		[Test]
		public void HorizontalStackLayoutChildren()
		{
			var horizontalStackLayout = new HorizontalStackLayout() { IsPlatformEnabled = true };
			horizontalStackLayout.Children.Add(new Label());
			horizontalStackLayout.Children.Add(new Button());
			Assert.AreEqual(horizontalStackLayout.Children.Count, horizontalStackLayout.GetVisualChildren().Count);
		}

		[Test]
		public void StackLayoutChildren()
		{
			var stackLayout = new StackLayout() { IsPlatformEnabled = true };
			stackLayout.Children.Add(new Label());
			stackLayout.Children.Add(new Button());
			Assert.AreEqual(stackLayout.Children.Count, stackLayout.GetVisualChildren().Count);
		}

		[Test]
		public void ApplicationChildren()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();

			app.MainPage = page;

			var window = iapp.CreateWindow(null) as Window;

			var appChildren = app.GetVisualChildren();

			Assert.Greater(appChildren.Count, 0);
			Assert.IsTrue(appChildren[0] is IWindow);
			Assert.Greater(window.GetVisualChildren().Count, 0);
		}
	}
}
