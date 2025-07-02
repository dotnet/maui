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

		/// <summary>
		/// Internal property for the Page's SafeAreaInsets Thickness that may be changed in the future.
		/// </summary>
		internal Thickness SafeAreaInsets { set => { } } // Default no-op implementation

		/// <summary>
		/// Gets the safe area behavior for the specified edge (0=Left, 1=Top, 2=Right, 3=Bottom).
		/// </summary>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>True if safe area should be ignored for this edge, false otherwise.</returns>
		bool IgnoreSafeAreaForEdge(int edge)
		{
			if (this is Microsoft.Maui.Controls.BindableObject bindable)
			{
				return Microsoft.Maui.Controls.SafeAreaGuides.ShouldIgnoreSafeAreaForEdge(bindable, edge);
			}
			return IgnoreSafeArea; // Fallback to legacy behavior
		}
	}
}
