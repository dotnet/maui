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
		/// Gets the current visual diagnostics layer for the Window.
		/// </summary>
		IVisualDiagnosticsLayer VisualDiagnosticsLayer { get; }

		/// <summary>
		/// Occurs when the Window is created.
		/// </summary>
		void Created();

		/// <summary>
		/// Occurs when the Window is resumed from a sleeping state.
		/// </summary>
		void Resumed();

		/// <summary>
		/// Occurs when the Window is activated.
		/// </summary>
		void Activated();

		/// <summary>
		/// Occurs when the Window is deactivated.
		/// </summary>
		void Deactivated();

		/// <summary>
		/// Occurs when the Window is stopped.
		/// </summary>
		void Stopped();

		/// <summary>
		/// Occurs when the Window is destroyed.
		/// </summary>
		void Destroying();

		/// <summary>
		/// Occurs when the Window is entering a background state.
		/// </summary>
		void Backgrounding(IPersistedState state);

		/// <summary>
		/// Occurs when the back button is pressed.
		/// </summary>
		/// <returns>Whether or not the back navigation was handled.</returns>
		bool BackButtonClicked();
	}
}