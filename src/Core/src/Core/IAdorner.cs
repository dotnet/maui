namespace Microsoft.Maui
{
	public interface IAdorner : IWindowOverlayElement
	{
		/// <summary>
		/// Gets the Density for the Adorner.
		/// Used to override the default density behavior for the containing border. 
		/// </summary>
		float Density { get; }

		/// <summary>
		/// Gets the underlying <see cref="IView"/> that makes up the border.
		/// </summary>
		IView VisualView { get; }
	}
}