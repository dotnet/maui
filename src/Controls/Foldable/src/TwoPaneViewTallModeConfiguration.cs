namespace Microsoft.Maui.Controls.Foldable
{
	/// <summary>
	/// Defines constants that specify how panes are shown in a TwoPaneView in tall mode, 
	/// determined by <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewPriority" />
	/// </summary>
	public enum TwoPaneViewTallModeConfiguration
	{
		/// <summary>
		/// Only the pane that has priority is shown, the other pane is hidden.
		/// </summary>
		SinglePane,
		/// <summary>
		/// The pane that has priority is shown on top, the other pane is shown on the bottom.
		/// </summary>
		TopBottom,
		/// <summary>
		/// The pane that has priority is shown on the bottom, the other pane is shown on top.
		/// </summary>
		BottomTop
	}
}
