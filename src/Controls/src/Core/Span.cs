using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="Type[@FullName='Microsoft.Maui.Controls.Span']/Docs/*" />
	[ContentProperty("Text")]
	public class Span : GestureElement, IFontElement, IStyleElement, ITextElement, ILineHeightElement, IDecorableTextElement
	{
		internal readonly MergedStyle _mergedStyle;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Span()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='StyleProperty']/Docs/*" />
		public static readonly BindableProperty StyleProperty = BindableProperty.Create(nameof(Style), typeof(Style), typeof(Span), default(Style),
			propertyChanged: (bindable, oldvalue, newvalue) => ((Span)bindable)._mergedStyle.Style = (Style)newvalue, defaultBindingMode: BindingMode.OneWay);

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextDecorationsProperty']/Docs/*" />
		public static readonly BindableProperty TextDecorationsProperty = DecorableTextElement.TextDecorationsProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextTransformProperty']/Docs/*" />
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='Style']/Docs/*" />
		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='BackgroundColorProperty']/Docs/*" />
		public static readonly BindableProperty BackgroundColorProperty
			= BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(Span), default(Color), defaultBindingMode: BindingMode.OneWay);

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='BackgroundColor']/Docs/*" />
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public static readonly BindableProperty TextProperty
			= BindableProperty.Create(nameof(Text), typeof(string), typeof(Span), "", defaultBindingMode: BindingMode.OneWay);

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='FontFamilyProperty']/Docs/*" />
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='FontSizeProperty']/Docs/*" />
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='FontAttributesProperty']/Docs/*" />
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='LineHeightProperty']/Docs/*" />
		public static readonly BindableProperty LineHeightProperty = LineHeightElement.LineHeightProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontElement.FontAttributesProperty); }
			set { SetValue(FontElement.FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontElement.FontFamilyProperty); }
			set { SetValue(FontElement.FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='TextDecorations']/Docs/*" />
		public TextDecorations TextDecorations
		{
			get { return (TextDecorations)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Span.xml" path="//Member[@MemberName='LineHeight']/Docs/*" />
		public double LineHeight
		{
			get { return (double)GetValue(LineHeightElement.LineHeightProperty); }
			set { SetValue(LineHeightElement.LineHeightProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			this.PropagateBindingContext(GestureRecognizers);
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
