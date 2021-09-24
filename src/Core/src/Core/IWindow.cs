namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the ability to create, configure, show, and manage Windows.
	/// </summary>
	public interface IWindow : ITitledElement
	{
		/// <summary>
		/// Gets or sets the current Page displayed in the Window.
		/// </summary>
		IView Content { get; }

		void Created();

		void Resumed();

		void Activated();

		void Deactivated();

		void Stopped();

		void Destroying();

		bool BackButtonPressed();
	}
}