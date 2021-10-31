namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the ability to create, configure, show, and manage Windows.
	/// </summary>
	public interface IWindow : ITitledElement
	{
		/// <summary>
		/// Gets the current Page displayed in the Window.
		/// </summary>
		IView Content { get; }

		/// <summary>
		/// Gets the current Highlight Layer, used for highlighting elements in the Window.
		/// </summary>
		IHighlightLayer HighlightLayer { get; }

		void Created();

		void Resumed();

		void Activated();

		void Deactivated();

		void Stopped();

		void Destroying();

		bool BackButtonClicked();
	}
}