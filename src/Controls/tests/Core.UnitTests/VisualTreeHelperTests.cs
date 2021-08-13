using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class VisualTreeHelperTests : BaseTestFixture
	{
		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void LayoutChildren(Type TLayout)
		{
			var layout = (Layout)Activator.CreateInstance(TLayout);
			layout.IsPlatformEnabled = true;
			var label = new Label();
			var button = new Button();
			layout.Children.Add(label);
			layout.Children.Add(button);
			var visualChildren = (layout as IVisualTreeElement).GetVisualChildren();
			Assert.AreEqual(layout.Children.Count, visualChildren.Count);
			Assert.AreEqual(label, visualChildren[0]);
			Assert.AreEqual(button, visualChildren[1]);
		}

		[Test]
		public async Task ModalChildren()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();
			app.MainPage = page;
			var window = (Window)iapp.CreateWindow(null);
			var modalPage = new ContentPage();
			await window.Navigation.PushModalAsync(modalPage);
			var windowChildren = (window as IVisualTreeElement).GetVisualChildren();
			var modalParent = (modalPage as IVisualTreeElement).GetVisualParent();

			Assert.AreEqual(windowChildren.Count, 2);
			Assert.AreEqual(page, windowChildren[0]);
			Assert.AreEqual(modalPage, windowChildren[1]);
			Assert.AreEqual(window, modalParent);
		}

		[Test]
		public async Task ModalChildrenFiresDiagnosticEvents()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();
			app.MainPage = page;
			var window = (Window)iapp.CreateWindow(null);
			var modalPage = new ContentPage();
			VisualDiagnostics.VisualTreeChanged += OnVisualTreeChanged;
			VisualTreeChangeEventArgs lastArgs = null;

			await window.Navigation.PushModalAsync(modalPage);
			Assert.AreEqual(window, lastArgs.Parent);
			Assert.AreEqual(modalPage, lastArgs.Child);
			Assert.AreEqual(1, lastArgs.ChildIndex);
			lastArgs = null;

			await window.Navigation.PopModalAsync();
			Assert.AreEqual(window, lastArgs.Parent);
			Assert.AreEqual(modalPage, lastArgs.Child);
			Assert.AreEqual(1, lastArgs.ChildIndex);

			void OnVisualTreeChanged(object s, VisualTreeChangeEventArgs e)
			{
				lastArgs = e;
			}
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
