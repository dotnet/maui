using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ButtonRenderer))]
	public class Button : View, IFontElement, ITextElement, IBorderElement, IButtonController, IElementConfiguration<Button>, IPaddingElement, IImageController, IViewController, IButtonElement, IImageElement
	{
		const int DefaultBorderRadius = 5;
		const double DefaultSpacing = 10;

		public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

		public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

		public static readonly BindableProperty ContentLayoutProperty =
			BindableProperty.Create("ContentLayout", typeof(ButtonContentLayout), typeof(Button), new ButtonContentLayout(ButtonContentLayout.ImagePosition.Left, DefaultSpacing));

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Button), null,
			propertyChanged: (bindable, oldVal, newVal) => ((Button)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty FontProperty = FontElement.FontProperty;

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create("BorderWidth", typeof(double), typeof(Button), -1d);

		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		[Obsolete("BorderRadiusProperty is obsolete as of 2.5.0. Please use CornerRadius instead.")]
		public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(int), typeof(Button), defaultValue: DefaultBorderRadius,
			propertyChanged: BorderRadiusPropertyChanged);

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create("CornerRadius", typeof(int), typeof(Button), defaultValue: BorderElement.DefaultCornerRadius,
			propertyChanged: CornerRadiusPropertyChanged);

		public static readonly BindableProperty ImageProperty = ImageElement.FileImageProperty;


		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return default(Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}


		internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(Button), default(bool));
		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;


		readonly Lazy<PlatformConfigurationRegistry<Button>> _platformConfigurationRegistry;

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		[Obsolete("BorderRadius is obsolete as of 2.5.0. Please use CornerRadius instead.")]
		public int BorderRadius
		{
			get { return (int)GetValue(BorderRadiusProperty); }
			set { SetValue(BorderRadiusProperty, value); }
		}

		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public ButtonContentLayout ContentLayout
		{
			get { return (ButtonContentLayout)GetValue(ContentLayoutProperty); }
			set { SetValue(ContentLayoutProperty, value); }
		}

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public Font Font
		{
			get { return (Font)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		public FileImageSource Image
		{
			get { return (FileImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextElement.TextColorProperty); }
			set { SetValue(TextElement.TextColorProperty, value); }
		}

		bool IButtonElement.IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() => ButtonElement.ElementClicked(this, this);

		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.SetIsPressed(bool isPressed) => SetValue(IsPressedPropertyKey, isPressed);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() => ButtonElement.ElementPressed(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() => ButtonElement.ElementReleased(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpClicked() => Clicked?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpPressed() => Pressed?.Invoke(this, EventArgs.Empty);

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IButtonElement.PropagateUpReleased() => Released?.Invoke(this, EventArgs.Empty);

		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)GetValue(FontAttributesProperty); }
			set { SetValue(FontAttributesProperty, value); }
		}

		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		[TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public event EventHandler Clicked;
		public event EventHandler Pressed;

		public event EventHandler Released;

		public Button()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Button>>(() => new PlatformConfigurationRegistry<Button>(this));
		}

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
			FileImageSource image = Image;
			if (image != null)
				SetInheritedBindingContext(image, BindingContext);

			base.OnBindingContextChanged();
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Button)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontChanged(Font oldValue, Font newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		Aspect IImageElement.Aspect => Aspect.AspectFit;
		ImageSource IImageElement.Source => Image;
		bool IImageElement.IsOpaque => false;


		void IImageElement.RaiseImageSourcePropertyChanged() => OnPropertyChanged(ImageProperty.PropertyName);

		int IBorderElement.CornerRadiusDefaultValue => (int)CornerRadiusProperty.DefaultValue;

		Color IBorderElement.BorderColorDefaultValue => (Color)BorderColorProperty.DefaultValue;

		double IBorderElement.BorderWidthDefaultValue => (double)BorderWidthProperty.DefaultValue;

		/// <summary>
		/// Flag to prevent overwriting the value of CornerRadius
		/// </summary>
		bool cornerOrBorderRadiusSetting = false;

		static void BorderRadiusPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue == oldvalue)
				return;

			var button = (Button)bindable;
			var val = (int)newvalue;
			if (val == DefaultBorderRadius && !button.cornerOrBorderRadiusSetting)
				val = BorderElement.DefaultCornerRadius;

			var oldVal = (int)bindable.GetValue(Button.CornerRadiusProperty);

			if (oldVal == val)
				return;

			button.cornerOrBorderRadiusSetting = true;
			bindable.SetValue(Button.CornerRadiusProperty, val);
			button.cornerOrBorderRadiusSetting = false;
		}

		static void CornerRadiusPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue == oldvalue)
				return;

			var button = (Button)bindable;
			var val = (int)newvalue;
			if (val == BorderElement.DefaultCornerRadius && !button.cornerOrBorderRadiusSetting)
				val = DefaultBorderRadius;

#pragma warning disable 0618 // retain until BorderRadiusProperty removed
			var oldVal = (int)bindable.GetValue(Button.BorderRadiusProperty);
#pragma warning restore

			if (oldVal == val)
				return;

#pragma warning disable 0618 // retain until BorderRadiusProperty removed
			button.cornerOrBorderRadiusSetting = true;
			bindable.SetValue(Button.BorderRadiusProperty, val);
			button.cornerOrBorderRadiusSetting = false;
#pragma warning restore
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void IImageElement.OnImageSourcesSourceChanged(object sender, EventArgs e) =>
			ImageElement.ImageSourcesSourceChanged(this, e);

		void IButtonElement.OnCommandCanExecuteChanged(object sender, EventArgs e) =>
			ButtonElement.CommandCanExecuteChanged(this, EventArgs.Empty);

		void IImageController.SetIsLoading(bool isLoading)
		{
		}

		bool IBorderElement.IsCornerRadiusSet() => IsSet(CornerRadiusProperty);
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBorderColorSet() => IsSet(BorderColorProperty);
		bool IBorderElement.IsBorderWidthSet() => IsSet(BorderWidthProperty);

		[DebuggerDisplay("Image Position = {Position}, Spacing = {Spacing}")]
		[TypeConverter(typeof(ButtonContentTypeConverter))]
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

			public override string ToString()
			{
				return $"Image Position = {Position}, Spacing = {Spacing}";
			}
		}

		[Xaml.TypeConversion(typeof(ButtonContentLayout))]
		public sealed class ButtonContentTypeConverter : TypeConverter
		{
			public override object ConvertFromInvariantString(string value)
			{
				if (value == null)
				{
					throw new InvalidOperationException($"Cannot convert null into {typeof(ButtonContentLayout)}");
				}

				string[] parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length != 1 && parts.Length != 2)
				{
					throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(ButtonContentLayout)}");
				}

				double spacing = DefaultSpacing;
				var position = ButtonContentLayout.ImagePosition.Left;

				var spacingFirst = char.IsDigit(parts[0][0]);

				int positionIndex = spacingFirst ? (parts.Length == 2 ? 1 : -1) : 0;
				int spacingIndex = spacingFirst ? 0 : (parts.Length == 2 ? 1 : -1);

				if (spacingIndex > -1)
				{
					spacing = double.Parse(parts[spacingIndex]);
				}

				if (positionIndex > -1)
				{
					position =
						(ButtonContentLayout.ImagePosition)Enum.Parse(typeof(ButtonContentLayout.ImagePosition), parts[positionIndex], true);
				}

				return new ButtonContentLayout(position, spacing);
			}
		}
	}
}
