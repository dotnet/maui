using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using global::Windows.ApplicationModel;
using global::Windows.ApplicationModel.Activation;
using global::Windows.Foundation;
using global::Windows.Foundation.Collections;
using global::Windows.UI.Core;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls.Primitives;
using global::Windows.UI.Xaml.Data;
using global::Windows.UI.Xaml.Input;
using global::Windows.UI.Xaml.Media;
using global::Windows.UI.Xaml.Navigation;
using System.Maui;
using Application = global::Windows.UI.Xaml.Application;
using Frame = global::Windows.UI.Xaml.Controls.Frame;
using NavigationEventArgs = global::Windows.UI.Xaml.Navigation.NavigationEventArgs;

namespace Embedding.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
	            rootFrame.Navigated += OnNavigated;

	            System.Maui.Maui.Init(e);

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

	            // Register a handler for BackRequested events and set the
	            // visibility of the Back button
	            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

	            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
		            rootFrame.CanGoBack  ?
			            AppViewBackButtonVisibility.Visible :
			            AppViewBackButtonVisibility.Collapsed;
            }

            if (e.PrelaunchActivated == false)
            {
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
        }

	    private void OnBackRequested(object sender, BackRequestedEventArgs e)
	    {
		    Frame rootFrame = Window.Current.Content as Frame;

		    if (rootFrame.CanGoBack)
		    {
			    e.Handled = true;
			    rootFrame.GoBack();
		    }
	    }

	    private void OnNavigated(object sender, NavigationEventArgs e)
	    {
		    // Each time a navigation event occurs, update the Back button's visibility
		    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
			    ((Frame)sender).CanGoBack ?
				    AppViewBackButtonVisibility.Visible :
				    AppViewBackButtonVisibility.Collapsed;
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
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
