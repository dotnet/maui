namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : MauiWinUIApplication
	{
		public static bool RunningAsUITests { get; private set; }

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			InitializeComponent();
			//Suspending += OnSuspending;
		}

		protected override IStartup OnCreateStartup() => new Startup();

		public override MauiWinUIWindow CreateWindow()
		{
			return new MainPage();
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
		{
			base.OnLaunched(e);
			if (!String.IsNullOrWhiteSpace(e.Arguments) &&
				e.Arguments.Contains("RunningAsUITests"))
			{
				RunningAsUITests = true;
				ControlGallery.App.PreloadTestCasesIssuesList = false;
			}
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}
	}

	public class MiddleApp : MauiWinUIApplication<WinUIStartup>
	{
	}
}
