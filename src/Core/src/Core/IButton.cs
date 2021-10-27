namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that reacts to touch events.
	/// </summary>
	public interface IButton : IView, IPadding
	{
		/// <summary>
		/// Occurs when the Button is pressed.
		/// </summary>
		void Pressed();

		/// <summary>
		/// Occurs when the Button is released.
		/// </summary>
		void Released();

		/// <summary>
		/// Occurs when the Button is clicked.
		/// </summary>
		void Clicked();
	}
}