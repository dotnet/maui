#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class PlaceholderElement
	{
		/// <summary>Bindable property for <see cref="IPlaceholderElement.Placeholder"/>.</summary>
		public static readonly BindableProperty PlaceholderProperty =
			BindableProperty.Create(nameof(IPlaceholderElement.Placeholder), typeof(string), typeof(IPlaceholderElement), default(string));

		/// <summary>Bindable property for <see cref="IPlaceholderElement.PlaceholderColor"/>.</summary>
		public static readonly BindableProperty PlaceholderColorProperty =
			BindableProperty.Create(nameof(IPlaceholderElement.PlaceholderColor), typeof(Color), typeof(IPlaceholderElement), default(Color));
	}
}