namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies how the flyout header behaves when scrolling.</summary>
	public enum FlyoutHeaderBehavior
	{
		/// <summary>Header uses platform-specific default scroll behavior.</summary>
		Default,
		/// <summary>Header remains fixed and does not scroll.</summary>
		Fixed,
		/// <summary>Header scrolls with the flyout content.</summary>
		Scroll,
		/// <summary>Header collapses as the user scrolls down.</summary>
		CollapseOnScroll,
	}
}