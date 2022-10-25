using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A button <see cref="View" /> that reacts to touch events.
	/// </summary>
	public partial class Button : View, IFontElement, ITextElement, IBorderElement, IButtonController, IElementConfiguration<Button>, IPaddingElement, IImageController, IViewController, IButtonElement, IImageElement
	{
		const double DefaultSpacing = 10;

		/// <summary>
		/// The backing store for the <see cref="Command" /> bindable property.
		/// </summary>
		public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

		/// <summary>
		/// The backing store for the <see cref="CommandParameter" /> bindable property.
		/// </summary>
		public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

		/// <summary>
		/// The backing store for the <see cref="ContentLayout" /> bindable property.
		/// </summary>
		public static readonly BindableProperty ContentLayoutProperty = BindableProperty.Create(
			nameof(ContentLayout), typeof(ButtonContentLayout), typeof(Button), new ButtonContentLayout(ButtonContentLayout.ImagePosition.Left, DefaultSpacing),
			propertyChanged: (bindable, oldVal, newVal) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <summary>
		/// The backing store for the <see cref="Text" /> bindable property.
		/// </summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(
			nameof(Text), typeof(string), typeof(Button), null,
			propertyChanged: (bindable, oldVal, newVal) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <summary>
		/// The backing store for the <see cref="TextColor" /> bindable property.
		/// </summary>
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <summary>
		/// The backing store for the <see cref="CharacterSpacing" /> bindable property.
		/// </summary>
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <summary>
		/// The backing store for the <see cref="FontFamily" /> bindable property.
		/// </summary>
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <summary>
		/// The backing store for the <see cref="FontSize" /> bindable property.
		/// </summary>
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <summary>
		/// The backing store for the <see cref="TextTransform" /> bindable property.
		/// </summary>
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <summary>
		/// The backing store for the <see cref="FontAttributes" /> bindable property.
		/// </summary>
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <summary>
		/// The backing store for the <see cref="FontAutoScalingEnabled" /> bindable property.
		/// </summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <summary>
		/// The backing store for the <see cref="BorderWidth"/> bindable property.
		/// </summary>
		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(Button), -1d);

		/// <summary>
		/// The backing store for the <see cref="BorderColor" /> bindable property.
		/// </summary>
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <summary>
		/// The backing store for the <see cref="CornerRadius"/> bindable property.
		/// </summary>
		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(Button), defaultValue: BorderElement.DefaultCornerRadius);

		/// <summary>
		/// The backing store for the <see cref="ImageSource" /> bindable property.
		/// </summary>
		public static readonly BindableProperty ImageSourceProperty = ImageElement.ImageSourceProperty;

		/// <summary>
		/// The backing store for the <see cref="Padding" /> bindable property.
		/// </summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>
		/// The backing store for the <see cref="LineBreakMode"/> bindable property.
		/// </summary>
		public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create(
			nameof(LineBreakMode), typeof(LineBreakMode), typeof(Button), LineBreakMode.NoWrap,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <summary>
		/// Gets or sets the padding for the button. This is a bindable property.
		/// </summary>
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => new Thickness(double.NaN);

		/// <summary>
		/// Determines how <see cref="Text"/> is shown when the length is overflowing the size of this button.
		/// This is a bindable property.
		/// </summary>
		public LineBreakMode LineBreakMode
		{
			get { return (LineBreakMode)GetValue(LineBreakModeProperty); }
			set { SetValue(LineBreakModeProperty, value); }
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(Button), default(bool));
		
		/// <summary>
		/// The backing store for the <see cref="IsPressed"/> bindable property.
		/// </summary>
		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<Button>> _platformConfigurationRegistry;

		/// <summary>
		/// Gets or sets a color that describes the border stroke color of the button. This is a bindable property.
		/// </summary>
		/// <remarks>This property has no effect if <see cref="IBorderElement.BorderWidth" /> is set to 0. On Android this property will not have an effect unless <see cref="VisualElement.BackgroundColor" /> is set to a non-default color.</remarks>
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the corner radius for the button, in device-independent units. This is a bindable property.
		/// </summary>
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <summary>
		/// Gets or sets the width of the border, in device-independent units. This is a bindable property.
		/// </summary>
		/// <remarks>Set this value to a non-zero value in order to have a visible border.</remarks>
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets an object that controls the position of the button image and the spacing between the button's image and the button's text.
		/// This is a bindable property.
		/// </summary>
		public ButtonContentLayout ContentLayout
		{
			get { return (ButtonContentLayout)GetValue(ContentLayoutProperty); }
			set { SetValue(ContentLayoutProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to invoke when the button is activated. This is a bindable property.
		/// </summary>
		/// <remarks>This property is used to associate a command with an instance of a button. This property is most often set in the MVVM pattern to bind callbacks back into the ViewModel. <see cref="VisualElement.IsEnabled" /> is controlled by the <see cref="Command.CanExecute(object)"/> if set.</remarks>
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/> property.
		/// The default value is <see langword="null"/>. This is a bindable property.
		/// </summary>
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <summary>
		/// Allows you to display a bitmap image on the Button. This is a bindable property.
		/// </summary>
		/// <remarks>For more options have a look at <see cref="ImageButton"/>.</remarks>
		public ImageSource ImageSource
		{
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the text displayed as the content of the button.
		/// The default value is <see langword="null"/>. This is a bindable property.
		/// </summary>
		/// <remarks>Changing the text of a button will trigger a layout cycle.</remarks>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary>
		/// Gets or sets the <see cref="Color" /> for the text of the button. This is a bindable property.
		/// </summary>
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the spacing between each of the characters of <see cref="Text"/> when displayed on the button.
		/// This is a bindable property.
		/// </summary>
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		bool IButtonElement.IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		/// <summary>
		/// Internal method to trigger the <see cref="Clicked"/> event.
		/// Should not be called manually outside of .NET MAUI.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() => ButtonElement.ElementClicked(this, this);

		/// <summary>
		/// Gets whether or not the button is currently pressed.
		/// </summary>
		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.SetIsPressed(bool isPressed) => SetValue(IsPressedPropertyKey, isPressed);

		/// <summary>
		/// Internal method to trigger the <see cref="Pressed"/> event.
		/// Should not be called manually outside of .NET MAUI.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() => ButtonElement.ElementPressed(this, this);

		/// <summary>
		/// Internal method to trigger the <see cref="Released"/> event.
		/// Should not be called manually outside of .NET MAUI.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() => ButtonElement.ElementReleased(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpClicked() => Clicked?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpPressed() => Pressed?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpReleased() => Released?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Gets or sets a value that indicates whether the font for the text of this button is bold, italic, or neither.
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
		/// Applies text transformation to the <see cref="Text"/> displayed on this button.
		/// This is a bindable property.
		/// </summary>
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		/// <summary>
		/// Occurs when the button is clicked/tapped.
		/// </summary>
		public event EventHandler Clicked;

		/// <summary>
		/// Occurs when the button is pressed.
		/// </summary>
		public event EventHandler Pressed;

		/// <summary>
		/// Occurs when the button is released.
		/// </summary>
		public event EventHandler Released;

		/// <summary>
		/// Initializes a new instance of the <see cref="Button"/> class.
		/// </summary>
		public Button()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Button>>(() => new PlatformConfigurationRegistry<Button>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Button> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsPressed)
			{
				VisualStateManager.GoToState(this, ButtonElement.PressedVisualState);
			}
			else
			{
				base.ChangeVisualState();
			}
		}

		protected override void OnBindingContextChanged()
		{
			ImageSource image = ImageSource;
			if (image != null)
				SetInheritedBindingContext(image, BindingContext);

			base.OnBindingContextChanged();
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			HandleFontChanged();

		double IFontElement.FontSizeDefaultValueCreator() =>
			this.GetDefaultFontSize();

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			HandleFontChanged();

		void IFontElement.OnFontAutoScalingEnabledChanged(bool oldValue, bool newValue) =>
			HandleFontChanged();

		void HandleFontChanged()
		{
			Handler?.UpdateValue(nameof(ITextStyle.Font));
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		Aspect IImageElement.Aspect => Aspect.AspectFit;
		ImageSource IImageElement.Source => ImageSource;
		bool IImageElement.IsOpaque => false;


		void IImageElement.RaiseImageSourcePropertyChanged() => OnPropertyChanged(ImageSourceProperty.PropertyName);

		int IBorderElement.CornerRadiusDefaultValue => (int)CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => (double)BorderWidthProperty.DefaultValue;

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextElement.OnCharacterSpacingPropertyChanged(double oldValue, double newValue)
		{
			InvalidateMeasure();
		}


		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		bool IImageController.GetLoadAsAnimation() => false;
		bool IImageElement.IsLoading => false;

		bool IImageElement.IsAnimationPlaying => false;

		void IImageElement.OnImageSourceSourceChanged(object sender, EventArgs e) =>
			ImageElement.ImageSourceSourceChanged(this, e);

		void IButtonElement.OnCommandCanExecuteChanged(object sender, EventArgs e) =>
			ButtonElement.CommandCanExecuteChanged(this, EventArgs.Empty);

		void IImageController.SetIsLoading(bool isLoading)
		{
		}

		bool IBorderElement.IsCornerRadiusSet() => IsSet(CornerRadiusProperty);
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);
		bool IBorderElement.IsBorderColorSet() => IsSet(BorderColorProperty);
		bool IBorderElement.IsBorderWidthSet() => IsSet(BorderWidthProperty);

		void ITextElement.OnTextTransformChanged(TextTransform oldValue, TextTransform newValue)
			=> InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		/// <summary>
		/// Applies the <see cref="TextTransform"/> to <see cref="Text"/>.
		/// </summary>
		/// <remarks>For internal use by the .NET MAUI platform mostly.</remarks>
		/// <param name="source">The text to transform.</param>
		/// <param name="textTransform">The transform to apply to <paramref name="source"/>.</param>
		/// <returns>The transformed text.</returns>
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		/// <summary>
		/// Represents the layout of the button content whenever an image is shown.
		/// </summary>
		[DebuggerDisplay("Image Position = {Position}, Spacing = {Spacing}")]
		[System.ComponentModel.TypeConverter(typeof(ButtonContentTypeConverter))]
		public sealed class ButtonContentLayout
		{
			/// <summary>
			/// Enumerates values that determine the position of the image on the button.
			/// </summary>
			public enum ImagePosition
			{
				Left,
				Top,
				Right,
				Bottom
			}

			/// <summary>
			/// Initializes a new instance of the this class.
			/// </summary>
			/// <param name="position">The position of the image.</param>
			/// <param name="spacing">The spacing for the button content.</param>
			public ButtonContentLayout(ImagePosition position, double spacing)
			{
				Position = position;
				Spacing = spacing;
			}

			/// <summary>
			/// Gets the position of the image on the button.
			/// </summary>
			public ImagePosition Position { get; }

			/// <summary>
			/// Gets the spacing for the button content.
			/// </summary>
			public double Spacing { get; }

			/// <summary>
			/// Gets the string representation of this object.
			/// </summary>
			/// <returns>Prints out the values of <see cref="Position"/> and <see cref="Spacing"/>.</returns>
			public override string ToString() => $"Image Position = {Position}, Spacing = {Spacing}";
		}

		/// <summary>
		/// A converter to convert a string to a <see cref="ButtonContentLayout"/> object.
		/// </summary>
		public sealed class ButtonContentTypeConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				=> sourceType == typeof(string);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				=> false;

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				var strValue = value?.ToString();
				if (strValue == null)
					throw new InvalidOperationException($"Cannot convert null into {typeof(ButtonContentLayout)}");

				string[] parts = strValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length != 1 && parts.Length != 2)
					throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(ButtonContentLayout)}");

				double spacing = DefaultSpacing;
				var position = ButtonContentLayout.ImagePosition.Left;

				var spacingFirst = char.IsDigit(parts[0][0]);

				int positionIndex = spacingFirst ? (parts.Length == 2 ? 1 : -1) : 0;
				int spacingIndex = spacingFirst ? 0 : (parts.Length == 2 ? 1 : -1);

				if (spacingIndex > -1)
					spacing = double.Parse(parts[spacingIndex]);

				if (positionIndex > -1)
					position = (ButtonContentLayout.ImagePosition)Enum.Parse(typeof(ButtonContentLayout.ImagePosition), parts[positionIndex], true);

				return new ButtonContentLayout(position, spacing);
			}

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
				=> throw new NotSupportedException();
		}
	}
}
