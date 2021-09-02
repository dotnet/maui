namespace Microsoft.Maui.Primitives
{
	// We don't use Microsoft.Maui.Controls.LayoutAlignment directly because it has a Flags attribute, which we do not want

	/// <summary>
	/// Determines the position and size of an IView when arranged in a parent element
	/// </summary>
	public enum LayoutAlignment
	{
		/// <summary>
		/// Fill the available space
		/// </summary>
		Fill,

		/// <summary>
		/// Align with the leading edge of the available space, as determined by FlowDirection
		/// </summary>
		Start,

		/// <summary>
		/// Center in the available space
		/// </summary>
		Center,

		/// <summary>
		/// Align with the trailing edge of the available space, as determined by FlowDirection
		/// </summary>
		End
	}
}
