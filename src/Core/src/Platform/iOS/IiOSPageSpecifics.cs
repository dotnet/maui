namespace Microsoft.Maui.Platform;

/// <summary>
/// Provides functionality for requesting hiding the home indicator on the device screen.
/// </summary>
/// <remarks>
/// This interface may be applied to IContentView.
/// This interface is only recognized on the iOS platform; other platforms will ignore it.
/// </remarks>
internal interface IiOSPageSpecifics
{
	/// <summary>
	/// Gets a Boolean value that, if true, hides the HomeIndicator.
	/// </summary>
	bool IsHomeIndicatorAutoHidden { get; }

	/// <summary>
	/// Gets the preferred mode for the status bar.
	/// </summary>
	int PrefersStatusBarHiddenMode { get; }

	/// <summary>
	/// Gets the preferred animation for updating the status bar.
	/// </summary>
	int PreferredStatusBarUpdateAnimationMode { get; }
}