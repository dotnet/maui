using Maui.Controls.Sample;

namespace Maui.Controls.Sample
{
	public partial class App : Application
	{
		public const string AppName = "CompatibilityGalleryControls";
		public const string DefaultMainPageId = "ControlGalleryMainPage";

		public App()
		{
			InitializeComponent();
			SetMainPage(CreateDefaultMainPage());
		}

		public static bool PreloadTestCasesIssuesList { get; set; } = true;

		public void SetMainPage(Page rootPage)
		{
			MainPage = rootPage;
		}

		public Page CreateDefaultMainPage()
		{
			return new CoreNavigationPage();
		}

		protected override void OnAppLinkRequestReceived(Uri uri)
		{
			base.OnAppLinkRequestReceived(uri);
		}

#if WINDOWS || MACCATALYST
		protected override Window CreateWindow(IActivationState activationState)
		{
			var window = base.CreateWindow(activationState);

			// For desktop use a fixed window size, so that screenshots are deterministic,
			// matching (as much as possible) between dev machines and CI. Currently
			// our Windows CI machines run at 1024x768 resolution and the window size can't
			// be larger than screen size. We'll investigate increasing CI screen
			// resolution in the future so we can have a bigger window, but for now
			// this size works.
			const int desktopWindowWidth = 1024;
			const int desktopWindowHeight = 768;

#if WINDOWS
			window.Width = desktopWindowWidth;
			window.Height = desktopWindowHeight;

			var info = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo;
			int screenWidth = (int)(info.Width / info.Density);
			int screenHeight = (int)(info.Height / info.Density);

			// Center the window on the screen, to ensure no part of it goes off screen in CI
			window.X = (screenWidth - desktopWindowWidth) / 2;
			window.Y = (screenHeight - desktopWindowHeight) / 2;
#elif MACCATALYST
			// Setting max and min is currently needed to force the size on Catalyst;
			// just setting width/height has no effect on Catalyst
			window.MaximumWidth = desktopWindowWidth;
			window.MinimumWidth = desktopWindowWidth;

			window.MaximumHeight = desktopWindowHeight;
			window.MinimumHeight = desktopWindowHeight;
#endif


			return window;
		}
#endif
	}
}
