using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Entry is a single line text entry. It is best used for collecting small discrete pieces of information, like usernames and passwords.
	/// </summary>
	public partial class Entry : InputView, IFontElement, ITextAlignmentElement, IEntryController, IElementConfiguration<Entry>
	{
		/// <summary>
		/// Backing store for the <see cref="ReturnType"/> property.
		/// </summary>
		public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(Entry), ReturnType.Default);

		/// <summary>
		/// Backing store for the <see cref="ReturnCommand"/> property.
		/// </summary>
		public static readonly BindableProperty ReturnCommandProperty = BindableProperty.Create(nameof(ReturnCommand), typeof(ICommand), typeof(Entry), default(ICommand));

		/// <summary>
		/// Backing store for the <see cref="ReturnCommandParameter"/> property.
		/// </summary>
		public static readonly BindableProperty ReturnCommandParameterProperty = BindableProperty.Create(nameof(ReturnCommandParameter), typeof(object), typeof(Entry), default(object));

		/// <inheritdoc cref="InputView.PlaceholderProperty"/>
		public new static readonly BindableProperty PlaceholderProperty = InputView.PlaceholderProperty;

		/// <inheritdoc cref="InputView.PlaceholderColorProperty"/>
		public new static readonly BindableProperty PlaceholderColorProperty = InputView.PlaceholderColorProperty;

		/// <summary>
		/// Backing store for the <see cref="IsPassword"/> property.
		/// </summary>
		public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(Entry), default(bool));

		/// <inheritdoc cref="InputView.TextProperty"/>
		public new static readonly BindableProperty TextProperty = InputView.TextProperty;

		/// <inheritdoc cref="InputView.TextColorProperty"/>
		public new static readonly BindableProperty TextColorProperty = InputView.TextColorProperty;

		/// <inheritdoc cref="InputView.KeyboardProperty"/>
		public new static readonly BindableProperty KeyboardProperty = InputView.KeyboardProperty;

		/// <inheritdoc cref="InputView.CharacterSpacingProperty"/>
		public new static readonly BindableProperty CharacterSpacingProperty = InputView.CharacterSpacingProperty;

		/// <summary>
		/// Backing store for the <see cref="HorizontalTextAlignment"/> property.
		/// </summary>
		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		/// <summary>
		/// Backing store for the <see cref="ReturnType"/> property.
		/// </summary>
		public static readonly BindableProperty VerticalTextAlignmentProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

		/// <summary>
		/// Backing store for the <see cref="ReturnType"/> property.
		/// </summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>
		/// Backing store for the <see cref="ReturnType"/> property.
		/// </summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>
		/// Backing store for the <see cref="ReturnType"/> property.
		/// </summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>
		/// Backing store for the <see cref="ReturnType"/> property.
		/// </summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>
		/// Backing store for the <see cref="IsTextPredictionEnabled"/> property.
		/// </summary>
		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Entry), true, BindingMode.OneTime);

		/// <summary>
		/// Backing store for the <see cref="CursorPosition"/> property.
		/// </summary>
		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		/// <summary>
		/// Backing store for the <see cref="SelectionLength"/> property.
		/// </summary>
		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		/// <summary>
		/// Backing store for the <see cref="SelectionLength"/> property.
		/// </summary>
		public static readonly BindableProperty ClearButtonVisibilityProperty = BindableProperty.Create(nameof(SelectionLength), typeof(ClearButtonVisibility), typeof(Entry), ClearButtonVisibility.Never);

		readonly Lazy<PlatformConfigurationRegistry<Entry>> _platformConfigurationRegistry;

		/// <summary>
		/// Creates a new <see cref="Entry"/> object with default values.
		/// </summary>
		public Entry()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Entry>>(() => new PlatformConfigurationRegistry<Entry>(this));
		}

		/// <summary>
		/// Gets or sets the horizontal text alignment. This is a bindable property.
		/// </summary>
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the vertical text alignment. This is a bindable property.
		/// </summary>
		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.VerticalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.VerticalTextAlignmentProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value that indicates if the entry should visually obscure typed text.
		/// Value is <see langword="true" /> if the element is a password box; otherwise, <see langword="false" />. Default value is <see langword="false" />.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>Toggling this value does not reset the contents of the entry, therefore it is advisable to be careful about setting <see cref="IsPassword"/> to false, as it may contain sensitive information.</remarks>
		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
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

		/// <summary>
		/// Determines whether text prediction and automatic text correction is enabled. Default value is <see langword="true"/>.
		/// </summary>
		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
		}

		/// <summary>
		/// Determines what the return key on the on-screen keyboard should look like. This is a bindable property.
		/// </summary>
		public ReturnType ReturnType
		{
			get => (ReturnType)GetValue(ReturnTypeProperty);
			set => SetValue(ReturnTypeProperty, value);
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
		/// Gets or sets the command to run when the user presses the return key, either physically or on the on-screen keyboard.
		/// This is a bindable property.
		/// </summary>
		public ICommand ReturnCommand
		{
			get => (ICommand)GetValue(ReturnCommandProperty);
			set => SetValue(ReturnCommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter object for the <see cref="ReturnCommand" /> that can be used to provide extra information.
		/// This is a bindable property.
		/// </summary>
		public object ReturnCommandParameter
		{
			get => GetValue(ReturnCommandParameterProperty);
			set => SetValue(ReturnCommandParameterProperty, value);
		}

		/// <summary>
		/// Determines the behavior of the clear text button on this entry. This is a bindable property.
		/// </summary>
		public ClearButtonVisibility ClearButtonVisibility
		{
			get => (ClearButtonVisibility)GetValue(ClearButtonVisibilityProperty);
			set => SetValue(ClearButtonVisibilityProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		/// <summary>
		/// Occurs when the user finalizes the text in an entry with the return key.
		/// </summary>
		public event EventHandler Completed;

		/// <summary>
		/// Internal method to trigger <see cref="Completed"/> and <see cref="ReturnCommand"/>.
		/// Should not be called manually outside of .NET MAUI.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
		{
			if (IsEnabled)
			{
				Completed?.Invoke(this, EventArgs.Empty);

				if (ReturnCommand != null && ReturnCommand.CanExecute(ReturnCommandParameter))
				{
					ReturnCommand.Execute(ReturnCommandParameter);
				}
			}
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Entry> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}
	}
}
