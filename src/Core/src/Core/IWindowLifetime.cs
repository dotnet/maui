using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the methods and events to respond to Window lifecycle changes.
	/// </summary>
	public interface IWindowLifetime
	{
		/// <summary>
		/// Called when the Window is created.
		/// </summary>
		void OnCreated();

		/// <summary>
		/// Called when the application is not visible to the user.
		/// </summary>
		void OnResumed();

		/// <summary>
		/// Called before the application is closed.
		/// </summary>
		void OnPaused();

		/// <summary>
		/// Called when the Window is closed.
		/// </summary>
		void OnStopped();

		/// <summary>
		/// This event is raised when the window is closed.
		/// </summary>
		event EventHandler? Closed;

		/// <summary>
		/// This event is raised when the window is created.
		/// </summary>
		event EventHandler? Created;

		/// <summary>
		/// This event is raised when the window is resumed.
		/// </summary>
		event EventHandler? Resumed;

		/// <summary>
		/// This event is raised when the window is paused.
		/// </summary>
		event EventHandler? Paused;

		/// <summary>
		/// This event is raised when the window is closed.
		/// </summary>
		event EventHandler? Stopped;
	}
}
