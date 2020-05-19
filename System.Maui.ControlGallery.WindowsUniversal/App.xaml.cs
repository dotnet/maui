using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using global::Windows.ApplicationModel;
using global::Windows.ApplicationModel.Activation;
using global::Windows.Foundation;
using global::Windows.Foundation.Collections;
using global::Windows.Foundation.Metadata;
using global::Windows.UI;
using global::Windows.UI.ViewManagement;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Controls.Primitives;
using global::Windows.UI.Xaml.Data;
using global::Windows.UI.Xaml.Input;
using global::Windows.UI.Xaml.Media;
using global::Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

namespace System.Maui.ControlGallery.WindowsUniversal
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
			/* uncomment if you want to run tests without preloading
			 * issues list or change other behavior based on if tests
			 * are running in UI Harness
			 * if (!String.IsNullOrWhiteSpace(e.Arguments) &&
				e.Arguments.Contains("RunningAsUITests"))
				Controls.App.PreloadTestCasesIssuesList = false;*/
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
            {
             //   DebugSettings.EnableFrameRateCounter = true;
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

				System.Maui.Maui.SetFlags("Shell_UWP_Experimental", "SwipeView_Experimental", "MediaElement_Experimental", "AppTheme_Experimental");
				System.Maui.Maui.Init(e);
				//FormsMaps.Init (Controls.App.Config["UWPMapsAuthKey"]);

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

			//// Uncomment to test overriding the status bar color
			//if (ApiInformation.IsTypePresent("global::Windows.UI.ViewManagement.StatusBar"))
			//{
			//	var statusBar = StatusBar.GetForCurrentView();
			//	if (statusBar != null)
			//	{
			//		statusBar.BackgroundOpacity = 1;
			//		statusBar.BackgroundColor = Colors.Black;
			//		statusBar.ForegroundColor = Colors.White;
			//	}
			//}

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

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
