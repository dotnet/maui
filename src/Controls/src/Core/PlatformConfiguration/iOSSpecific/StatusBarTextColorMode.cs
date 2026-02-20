namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	/// <summary>Specifies how iOS status bar text color adjusts based on navigation bar color.</summary>
	public enum StatusBarTextColorMode
	{
		/// <summary>Adjusts text color based on navigation bar title text luminosity.</summary>
		MatchNavigationBarTextLuminosity,
		/// <summary>Does not adjust the status bar text color.</summary>
		DoNotAdjust
	}
}
