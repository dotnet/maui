namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality for requesting layout outside of the "safe" areas of the device screen.
	/// </summary>
	/// <remarks>
	/// This interface may be applied to any ILayout or IContentView.
	/// This interface is only recognized on the iOS/Mac Catalyst platforms; other platforms will ignore it.
	/// </remarks>
	public interface ISafeAreaView
	{
		/// <summary>
		/// Specifies how the View's content should be positioned in relation to obstructions. If this value is `false`, the 
		/// content will be positioned only in the unobstructed portion of the screen. If this value is `true`, the content
		/// may be positioned anywhere on the screen. This includes the portion of the screen behind toolbars, screen cutouts, etc.
		/// </summary>
		bool IgnoreSafeArea { get; }
	}
}
