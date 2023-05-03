#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="Type[@FullName='Microsoft.Maui.Controls.InputView']/Docs/*" />
	public class InputView : View, IPlaceholderElement, ITextElement
	{
		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(InputView), defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (bindable, oldValue, newValue) => ((InputView)bindable).OnTextChanged((string)oldValue, (string)newValue));

		/// <summary>Bindable property for <see cref="Keyboard"/>.</summary>
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(InputView), Keyboard.Default,
			coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);

		/// <summary>Bindable property for <see cref="IsSpellCheckEnabled"/>.</summary>
		public static readonly BindableProperty IsSpellCheckEnabledProperty = BindableProperty.Create(nameof(IsSpellCheckEnabled), typeof(bool), typeof(InputView), true);

		/// <summary>Bindable property for <see cref="MaxLength"/>.</summary>
		public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(int), int.MaxValue);

		/// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
		public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(InputView), false);

		/// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
		public static readonly BindableProperty PlaceholderProperty = PlaceholderElement.PlaceholderProperty;

		/// <summary>Bindable property for <see cref="PlaceholderColor"/>.</summary>
		public static readonly BindableProperty PlaceholderColorProperty = PlaceholderElement.PlaceholderColorProperty;

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>Bindable property for <see cref="CharacterSpacing"/>.</summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>Bindable property for <see cref="TextTransform"/>.</summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='MaxLength']/Docs/*" />
		public int MaxLength
		{
			get => (int)GetValue(MaxLengthProperty);
			set => SetValue(MaxLengthProperty, value);
		}

		internal InputView()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='Keyboard']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(Converters.KeyboardTypeConverter))]
		public Keyboard Keyboard
		{
			get => (Keyboard)GetValue(KeyboardProperty);
			set => SetValue(KeyboardProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='IsSpellCheckEnabled']/Docs/*" />
		public bool IsSpellCheckEnabled
		{
			get => (bool)GetValue(IsSpellCheckEnabledProperty);
			set => SetValue(IsSpellCheckEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='IsReadOnly']/Docs/*" />
		public bool IsReadOnly
		{
			get => (bool)GetValue(IsReadOnlyProperty);
			set => SetValue(IsReadOnlyProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='Placeholder']/Docs/*" />
		public string Placeholder
		{
			get => (string)GetValue(PlaceholderProperty);
			set => SetValue(PlaceholderProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='PlaceholderColor']/Docs/*" />
		public Color PlaceholderColor
		{
			get => (Color)GetValue(PlaceholderColorProperty);
			set => SetValue(PlaceholderColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get => (Color)GetValue(TextColorProperty);
			set => SetValue(TextColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get => (double)GetValue(CharacterSpacingProperty);
			set => SetValue(CharacterSpacingProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		public event EventHandler<TextChangedEventArgs> TextChanged;

		protected virtual void OnTextChanged(string oldValue, string newValue)
		{
			TextChanged?.Invoke(this, new TextChangedEventArgs(oldValue, newValue));
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
			InvalidateMeasure();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='OnTextTransformChanged']/Docs/*" />
		public void OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public string UpdateFormsText(string original, TextTransform transform)
		{
			return TextTransformUtilites.GetTransformedText(original, transform);
		}
	}
}