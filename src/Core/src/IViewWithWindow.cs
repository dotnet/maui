#nullable enable

namespace Microsoft.Maui
{
	/// <summary>
	/// Internal interface for views that can provide access to their window.
	/// This enables dependency injection for testing scenarios.
	/// </summary>
	internal interface IViewWithWindow
	{
		/// <summary>
		/// Gets the window associated with this view.
		/// </summary>
		IWindow? Window { get; }
	}
}