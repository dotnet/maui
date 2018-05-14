using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ButtonRenderer))]
	public class Button : View, IFontElement, ITextElement, IBorderElement, IButtonController, IElementConfiguration<Button>, IPaddingElement
	{
		const double DefaultSpacing = 10;
		const int DefaultBorderRadius = 5;
		const int DefaultCornerRadius = -1;

		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(Button), null, propertyChanged: (bo, o, n) => ((Button)bo).OnCommandChanged());

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(Button), null,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Button)bindable).CommandCanExecuteChanged(bindable, EventArgs.Empty));

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

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create("CornerRadius", typeof(int), typeof(Button), defaultValue: DefaultCornerRadius,
			propertyChanged: CornerRadiusPropertyChanged);

		public static readonly BindableProperty ImageProperty = BindableProperty.Create("Image", typeof(FileImageSource), typeof(Button), default(FileImageSource),
			propertyChanging: (bindable, oldvalue, newvalue) => ((Button)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue),
			propertyChanged: (bindable, oldvalue, newvalue) => ((Button)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue));

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public Thickness Padding
		{
			get { return (Thickness)GetValue (PaddingElement.PaddingProperty); }
			set { SetValue (PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator ()
		{
			return default (Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged (Thickness oldValue, Thickness newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

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

		bool IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked()
		{
			if (IsEnabled == true)
			{
				Command?.Execute(CommandParameter);
				Clicked?.Invoke(this, EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed()
		{
			if (IsEnabled == true)
			{
				Pressed?.Invoke(this, EventArgs.Empty); 
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased()
		{
			if (IsEnabled == true)
			{
				Released?.Invoke(this, EventArgs.Empty);
			}
		}

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

		protected override void OnBindingContextChanged()
		{
			FileImageSource image = Image;
			if (image != null)
				SetInheritedBindingContext(image, BindingContext);

			base.OnBindingContextChanged();
		}

		protected override void OnPropertyChanging(string propertyName = null)
		{
			if (propertyName == CommandProperty.PropertyName)
			{
				ICommand cmd = Command;
				if (cmd != null)
					cmd.CanExecuteChanged -= CommandCanExecuteChanged;
			}

			base.OnPropertyChanging(propertyName);
		}

		void CommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			ICommand cmd = Command;
			if (cmd != null)
				IsEnabledCore = cmd.CanExecute(CommandParameter);
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

		void OnCommandChanged()
		{
			if (Command != null)
			{
				Command.CanExecuteChanged += CommandCanExecuteChanged;
				CommandCanExecuteChanged(this, EventArgs.Empty);
			}
			else
				IsEnabledCore = true;
		}

		void OnSourceChanged(object sender, EventArgs eventArgs)
		{
			OnPropertyChanged(ImageProperty.PropertyName);
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void OnSourcePropertyChanged(ImageSource oldvalue, ImageSource newvalue)
		{
			if (newvalue != null)
			{
				newvalue.SourceChanged += OnSourceChanged;
				SetInheritedBindingContext(newvalue, BindingContext);
			}
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void OnSourcePropertyChanging(ImageSource oldvalue, ImageSource newvalue)
		{
			if (oldvalue != null)
				oldvalue.SourceChanged -= OnSourceChanged;
		}

		static void BorderRadiusPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue == oldvalue)
				return;

			var val = (int)newvalue;
			if (val == DefaultBorderRadius)
				val = DefaultCornerRadius;

			var oldVal = (int)bindable.GetValue(Button.CornerRadiusProperty);

			if (oldVal == val)
				return;

			bindable.SetValue(Button.CornerRadiusProperty, val);
		}

		static void CornerRadiusPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue == oldvalue)
				return;

			var val = (int)newvalue;
			if (val == DefaultCornerRadius)
				val = DefaultBorderRadius;

#pragma warning disable 0618 // retain until BorderRadiusProperty removed
			var oldVal = (int)bindable.GetValue(Button.BorderRadiusProperty);
#pragma warning restore

			if (oldVal == val)
				return;

#pragma warning disable 0618 // retain until BorderRadiusProperty removed
			bindable.SetValue(Button.BorderRadiusProperty, val);
#pragma warning restore
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

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
