using System;

namespace Xamarin.Forms
{
	public class AppThemeChangedEventArgs : EventArgs
	{
		public AppThemeChangedEventArgs(OSAppTheme appTheme) =>
			RequestedTheme = appTheme;

		public OSAppTheme RequestedTheme { get; }
	}
}