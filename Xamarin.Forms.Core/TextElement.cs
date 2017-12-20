namespace Xamarin.Forms
{
	static class TextElement
	{
		public static readonly BindableProperty TextColorProperty =
			BindableProperty.Create(nameof(ITextElement.TextColor), typeof(Color), typeof(ITextElement), Color.Default,
									propertyChanged: OnTextColorPropertyChanged);

		static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextElement)bindable).OnTextColorPropertyChanged((Color)oldValue, (Color)newValue);
		}
	}
}