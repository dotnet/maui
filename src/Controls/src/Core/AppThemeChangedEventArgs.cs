using System;

namespace Microsoft.Maui.Controls
{
	public class AppThemeChangedEventArgs : EventArgs
	{
		public AppThemeChangedEventArgs(OSAppTheme appTheme) =>
			RequestedTheme = appTheme;

		public OSAppTheme RequestedTheme { get; }
	}
}