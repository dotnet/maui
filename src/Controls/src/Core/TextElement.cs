namespace Microsoft.Maui.Controls
{
	static class TextElement
	{
		public static readonly BindableProperty TextColorProperty =
			BindableProperty.Create(nameof(ITextElement.TextColor), typeof(Color), typeof(ITextElement), Color.Default,
									propertyChanged: OnTextColorPropertyChanged);

		public static readonly BindableProperty CharacterSpacingProperty =
			BindableProperty.Create(nameof(ITextElement.CharacterSpacing), typeof(double), typeof(ITextElement), 0.0d,
				propertyChanged: OnCharacterSpacingPropertyChanged);

		static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextElement)bindable).OnTextColorPropertyChanged((Color)oldValue, (Color)newValue);
		}

		static void OnCharacterSpacingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextElement)bindable).OnCharacterSpacingPropertyChanged((double)oldValue, (double)newValue);
		}

		public static readonly BindableProperty TextTransformProperty =
			BindableProperty.Create(nameof(ITextElement.TextTransform), typeof(TextTransform), typeof(ITextElement), TextTransform.Default,
							propertyChanged: OnTextTransformPropertyChanged);

		static void OnTextTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextElement)bindable).OnTextTransformChanged((TextTransform)oldValue, (TextTransform)newValue);
		}
	}
}