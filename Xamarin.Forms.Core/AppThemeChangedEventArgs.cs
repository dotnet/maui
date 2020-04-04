using System;

namespace Xamarin.Forms
{
    public class AppThemeChangedEventArgs : EventArgs
    {
        public AppThemeChangedEventArgs(AppTheme appTheme) =>
            RequestedTheme = appTheme;

        public AppTheme RequestedTheme { get; }
    }
}