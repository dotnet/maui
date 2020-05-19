using System;

namespace System.Maui
{
    public class AppThemeChangedEventArgs : EventArgs
    {
        public AppThemeChangedEventArgs(OSAppTheme appTheme) =>
            RequestedTheme = appTheme;

        public OSAppTheme RequestedTheme { get; }
    }
}