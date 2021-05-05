using Microsoft.Maui;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Maui.Controls.Sample.WinUI3
{
	// TODO: this is not nice.
	public class MiddleApp : MauiWinUIApplication<Startup>
	{
	}

	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : MiddleApp
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			InitializeComponent();
			//TODO WINUI
			//Suspending += OnSuspending;
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
			// Save application state and stop any background activity
		}
	}
}