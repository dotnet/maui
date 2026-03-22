#nullable disable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="Application.RequestedThemeChanged"/> event.
	/// </summary>
	public class AppThemeChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AppThemeChangedEventArgs"/> class.
		/// </summary>
		/// <param name="appTheme">The new app theme.</param>
		public AppThemeChangedEventArgs(AppTheme appTheme) =>
			RequestedTheme = appTheme;

		/// <summary>
		/// Gets the new requested theme.
		/// </summary>
		public AppTheme RequestedTheme { get; }
	}
}