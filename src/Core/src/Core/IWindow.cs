namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the ability to create, configure, show, and manage Windows.
	/// </summary>
	public interface IWindow : IFrameworkElement
	{
		/// <summary>
		/// Gets or sets the current Page displayed in the Window.
		/// </summary>
		IView View { get; set; }
	}
}