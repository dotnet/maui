namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that reacts to touch events.
	/// </summary>
	public interface IButton : IView, IText, IPadding
	{
		/// <summary>
		/// Allows you to display a bitmap image on the Button.
		/// </summary>
		IImageSource? ImageSource { get; }


		void ImageSourceLoaded();

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