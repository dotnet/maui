using Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries;

namespace Xamarin.Forms.Controls
{
	public class PlatformSpecificsGallery : ContentPage
	{
		Page _originalRoot;

		public PlatformSpecificsGallery()
		{
			var mdpWindowsButton = new Button { Text = "Master Detail Page (Windows)" };
			var npWindowsButton = new Button { Text = "Navigation Page (Windows)" };
			var tbWindowsButton = new Button { Text = "Tabbed Page (Windows)" };
			var navpageiOSButton = new Button() { Text = "Navigation Page (iOS)" };
			var viselemiOSButton = new Button() { Text = "Visual Element (iOS)" };
			var appAndroidButton = new Button() { Text = "Application (Android)" };
			var tbAndroidButton = new Button { Text = "TabbedPage (Android)" };
			var entryiOSButton = new Button() { Text = "Entry (iOS)" };

			mdpWindowsButton.Clicked += (sender, args) => { SetRoot(new MasterDetailPageWindows(new Command(RestoreOriginal))); };
			npWindowsButton.Clicked += (sender, args) => { SetRoot(new NavigationPageWindows(new Command(RestoreOriginal))); };
			tbWindowsButton.Clicked += (sender, args) => { SetRoot(new TabbedPageWindows(new Command(RestoreOriginal))); };
			navpageiOSButton.Clicked += (sender, args) => { SetRoot(NavigationPageiOS.Create(new Command(RestoreOriginal))); };
			viselemiOSButton.Clicked += (sender, args) => { SetRoot(new VisualElementiOS(new Command(RestoreOriginal))); };
			appAndroidButton.Clicked += (sender, args) => { SetRoot(new ApplicationAndroid(new Command(RestoreOriginal))); };
			tbAndroidButton.Clicked += (sender, args) => { SetRoot(new TabbedPageAndroid(new Command(RestoreOriginal))); };
			entryiOSButton.Clicked += (sender, args) => { Navigation.PushAsync(new EntryPageiOS()); };
			

			Content = new StackLayout
			{
				Children = { mdpWindowsButton, npWindowsButton, tbWindowsButton, navpageiOSButton, viselemiOSButton, appAndroidButton, entryiOSButton }
			};
		}

		void SetRoot(Page page)
		{
			var app = Application.Current as App;
			if (app == null)
			{
				return;
			}

			_originalRoot = app.MainPage;
			app.SetMainPage(page);
		}

		void RestoreOriginal()
		{
			if (_originalRoot == null)
			{
				return;
			}

			var app = Application.Current as App;
			app?.SetMainPage(_originalRoot);
		}
	}
}