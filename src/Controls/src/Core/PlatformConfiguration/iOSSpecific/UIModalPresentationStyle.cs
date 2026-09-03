namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	/// <summary>
	/// Enumerates valid modal presentation styles for iOS.
	/// </summary>
	public enum UIModalPresentationStyle
	{
		/// <summary>The content is displayed in a manner that covers the screen.</summary>
		/// <remarks>The views belonging to the presenting view controller are removed after the presentation completes.</remarks>
		FullScreen,

		/// <summary>The content is displayed in the center of the screen.</summary>
		FormSheet,

		/// <summary>The system selects an appropriate presentation style for the view controller.</summary>
		Automatic,

		/// <summary>The content is displayed in a manner that covers the screen.</summary>
		/// <remarks>The views belonging to the presenting view controller are not removed after the presentation completes.</remarks>
		OverFullScreen,

		/// <summary>The content is displayed in a popover view.</summary>
		PageSheet,

		/// <summary>The content is displayed in a popover view.</summary>
		Popover,

	}
}