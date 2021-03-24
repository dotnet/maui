using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Allow to get iOS UIApplication lifecycle callbacks.
	/// </summary>
	public interface IIosApplicationLifetime : IPlatformApplicationLifetime
	{
		/// <summary>
		/// Method invoked after the application has launched to configure the main window and view controller.
		/// </summary>
		/// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
		/// <param name="launchOptions">An NSDictionary with the launch options, can be null.</param>
		/// <returns>Boolean</returns>
		bool FinishedLaunching(UIApplication application, NSDictionary launchOptions);

		/// <summary>
		/// Called when the application is launched and every time the app returns to the foreground.
		/// </summary>
		/// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
		void OnActivated(UIApplication application);

		/// <summary>
		/// The app is about to move from the active state to the inactive state.
		/// </summary>
		/// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
		void OnResignActivation(UIApplication application);

		/// <summary>
		/// Called if the application is being terminated due to memory constraints or directly by the user.
		/// </summary>
		/// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
		void WillTerminate(UIApplication application);

		/// <summary>
		/// Indicates that the application has entered the background.
		/// </summary>
		/// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
		void DidEnterBackground(UIApplication application);

		/// <summary>
		/// Called prior to the application returning from a backgrounded state.
		/// </summary>
		/// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
		void WillEnterForeground(UIApplication application);
	}
}