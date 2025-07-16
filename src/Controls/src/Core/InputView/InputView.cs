#nullable disable
using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Internals;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="Type[@FullName='Microsoft.Maui.Controls.InputView']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class InputView : View, IPlaceholderElement, ITextElement, ITextInput, IFontElement
	{
		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(InputView), defaultBindingMode: BindingMode.TwoWay,
			propertyChanged: (bindable, oldValue, newValue) => ((InputView)bindable).OnTextChanged((string)oldValue, (string)newValue));

		/// <summary>Bindable property for <see cref="Keyboard"/>.</summary>
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(InputView), Keyboard.Default,
			coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);

		/// <summary>Bindable property for <see cref="IsSpellCheckEnabled"/>.</summary>
		public static readonly BindableProperty IsSpellCheckEnabledProperty = BindableProperty.Create(nameof(IsSpellCheckEnabled), typeof(bool), typeof(InputView), true);

		/// <summary>Bindable property for <see cref="IsTextPredictionEnabled"/>.</summary>
		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(InputView), true);

		/// <summary>Bindable property for <see cref="MaxLength"/>.</summary>
		public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(InputView), int.MaxValue);

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

		/// <summary>Bindable property for <see cref="CursorPosition"/>.</summary>
		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(InputView), 0, validateValue: (b, v) => (int)v >= 0);

		/// <summary>Bindable property for <see cref="SelectionLength"/>.</summary>
		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(InputView), 0, validateValue: (b, v) => (int)v >= 0);

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/InputView.xml" path="//Member[@MemberName='MaxLength']/Docs/*" />
		public int MaxLength
		{
			get => (int)GetValue(MaxLengthProperty);
			set => SetValue(MaxLengthProperty, value);
		}

		internal InputView()
		{
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (Application.Current is null)
				return;

			if (args.NewHandler is null || args.OldHandler is not null)
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;

			if (args.NewHandler is not null && args.OldHandler is null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
		}

		private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			OnPropertyChanged(nameof(PlaceholderColor));
			OnPropertyChanged(nameof(TextColor));
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

		/// <summary>Gets or sets a value that controls whether spell checking is enabled.</summary>
		/// <value><see langword = "true" /> if spell checking is enabled. Otherwise <see langword="false" />.</value>
		/// <remarks>On Windows, spellchecking also turns on auto correction</remarks>
		public bool IsSpellCheckEnabled
		{
			get => (bool)GetValue(IsSpellCheckEnabledProperty);
			set => SetValue(IsSpellCheckEnabledProperty, value);
		}

		/// <summary>Gets or sets a value that controls whether text prediction and automatic text correction are enabled.</summary>
		/// <value><see langword="true" /> if text prediction (auto correction) is enabled. Otherwise <see langword="false" />.</value>
		/// <remarks>On Windows, text prediction only affects touch keyboards and only affects keyboard word suggestions.</remarks>
		public bool IsTextPredictionEnabled
		{
			get => (bool)GetValue(IsTextPredictionEnabledProperty);
			set => SetValue(IsTextPredictionEnabledProperty, value);
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

		/// <summary>
		/// Gets or sets the position of the cursor. The value must be more than or equal to 0 and less or equal to the length of <see cref="InputView.Text"/>.
		/// This is a bindable property.
		/// </summary>
		public int CursorPosition
		{
			get { return (int)GetValue(CursorPositionProperty); }
			set { SetValue(CursorPositionProperty, value); }
		}

		/// <summary>
		/// Gets or sets the length of the selection. The selection will start at <see cref="CursorPosition"/>.
		/// This is a bindable property.
		/// </summary>
		public int SelectionLength
		{
			get { return (int)GetValue(SelectionLengthProperty); }
			set { SetValue(SelectionLengthProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the font for the text of this entry is bold, italic, or neither.
		/// This is a bindable property.
		/// </summary>
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <summary>
		/// Gets or sets the font family for the text of this entry. This is a bindable property.
		/// </summary>
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the size of the font for the text of this entry. This is a bindable property.
		/// </summary>
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <summary>
		/// Determines whether or not the font of this entry should scale automatically according to the operating system settings. Default value is <see langword="true"/>.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>Typically this should always be enabled for accessibility reasons.</remarks>
		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
				this.GetDefaultFontSize();

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		Font ITextStyle.Font => this.ToFont();

		int ITextInput.SelectionLength
		{
			get => SelectionLength;
			set => SetValue(SelectionLengthProperty, value, SetterSpecificity.FromHandler);
		}

		int ITextInput.CursorPosition
		{
			get => CursorPosition;
			set => SetValue(CursorPositionProperty, value, SetterSpecificity.FromHandler);
		}

		string ITextInput.Text
		{
			get => Text;
			set => SetValue(TextProperty, value, SetterSpecificity.FromHandler);
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Text), Text);
			return $"{base.GetDebuggerDisplay()}, Text = {debugText}";
		}
	}
}