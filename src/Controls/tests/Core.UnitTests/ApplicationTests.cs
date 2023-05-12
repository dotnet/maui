using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ApplicationTests : BaseTestFixture
	{
		[Fact]
		public void NewApplicationHasNoWindowsNorPage()
		{
			var app = new Application();

			Assert.Null(app.MainPage);
			Assert.Empty(app.Windows);
		}

		[Fact]
		public void SettingMainPageSetsMainPageButNotWindow()
		{
			var app = new Application();
			var page = new ContentPage();

			app.MainPage = page;

			Assert.Equal(page, app.MainPage);
			Assert.Empty(app.Windows);
		}

		[Fact]
		public void CreateWindowUsesMainPage()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();

			app.MainPage = page;

			var window = iapp.CreateWindow(null);

			Assert.Equal(page, app.MainPage);
			Assert.NotEmpty(app.Windows);
			Assert.Equal(1, app.Windows.Count);
			Assert.Equal(window, app.Windows[0]);
			Assert.Equal(page, app.Windows[0].Page);
		}

		[Fact]
		public void SettingMainPageUpdatesWindow()
		{
			var app = new Application();
			var iapp = app as IApplication;
			var page = new ContentPage();

			app.MainPage = page;
			var window = iapp.CreateWindow(null);

			var page2 = new ContentPage();
			app.MainPage = page2;

			Assert.Equal(page2, app.MainPage);
			Assert.NotEmpty(app.Windows);
			Assert.Equal(1, app.Windows.Count);
			Assert.Equal(window, app.Windows[0]);
			Assert.Equal(page2, app.Windows[0].Page);
		}

		[Fact]
		public void NotSettingMainPageThrows()
		{
			var app = new Application();
			var iapp = app as IApplication;

			Assert.Throws<NotImplementedException>(() => iapp.CreateWindow(null));
		}

		[Fact]
		public void SettingMainPageAndOverridingCreateWindowWithSamePageIsValid()
		{
			var page = new ContentPage();
			var window = new Window(page);

			var app = new StubApp() { MainWindow = window, MainPage = page };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			Assert.Equal(window, win);
			Assert.Equal(window.Page, page);
			Assert.Equal(app.MainPage, page);
		}

		[Fact]
		public void SettingMainPageAndOverridingCreateWindowThrows()
		{
			var window = new Window(new ContentPage());

			var app = new StubApp() { MainWindow = window, MainPage = new ContentPage() };
			var iapp = app as IApplication;

			Assert.Throws<InvalidOperationException>(() => iapp.CreateWindow(null));
		}

		[Fact]
		public void CanUseExistingWindow()
		{
			var window = new Window();

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			Assert.Equal(window, win);
			Assert.Null(app.MainPage);
		}

		[Fact]
		public void CanUseExistingWindowWithPage()
		{
			var window = new Window { Page = new ContentPage() };

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			Assert.Equal(window, win);
			Assert.Equal(window.Page, app.MainPage);
		}

		[Fact]
		public void SettingMainPageOverwritesExistingPage()
		{
			var window = new Window { Page = new ContentPage() };

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			var page2 = new ContentPage();
			app.MainPage = page2;

			Assert.Equal(window, win);
			Assert.Equal(page2, app.MainPage);
			Assert.Equal(window.Page, app.MainPage);
		}

		[Fact]
		public void SettingWindowPageOverwritesMainPage()
		{
			var window = new Window { Page = new ContentPage() };

			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;

			var win = iapp.CreateWindow(null);

			var page2 = new ContentPage();
			window.Page = page2;

			Assert.Equal(window, win);
			Assert.Equal(page2, app.MainPage);
			Assert.Equal(window.Page, app.MainPage);
		}

		[Fact]
		public void OnStartFiresOnceFromWindowCreated()
		{
			var window = new Window { Page = new ContentPage() };
			var app = new StubApp { MainWindow = window };
			var iapp = app as IApplication;
			var win = iapp.CreateWindow(null);

			Assert.Equal(0, app.OnStartCount);
			(window as IWindow).Created();
			Assert.Equal(1, app.OnStartCount);
			Assert.Throws<InvalidOperationException>(() => (window as IWindow).Created());
			Assert.Equal(1, app.OnStartCount);

		}

		class StubApp : Application
		{
			public int OnStartCount { get; private set; }
			public StubApp() : base(false)
			{

			}

			public Window MainWindow { get; set; }

			protected override Window CreateWindow(IActivationState activationState)
			{
				return MainWindow;
			}

			protected override void OnStart()
			{
				base.OnStart();
				OnStartCount++;
			}
		}
	}
}
