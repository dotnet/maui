using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IMenuElement : IElement, IImageSourcePart
	{
		/// <summary>
		/// Gets the paint which will fill the background of a View.
		/// </summary>
		Paint? Background { get; }

		/// <summary>
		/// Gets the text.
		/// </summary>

		string? Text { get; }

		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Gets a value that determines whether this View should be part of the visual tree or not.
		/// </summary>
		Visibility Visibility { get; }
	}
}
