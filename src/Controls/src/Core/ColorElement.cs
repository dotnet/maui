using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class ColorElement
	{
		public static readonly BindableProperty ColorProperty =
			BindableProperty.Create(nameof(IColorElement.Color), typeof(Color), typeof(IColorElement), null, propertyChanged: OnColorPropertyChanged);

		static void OnColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// IColorElement doesn't provide handling for property changes directly (the way ITextElement does);
			// instead we check to see if the BindableObject implements IMapColorPropertyToPaint and use that to update relevant core property
			if (bindable is IMapColorPropertyToPaint mapper)
			{
				mapper.MapColorPropertyToPaint((Color)newValue);
			}
		}
	}
}