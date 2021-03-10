namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the ability to create, configure, show, and manage Windows.
	/// </summary>
	public interface IWindow
	{
		/// <summary>
		/// Gets or sets the .NET MAUI Context.
		/// </summary>
		IMauiContext MauiContext { get; set; }

		/// <summary>
		/// Gets or sets the current Page displayed in the Window.
		/// </summary>
		IPage Page { get; set; }
	}
}