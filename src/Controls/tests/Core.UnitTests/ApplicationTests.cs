using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ApplicationTests : BaseTestFixture
	{
		[Test]
		public void NewApplicationHasNoWindowsNorPage()
		{
			var app = new Application();

			Assert.Null(app.MainPage);
			Assert.IsEmpty(app.Windows);
		}

		[Test]
		public void SettingMainPageSetsMainPageAndWindow()
		{
			var app = new Application();
			var page = new ContentPage();

			app.MainPage = page;

			Assert.AreEqual(page, app.MainPage);
			Assert.AreEqual(1, app.Windows.Count);
			Assert.AreEqual(page, app.Windows[0].Page);
		}

		[Test]
		public void CreateWindowUsesMainPage()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();

			app.MainPage = page;

			var window = iapp.CreateWindow(null);

			Assert.AreEqual(page, app.MainPage);
			Assert.IsNotEmpty(app.Windows);
			Assert.AreEqual(1, app.Windows.Count);
			Assert.AreEqual(window, app.Windows[0]);
			Assert.AreEqual(page, app.Windows[0].Page);
		}

		[Test]
		public void SettingMainPageUpdatesWindow()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();

			app.MainPage = page;
			var window = iapp.CreateWindow(null);

			var page2 = new ContentPage();
			app.MainPage = page2;

			Assert.AreEqual(page2, app.MainPage);
			Assert.IsNotEmpty(app.Windows);
			Assert.AreEqual(1, app.Windows.Count);
			Assert.AreEqual(window, app.Windows[0]);
			Assert.AreEqual(page2, app.Windows[0].Page);
		}

		[Test]
		public void NotSettingMainPageThrows()
		{
			var app = new Application();
			var iapp = app as IApplication;

			Assert.Throws<NotImplementedException>(() => iapp.CreateWindow(null));
		}

		[Test]
		public void CanUseExistingWindow()
		{
			var window = new Window();

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			Assert.AreEqual(window, win);
			Assert.Null(app.MainPage);
		}

		[Test]
		public void CanUseExistingWindowWithPage()
		{
			var window = new Window { Page = new ContentPage() };

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			Assert.AreEqual(window, win);
			Assert.AreEqual(window.Page, app.MainPage);
		}

		[Test]
		public void SettingMainPageOverwritesExistingPage()
		{
			var window = new Window { Page = new ContentPage() };

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			var page2 = new ContentPage();
			app.MainPage = page2;

			Assert.AreEqual(window, win);
			Assert.AreEqual(page2, app.MainPage);
			Assert.AreEqual(window.Page, app.MainPage);
		}

		[Test]
		public void SettingWindowPageOverwritesMainPage()
		{
			var window = new Window { Page = new ContentPage() };

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			var page2 = new ContentPage();
			window.Page = page2;

			Assert.AreEqual(window, win);
			Assert.AreEqual(page2, app.MainPage);
			Assert.AreEqual(window.Page, app.MainPage);
		}

		class StubApp : Application
		{
			public Window MainWindow { get; set; }

			protected override Window CreateWindow(IActivationState activationState)
			{
				return MainWindow;
			}
		}
	}
}
