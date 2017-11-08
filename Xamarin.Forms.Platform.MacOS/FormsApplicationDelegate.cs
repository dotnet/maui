using System;
using System.ComponentModel;
using AppKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.macOS.Extensions;

namespace Xamarin.Forms.Platform.MacOS
{
	public abstract class FormsApplicationDelegate : NSApplicationDelegate
	{
		Application _application;
		bool _isSuspended;

		public abstract NSWindow MainWindow { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing && _application != null)
				_application.PropertyChanged -= ApplicationOnPropertyChanged;

			base.Dispose(disposing);
		}

		protected void LoadApplication(Application application)
		{
			if (application == null)
				throw new ArgumentNullException(nameof(application));

			Application.SetCurrentApplication(application);
			_application = application;

			application.PropertyChanged += ApplicationOnPropertyChanged;
		}

		public override void DidFinishLaunching(Foundation.NSNotification notification)
		{
			if (MainWindow == null)
				throw new InvalidOperationException("Please provide a main window in your app");

			MainWindow.Display();
			MainWindow.MakeKeyAndOrderFront(NSApplication.SharedApplication);
			if (_application == null)
				throw new InvalidOperationException("You MUST invoke LoadApplication () before calling base.FinishedLaunching ()");

			SetMainPage();
			UpdateMainMenu();
			_application.SendStart();
		}

		public override void DidBecomeActive(Foundation.NSNotification notification)
		{
			// applicationDidBecomeActive
			// execute any OpenGL ES drawing calls
			if (_application == null || !_isSuspended) return;
			_isSuspended = false;
			_application.SendResume();
		}

		public override async void DidResignActive(Foundation.NSNotification notification)
		{
			// applicationWillResignActive
			if (_application == null) return;
			_isSuspended = true;
			await _application.SendSleepAsync();
		}

		void ApplicationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Application.MainPage))
				UpdateMainPage();
			if (e.PropertyName == nameof(Menu))
				UpdateMainMenu();
		}

		void SetMainPage()
		{
			UpdateMainPage();
		}

		void UpdateMainPage()
		{
			if (_application.MainPage == null)
				return;

			var platformRenderer = (PlatformRenderer)MainWindow.ContentViewController;
			MainWindow.ContentViewController = _application.MainPage.CreateViewController();
			(platformRenderer?.Platform as IDisposable)?.Dispose();
		}

		void UpdateMainMenu()
		{
			var mainMenu = Element.GetMenu(_application);
			var nsMenu = NSApplication.SharedApplication.MainMenu;
			if (mainMenu != null)
				SetMainMenu(mainMenu);
			else if (nsMenu != null && nsMenu.Count >= 2)
				ClearNSMenu(nsMenu);
		}

		void SetMainMenu(Menu mainMenu)
		{
			mainMenu.PropertyChanged -= MainMenuOnPropertyChanged;
			mainMenu.PropertyChanged += MainMenuOnPropertyChanged;
			MainMenuOnPropertyChanged(this, null);
		}

		void MainMenuOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var nsMenu = NSApplication.SharedApplication.MainMenu;
			if (nsMenu == null)
			{
				Log.Warning("FormsApplicationDelegate", "Please provide a Main.storyboard to handle menus");
				return;
			}
				
			ClearNSMenu(nsMenu);
			Element.GetMenu(_application).ToNSMenu(nsMenu);
		}

		static void ClearNSMenu(NSMenu menu)
		{
			//for now we can't remove the 1st menu item		
			for (var i = menu.Count - 1; i > 0; i--)
				menu.RemoveItemAt(i);
		}
	}
}