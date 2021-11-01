using System.Collections.Generic;

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
		IWindow CreateWindow(IActivationState activationState);

		/// <summary>
		/// Notify a theme change.
		/// </summary>
		void ThemeChanged();
	}
}