#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents a portion of formatted text for use in a FormattedString.</summary>
	[ContentProperty("Text")]
	public class Span : GestureElement, IFontElement, IStyleElement, ITextElement, ILineHeightElement, IDecorableTextElement
	{
		internal readonly MergedStyle _mergedStyle;

		/// <summary>Creates a new Span instance.</summary>
		public Span()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <summary>Bindable property for <see cref="Style"/>.</summary>
		public static readonly BindableProperty StyleProperty = BindableProperty.Create(nameof(Style), typeof(Style), typeof(Span), default(Style),
			propertyChanged: (bindable, oldvalue, newvalue) => ((Span)bindable)._mergedStyle.Style = (Style)newvalue, defaultBindingMode: BindingMode.OneWay);

		/// <summary>Bindable property for <see cref="TextDecorations"/>.</summary>
		public static readonly BindableProperty TextDecorationsProperty = DecorableTextElement.TextDecorationsProperty;

		/// <summary>Bindable property for <see cref="TextTransform"/>.</summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <summary>Gets or sets the style for the span. This is a bindable property.</summary>
		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty
			= BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(Span), default(Color), defaultBindingMode: BindingMode.OneWay);

		/// <summary>Gets or sets the background color for the span. This is a bindable property.</summary>
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Gets or sets the text color for the span. This is a bindable property.</summary>
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Gets or sets the character spacing for the span. This is a bindable property.</summary>
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <summary>Gets or sets the text transform for the span. This is a bindable property.</summary>
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		/// <summary>Transforms the source text using the specified text transform.</summary>
		/// <param name="source">The source text to transform.</param>
		/// <param name="textTransform">The text transformation to apply.</param>
		/// <returns>The transformed text.</returns>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilities.GetTransformedText(source, textTransform);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty
			= BindableProperty.Create(nameof(Text), typeof(string), typeof(Span), "", defaultBindingMode: BindingMode.OneWay);

		/// <summary>Gets or sets the text content of the span. This is a bindable property.</summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>Bindable property for <see cref="FontSize"/>.</summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>Bindable property for <see cref="FontAttributes"/>.</summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>Bindable property for <see cref="LineHeight"/>.</summary>
		public static readonly BindableProperty LineHeightProperty = LineHeightElement.LineHeightProperty;

		/// <summary>Gets or sets the font attributes for the span. This is a bindable property.</summary>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontElement.FontAttributesProperty); }
			set { SetValue(FontElement.FontAttributesProperty, value); }
		}

		/// <summary>Gets or sets the font family for the span. This is a bindable property.</summary>
		public string FontFamily
		{
			get { return (string)GetValue(FontElement.FontFamilyProperty); }
			set { SetValue(FontElement.FontFamilyProperty, value); }
		}

		/// <summary>Gets or sets the font size for the span. This is a bindable property.</summary>
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontElement.FontSizeProperty); }
			set { SetValue(FontElement.FontSizeProperty, value); }
		}

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		/// <summary>Gets or sets the text decorations for the span. This is a bindable property.</summary>
		public TextDecorations TextDecorations
		{
			get { return (TextDecorations)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		/// <summary>Gets or sets the line height multiplier for the span. This is a bindable property.</summary>
		public double LineHeight
		{
			get { return (double)GetValue(LineHeightElement.LineHeightProperty); }
			set { SetValue(LineHeightElement.LineHeightProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			var gestureRecognizers = GestureRecognizers;
			if (gestureRecognizers.Count > 0)
			{
				this.PropagateBindingContext(gestureRecognizers);
			}

			base.OnBindingContextChanged();
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue)
		{
		}

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue)
		{
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			double.NaN;

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
		{
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
		}

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
		{
		}

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) { }

		internal override void ValidateGesture(IGestureRecognizer gesture)
		{
			switch (gesture)
			{
				case TapGestureRecognizer tap:
				case null:
					break;
				default:
					throw new InvalidOperationException($"{gesture.GetType().Name} is not supported on a {nameof(Span)}");

			}
		}

		void ILineHeightElement.OnLineHeightChanged(double oldValue, double newValue)
		{
		}
	}
}
