namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a visual element that is used to place layouts and controls on the screen.
	/// </summary>
	public interface IView : IFrameworkElement
	{
		/// <summary>
		/// The Margin represents the distance between an view and its adjacent views.
		/// </summary>
		Thickness? Margin { get; }
	}
}