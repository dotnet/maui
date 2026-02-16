namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	/// <summary>Enumerates navigation transition animation styles for macOS.</summary>
	public enum NavigationTransitionStyle
	{
		/// <summary>No transition animation.</summary>
		None,
		/// <summary>A crossfade transition animation.</summary>
		Crossfade,
		/// <summary>A slide up transition animation.</summary>
		SlideUp,
		/// <summary>A slide down transition animation.</summary>
		SlideDown,
		/// <summary>A slide left transition animation.</summary>
		SlideLeft,
		/// <summary>A slide right transition animation.</summary>
		SlideRight,
		/// <summary>A slide forward transition animation that respects the current layout direction.</summary>
		SlideForward,
		/// <summary>A slide backward transition animation that respects the current layout direction.</summary>
		SlideBackward
	}
}
