namespace Xamarin.Forms
{
	static class TextAlignmentElement
	{
		public static readonly BindableProperty HorizontalTextAlignmentProperty =
			BindableProperty.Create(nameof(ITextAlignmentElement.HorizontalTextAlignment), typeof(TextAlignment), typeof(EntryCell), TextAlignment.Start,
									propertyChanged: OnHorizontalTextAlignmentPropertyChanged);

		static void OnHorizontalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextAlignmentElement)bindable).OnHorizontalTextAlignmentPropertyChanged((TextAlignment)oldValue, (TextAlignment)newValue);
		}
	}
}