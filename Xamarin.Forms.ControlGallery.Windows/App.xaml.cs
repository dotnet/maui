using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Xamarin.Forms.ControlGallery.Windows
{
	public sealed partial class App
	{
		public App()
		{
			InitializeComponent();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			var rootFrame = Window.Current.Content as global::Windows.UI.Xaml.Controls.Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new global::Windows.UI.Xaml.Controls.Frame();
				// Set the default language
				rootFrame.Language = global::Windows.Globalization.ApplicationLanguages.Languages[0];

				rootFrame.NavigationFailed += OnNavigationFailed;

				Forms.Init (e);
				FormsMaps.Init (Controls.App.Config["Win8MapsAuthKey"]);

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null)
			{
				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				rootFrame.Navigate(typeof(MainPage), e.Arguments);
			}
			// Ensure the current window is active
			Window.Current.Activate();
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
}