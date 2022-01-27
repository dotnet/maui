using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="Type[@FullName='Microsoft.Maui.Controls.InputView']/Docs" />
	public class InputView : View, IPlaceholderElement, ITextElement
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextProperty']/Docs" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(InputView), defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (bindable, oldValue, newValue) => ((InputView)bindable).OnTextChanged((string)oldValue, (string)newValue));

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='KeyboardProperty']/Docs" />
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(InputView), Keyboard.Default,
			coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='IsSpellCheckEnabledProperty']/Docs" />
		public static readonly BindableProperty IsSpellCheckEnabledProperty = BindableProperty.Create(nameof(IsSpellCheckEnabled), typeof(bool), typeof(InputView), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='MaxLengthProperty']/Docs" />
		public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(int), int.MaxValue);

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='IsReadOnlyProperty']/Docs" />
		public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(InputView), false);

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='PlaceholderProperty']/Docs" />
		public static readonly BindableProperty PlaceholderProperty = PlaceholderElement.PlaceholderProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='PlaceholderColorProperty']/Docs" />
		public static readonly BindableProperty PlaceholderColorProperty = PlaceholderElement.PlaceholderColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextColorProperty']/Docs" />
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs" />
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextTransformProperty']/Docs" />
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='MaxLength']/Docs" />
		public int MaxLength
		{
			get => (int)GetValue(MaxLengthProperty);
			set => SetValue(MaxLengthProperty, value);
		}

		internal InputView()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='Keyboard']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(Converters.KeyboardTypeConverter))]
		public Keyboard Keyboard
		{
			get => (Keyboard)GetValue(KeyboardProperty);
			set => SetValue(KeyboardProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='IsSpellCheckEnabled']/Docs" />
		public bool IsSpellCheckEnabled
		{
			get => (bool)GetValue(IsSpellCheckEnabledProperty);
			set => SetValue(IsSpellCheckEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='IsReadOnly']/Docs" />
		public bool IsReadOnly
		{
			get => (bool)GetValue(IsReadOnlyProperty);
			set => SetValue(IsReadOnlyProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='Placeholder']/Docs" />
		public string Placeholder
		{
			get => (string)GetValue(PlaceholderProperty);
			set => SetValue(PlaceholderProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='PlaceholderColor']/Docs" />
		public Color PlaceholderColor
		{
			get => (Color)GetValue(PlaceholderColorProperty);
			set => SetValue(PlaceholderColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextColor']/Docs" />
		public Color TextColor
		{
			get => (Color)GetValue(TextColorProperty);
			set => SetValue(TextColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='CharacterSpacing']/Docs" />
		public double CharacterSpacing
		{
			get => (double)GetValue(CharacterSpacingProperty);
			set => SetValue(CharacterSpacingProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='TextTransform']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='OnTextTransformChanged']/Docs" />
		public void OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='UpdateFormsText']/Docs" />
		public string UpdateFormsText(string original, TextTransform transform)
		{
			return TextTransformUtilites.GetTransformedText(original, transform);
		}
	}
}