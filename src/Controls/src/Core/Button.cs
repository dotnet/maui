using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="Type[@FullName='Microsoft.Maui.Controls.Button']/Docs/*" />
	public partial class Button : View, IFontElement, ITextElement, IBorderElement, IButtonController, IElementConfiguration<Button>, IPaddingElement, IImageController, IViewController, IButtonElement, IImageElement
	{
		const double DefaultSpacing = 10;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CommandProperty']/Docs/*" />
		public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CommandParameterProperty']/Docs/*" />
		public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='ContentLayoutProperty']/Docs/*" />
		public static readonly BindableProperty ContentLayoutProperty = BindableProperty.Create(
			nameof(ContentLayout), typeof(ButtonContentLayout), typeof(Button), new ButtonContentLayout(ButtonContentLayout.ImagePosition.Left, DefaultSpacing),
			propertyChanged: (bindable, oldVal, newVal) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='TextProperty']/Docs/*" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create(
			nameof(Text), typeof(string), typeof(Button), null,
			propertyChanged: (bindable, oldVal, newVal) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='TextColorProperty']/Docs/*" />
		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CharacterSpacingProperty']/Docs/*" />
		public static readonly BindableProperty CharacterSpacingProperty = TextElement.CharacterSpacingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontFamilyProperty']/Docs/*" />
		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontSizeProperty']/Docs/*" />
		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='TextTransformProperty']/Docs/*" />
		public static readonly BindableProperty TextTransformProperty = TextElement.TextTransformProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontAttributesProperty']/Docs/*" />
		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontAutoScalingEnabledProperty']/Docs/*" />
		public static readonly BindableProperty FontAutoScalingEnabledProperty = FontElement.FontAutoScalingEnabledProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='BorderWidthProperty']/Docs/*" />
		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(Button), -1d);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='BorderColorProperty']/Docs/*" />
		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CornerRadiusProperty']/Docs/*" />
		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(Button), defaultValue: BorderElement.DefaultCornerRadius);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='ImageSourceProperty']/Docs/*" />
		public static readonly BindableProperty ImageSourceProperty = ImageElement.ImageSourceProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='PaddingProperty']/Docs/*" />
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='LineBreakModeProperty']/Docs/*" />
		public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create(
			nameof(LineBreakMode), typeof(LineBreakMode), typeof(Button), LineBreakMode.NoWrap,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='Padding']/Docs/*" />
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => new Thickness(double.NaN);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='LineBreakMode']/Docs/*" />
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
		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='IsPressedProperty']/Docs/*" />
		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;


		readonly Lazy<PlatformConfigurationRegistry<Button>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='BorderColor']/Docs/*" />
		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='BorderWidth']/Docs/*" />
		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='ContentLayout']/Docs/*" />
		public ButtonContentLayout ContentLayout
		{
			get { return (ButtonContentLayout)GetValue(ContentLayoutProperty); }
			set { SetValue(ContentLayoutProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='Command']/Docs/*" />
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CommandParameter']/Docs/*" />
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='ImageSource']/Docs/*" />
		public ImageSource ImageSource
		{
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='CharacterSpacing']/Docs/*" />
		public double CharacterSpacing
		{
			get { return (double)GetValue(TextElement.CharacterSpacingProperty); }
			set { SetValue(TextElement.CharacterSpacingProperty, value); }
		}

		bool IButtonElement.IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='SendClicked']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() => ButtonElement.ElementClicked(this, this);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='IsPressed']/Docs/*" />
		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.SetIsPressed(bool isPressed) => SetValue(IsPressedPropertyKey, isPressed);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='SendPressed']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() => ButtonElement.ElementPressed(this, this);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='SendReleased']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() => ButtonElement.ElementReleased(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpClicked() => Clicked?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpPressed() => Pressed?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpReleased() => Released?.Invoke(this, EventArgs.Empty);

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontAttributes']/Docs/*" />
		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontSize']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='FontAutoScalingEnabled']/Docs/*" />
		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='TextTransform']/Docs/*" />
		public TextTransform TextTransform
		{
			get => (TextTransform)GetValue(TextTransformProperty);
			set => SetValue(TextTransformProperty, value);
		}

		public event EventHandler Clicked;
		public event EventHandler Pressed;

		public event EventHandler Released;

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Button.xml" path="//Member[@MemberName='UpdateFormsText']/Docs/*" />
		public virtual string UpdateFormsText(string source, TextTransform textTransform)
			=> TextTransformUtilites.GetTransformedText(source, textTransform);

		[DebuggerDisplay("Image Position = {Position}, Spacing = {Spacing}")]
		[System.ComponentModel.TypeConverter(typeof(ButtonContentTypeConverter))]
		public sealed class ButtonContentLayout
		{
			public enum ImagePosition
			{
				Left,
				Top,
				Right,
				Bottom
			}

			public ButtonContentLayout(ImagePosition position, double spacing)
			{
				Position = position;
				Spacing = spacing;
			}

			public ImagePosition Position { get; }

			public double Spacing { get; }

			public override string ToString() => $"Image Position = {Position}, Spacing = {Spacing}";
		}

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
