using System;
using System.Windows.Input;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ButtonRenderer))]
	public class Button : View, IFontElement, IButtonController
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(Button), null, propertyChanged: (bo, o, n) => ((Button)bo).OnCommandChanged());

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(Button), null,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Button)bindable).CommandCanExecuteChanged(bindable, EventArgs.Empty));

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Button), null,
			propertyChanged: (bindable, oldVal, newVal) => ((Button)bindable).InvalidateMeasure(InvalidationTrigger.MeasureChanged));

		public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(Button), Color.Default);

		public static readonly BindableProperty FontProperty = BindableProperty.Create("Font", typeof(Font), typeof(Button), default(Font), propertyChanged: FontStructPropertyChanged);

		public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create("FontFamily", typeof(string), typeof(Button), default(string), propertyChanged: SpecificFontPropertyChanged);

		public static readonly BindableProperty FontSizeProperty = BindableProperty.Create("FontSize", typeof(double), typeof(Button), -1.0, propertyChanged: SpecificFontPropertyChanged,
			defaultValueCreator: bindable => Device.GetNamedSize(NamedSize.Default, (Button)bindable));

		public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(Button), FontAttributes.None,
			propertyChanged: SpecificFontPropertyChanged);

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create("BorderWidth", typeof(double), typeof(Button), 0d);

		public static readonly BindableProperty BorderColorProperty = BindableProperty.Create("BorderColor", typeof(Color), typeof(Button), Color.Default);

		public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(int), typeof(Button), 5);

		public static readonly BindableProperty ImageProperty = BindableProperty.Create("Image", typeof(FileImageSource), typeof(Button), default(FileImageSource),
			propertyChanging: (bindable, oldvalue, newvalue) => ((Button)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue),
			propertyChanged: (bindable, oldvalue, newvalue) => ((Button)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue));

		bool _cancelEvents;

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderColorProperty); }
			set { SetValue(BorderColorProperty, value); }
		}

		public int BorderRadius
		{
			get { return (int)GetValue(BorderRadiusProperty); }
			set { SetValue(BorderRadiusProperty, value); }
		}

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
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
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		bool IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}

		void IButtonController.SendClicked()
		{
			ICommand cmd = Command;
			if (cmd != null)
				cmd.Execute(CommandParameter);

			EventHandler handler = Clicked;
			if (handler != null)
				handler(this, EventArgs.Empty);
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

		static void FontStructPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var button = (Button)bindable;

			if (button._cancelEvents)
				return;

			button.InvalidateMeasure(InvalidationTrigger.MeasureChanged);

			button._cancelEvents = true;

			if (button.Font == Font.Default)
			{
				button.FontFamily = null;
				button.FontSize = Device.GetNamedSize(NamedSize.Default, button);
				button.FontAttributes = FontAttributes.None;
			}
			else
			{
				button.FontFamily = button.Font.FontFamily;
				if (button.Font.UseNamedSize)
				{
					button.FontSize = Device.GetNamedSize(button.Font.NamedSize, button.GetType(), true);
				}
				else
				{
					button.FontSize = button.Font.FontSize;
				}
				button.FontAttributes = button.Font.FontAttributes;
			}

			button._cancelEvents = false;
		}

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
			InvalidateMeasure(InvalidationTrigger.MeasureChanged);
		}

		void OnSourcePropertyChanged(ImageSource oldvalue, ImageSource newvalue)
		{
			if (newvalue != null)
			{
				newvalue.SourceChanged += OnSourceChanged;
				SetInheritedBindingContext(newvalue, BindingContext);
			}
			InvalidateMeasure(InvalidationTrigger.MeasureChanged);
		}

		void OnSourcePropertyChanging(ImageSource oldvalue, ImageSource newvalue)
		{
			if (oldvalue != null)
				oldvalue.SourceChanged -= OnSourceChanged;
		}

		static void SpecificFontPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var button = (Button)bindable;

			if (button._cancelEvents)
				return;

			button.InvalidateMeasure(InvalidationTrigger.MeasureChanged);

			button._cancelEvents = true;

			if (button.FontFamily != null)
			{
				button.Font = Font.OfSize(button.FontFamily, button.FontSize).WithAttributes(button.FontAttributes);
			}
			else
			{
				button.Font = Font.SystemFontOfSize(button.FontSize, button.FontAttributes);
			}

			button._cancelEvents = false;
		}
	}
}