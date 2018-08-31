using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_EntryRenderer))]
	public class Entry : InputView, IFontElement, ITextElement, ITextAlignmentElement, IEntryController, IElementConfiguration<Entry>
	{
		public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(Entry), ReturnType.Default);

		public static readonly BindableProperty ReturnCommandProperty = BindableProperty.Create(nameof(ReturnCommand), typeof(ICommand), typeof(Entry), default(ICommand));

		public static readonly BindableProperty ReturnCommandParameterProperty = BindableProperty.Create(nameof(ReturnCommandParameter), typeof(object), typeof(Entry), default(object));

		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(Entry), default(string));

		public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create("IsPassword", typeof(bool), typeof(Entry), default(bool));

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Entry), null, BindingMode.TwoWay, propertyChanged: OnTextChanged);

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty HorizontalTextAlignmentProperty = TextAlignmentElement.HorizontalTextAlignmentProperty;

		public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(Entry), Color.Default);

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty IsTextPredictionEnabledProperty = BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(Entry), true, BindingMode.OneTime);

		public static readonly BindableProperty CursorPositionProperty = BindableProperty.Create(nameof(CursorPosition), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		public static readonly BindableProperty SelectionLengthProperty = BindableProperty.Create(nameof(SelectionLength), typeof(int), typeof(Entry), 0, validateValue: (b, v) => (int)v >= 0);

		readonly Lazy<PlatformConfigurationRegistry<Entry>> _platformConfigurationRegistry;

		public Entry()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Entry>>(() => new PlatformConfigurationRegistry<Entry>(this));
		}

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentElement.HorizontalTextAlignmentProperty); }
			set { SetValue(TextAlignmentElement.HorizontalTextAlignmentProperty, value); }
		}

		public bool IsPassword
		{
			get { return (bool)GetValue(IsPasswordProperty); }
			set { SetValue(IsPasswordProperty, value); }
		}

		public string Placeholder
		{
			get { return (string)GetValue(PlaceholderProperty); }
			set { SetValue(PlaceholderProperty, value); }
		}

		public Color PlaceholderColor
		{
			get { return (Color)GetValue(PlaceholderColorProperty); }
			set { SetValue(PlaceholderColorProperty, value); }
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

		public bool IsTextPredictionEnabled
		{
			get { return (bool)GetValue(IsTextPredictionEnabledProperty); }
			set { SetValue(IsTextPredictionEnabledProperty, value); }
		}
    
		public ReturnType ReturnType
		{
			get => (ReturnType)GetValue(ReturnTypeProperty);
			set => SetValue(ReturnTypeProperty, value);
		}

		public int CursorPosition
		{
			get { return (int)GetValue(CursorPositionProperty); }
			set { SetValue(CursorPositionProperty, value); }
		}

		public int SelectionLength
		{
			get { return (int)GetValue(SelectionLengthProperty); }
			set { SetValue(SelectionLengthProperty, value); }
		}

		public ICommand ReturnCommand
		{
			get => (ICommand)GetValue(ReturnCommandProperty);
			set => SetValue(ReturnCommandProperty, value);
		}

		public object ReturnCommandParameter
		{
			get => GetValue(ReturnCommandParameterProperty);
			set => SetValue(ReturnCommandParameterProperty, value);
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Entry)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue) =>
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		void IFontElement.OnFontChanged(Font oldValue, Font newValue) =>
			 InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		public event EventHandler Completed;

		public event EventHandler<TextChangedEventArgs> TextChanged;

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

		static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var entry = (Entry)bindable;

			entry.TextChanged?.Invoke(entry, new TextChangedEventArgs((string)oldValue, (string)newValue));
		}

		public IPlatformElementConfiguration<T, Entry> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		void ITextAlignmentElement.OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue)
		{
		}
	}
}