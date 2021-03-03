namespace Microsoft.Maui.Controls
{
	static class TextAlignmentElement
	{
		public static readonly BindableProperty HorizontalTextAlignmentProperty =
			BindableProperty.Create(nameof(ITextAlignmentElement.HorizontalTextAlignment), typeof(TextAlignment), typeof(ITextAlignmentElement), TextAlignment.Start,
									propertyChanged: OnHorizontalTextAlignmentPropertyChanged);

		public static readonly BindableProperty VerticalTextAlignmentProperty =
			BindableProperty.Create(nameof(ITextAlignmentElement.VerticalTextAlignment), typeof(TextAlignment), typeof(ITextAlignmentElement), TextAlignment.Center);

		static void OnHorizontalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextAlignmentElement)bindable).OnHorizontalTextAlignmentPropertyChanged((TextAlignment)oldValue, (TextAlignment)newValue);
		}
	}
}