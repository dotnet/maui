using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Encapsulates the app and its available services.
	/// </summary>
	public interface IApplication
	{
		/// <summary>
		/// Gets the instantiated windows in an application.
		/// </summary>
		IReadOnlyList<IWindow> Windows { get; }

		/// <summary>
		/// Create the main window of the application.
		/// </summary>
		/// <param name="activationState">Set of parameters that helps to instantiate the window.</param>
		/// <returns>The main window.</returns>
		IWindow CreateWindow(IActivationState activationState);

		/// <summary>
		/// Shuts down the app.
		/// </summary>
		void Quit();
	}
}