namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	/// <summary>Enumerates navigation transition styles for macOS.</summary>
	public enum NavigationTransitionStyle
	{
		/// <summary>Indicates no transition animation.</summary>
		None,
		/// <summary>Indicates a crossfade transition.</summary>
		Crossfade,
		/// <summary>Indicates a slide-up transition.</summary>
		SlideUp,
		/// <summary>Indicates a slide-down transition.</summary>
		SlideDown,
		/// <summary>Indicates a slide-left transition.</summary>
		SlideLeft,
		/// <summary>Indicates a slide-right transition.</summary>
		SlideRight,
		/// <summary>Indicates a slide-forward transition based on navigation direction.</summary>
		SlideForward,
		/// <summary>Indicates a slide-backward transition based on navigation direction.</summary>
		SlideBackward
	}
}
