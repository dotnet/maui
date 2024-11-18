using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui
{
	/// <summary>
	/// Class that represents a cross-platform .NET MAUI application.
	/// </summary>
	public interface IApplication : IElement
	{
		/// <summary>
		/// Gets the instantiated windows in an application.
		/// </summary>
		IReadOnlyList<IWindow> Windows { get; }

		/// <summary>
		/// Instantiate a new window.
		/// </summary>
		/// <param name="activationState">Argument containing specific information on each platform.</param>
		/// <returns>The created window.</returns>
		IWindow CreateWindow(IActivationState? activationState);

		/// <summary>
		/// Requests that the application open a new window.
		/// </summary>
		/// <param name="window">The window to open.</param>
		void OpenWindow(IWindow window);

		/// <summary>
		/// Requests that the application closes the window.
		/// </summary>
		/// <param name="window">The window to close.</param>
		void CloseWindow(IWindow window);

		/// <summary>
		/// Request that the application brings to the front the given window.
		/// </summary>
		/// <param name="window">The window to bring to the front.</param>
#if NETSTANDARD2_0
		void ActivateWindow(IWindow window);
#else
		void ActivateWindow(IWindow window) { }
#endif


		/// <summary>
		/// Gets the current requested theme by the system for your application. 
		/// The return value will be one of the following:
		/// - Unspecified
		/// - Light
		/// - Dark
		/// </summary>
		AppTheme UserAppTheme { get; }

		/// <summary>
		/// Notify a theme change.
		/// </summary>
		void ThemeChanged();
	}
}