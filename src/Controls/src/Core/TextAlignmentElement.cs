#nullable disable
namespace Microsoft.Maui.Controls
{
	static class TextAlignmentElement
	{
		/// <summary>Bindable property for <see cref="ITextAlignmentElement.HorizontalTextAlignment"/>.</summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty =
			BindableProperty.Create(nameof(ITextAlignmentElement.HorizontalTextAlignment), typeof(TextAlignment), typeof(ITextAlignmentElement), TextAlignment.Start,
									propertyChanged: OnHorizontalTextAlignmentPropertyChanged);

		/// <summary>Bindable property for <see cref="ITextAlignmentElement.VerticalTextAlignment"/>.</summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty =
			BindableProperty.Create(nameof(ITextAlignmentElement.VerticalTextAlignment), typeof(TextAlignment), typeof(ITextAlignmentElement), TextAlignment.Center);

		static void OnHorizontalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextAlignmentElement)bindable).OnHorizontalTextAlignmentPropertyChanged((TextAlignment)oldValue, (TextAlignment)newValue);
		}
	}
}