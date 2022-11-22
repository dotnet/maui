namespace Microsoft.Maui.Controls.Foldable
{
	/// <summary>
	/// Defines constants that specify how panes are shown in a TwoPaneView in wide mode, 
	/// determined by <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewPriority" />
	/// </summary>
	public enum TwoPaneViewWideModeConfiguration
	{
		/// <summary>
		/// Only the pane that has priority is shown, the other pane is hidden.
		/// </summary>
		SinglePane,
		/// <summary>
		/// The pane that has priority is shown on the left, the other pane is shown on the right.
		/// </summary>
		LeftRight,
		/// <summary>
		/// The pane that has priority is shown on the right, the other pane is shown on the left.
		/// </summary>
		RightLeft
	}
}
