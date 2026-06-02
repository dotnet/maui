using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies how the flyout page displays on the screen.</summary>
	public enum FlyoutLayoutBehavior
	{
		/// <summary>The flyout page layout is determined by the platform, based on device idiom and orientation.</summary>
		Default = 0,
		/// <summary>The flyout and detail are displayed side-by-side in landscape orientation.</summary>
		SplitOnLandscape = 1,
		/// <summary>The flyout and detail are always displayed side-by-side.</summary>
		Split = 2,
		/// <summary>The flyout is always displayed as a popover that slides in from the side.</summary>
		Popover = 3,
		/// <summary>The flyout and detail are displayed side-by-side in portrait orientation.</summary>
		SplitOnPortrait = 4
	}
}