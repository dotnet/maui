using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class ColorElement
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty =
			BindableProperty.Create(nameof(IColorElement.Color), typeof(Color), typeof(IColorElement), null);
	}
}