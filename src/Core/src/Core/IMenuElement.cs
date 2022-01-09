using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IMenuElement : IElement, IImageSourcePart, IText
	{
		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }
	}
}
