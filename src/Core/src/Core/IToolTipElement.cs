namespace Microsoft.Maui
{
	/// <summary>
	/// Indicates that this element has a ToolTip to show.
	/// </summary>
	public interface IToolTipElement
	{
		/// <summary>
		/// Represents a small rectangular pop-up window that displays a brief description of a 
		/// view's purpose when the user rests the pointer on the view.
		/// </summary>
		ToolTip? ToolTip { get; }
	}
}
