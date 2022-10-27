using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class TextElement
	{
		/// <summary>
		/// The backing store for the <see cref="ITextElement.TextColor" /> bindable property.
		/// </summary>
		public static readonly BindableProperty TextColorProperty =
			BindableProperty.Create(nameof(ITextElement.TextColor), typeof(Color), typeof(ITextElement), null,
									propertyChanged: OnTextColorPropertyChanged);

		/// <summary>
		/// The backing store for the <see cref="ITextElement.CharacterSpacing" /> bindable property.
		/// </summary>
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

		/// <summary>
		/// The backing store for the <see cref="ITextElement.TextTransform" /> bindable property.
		/// </summary>
		public static readonly BindableProperty TextTransformProperty =
			BindableProperty.Create(nameof(ITextElement.TextTransform), typeof(TextTransform), typeof(ITextElement), TextTransform.Default,
							propertyChanged: OnTextTransformPropertyChanged);

		static void OnTextTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((ITextElement)bindable).OnTextTransformChanged((TextTransform)oldValue, (TextTransform)newValue);
		}
	}
}