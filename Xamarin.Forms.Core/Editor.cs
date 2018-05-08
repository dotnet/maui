using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_EditorRenderer))]
	public class Editor : InputView, IEditorController, IFontElement, ITextElement, IElementConfiguration<Editor>
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Editor), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue)
			=> OnTextChanged((Editor)bindable, (string)oldValue, (string)newValue));

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Editor), default(string));

		public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(Editor), Color.Default);

		public static readonly BindableProperty AutoSizeProperty = BindableProperty.Create(nameof(AutoSize), typeof(EditorAutoSizeOption), typeof(Editor), defaultValue: EditorAutoSizeOption.Disabled, propertyChanged: (bindable, oldValue, newValue)
			=> ((Editor)bindable)?.InvalidateMeasure());


		readonly Lazy<PlatformConfigurationRegistry<Editor>> _platformConfigurationRegistry;


		public EditorAutoSizeOption AutoSize
		{
			get { return (EditorAutoSizeOption)GetValue(AutoSizeProperty); }
			set { SetValue(AutoSizeProperty, value); }
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

		protected void UpdateAutoSizeOption()
		{
			if (AutoSize == EditorAutoSizeOption.TextChanges)
			{
				InvalidateMeasure();
			}
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue)
		{
			UpdateAutoSizeOption();
		}

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue)
		{
			UpdateAutoSizeOption();
		}

		void IFontElement.OnFontChanged(Font oldValue, Font newValue)
		{
			UpdateAutoSizeOption();
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Editor)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
		{
			UpdateAutoSizeOption();
		}

		public event EventHandler Completed;

		public event EventHandler<TextChangedEventArgs> TextChanged;

		public Editor()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Editor>>(() => new PlatformConfigurationRegistry<Editor>(this));
		}

		public IPlatformElementConfiguration<T, Editor> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCompleted()
			=> Completed?.Invoke(this, EventArgs.Empty);

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		private static void OnTextChanged(Editor bindable, string oldValue, string newValue)
		{
			bindable.TextChanged?.Invoke(bindable, new TextChangedEventArgs(oldValue, newValue));
			if (bindable.AutoSize == EditorAutoSizeOption.TextChanges)
			{
				bindable.InvalidateMeasure();
			}
		}
	}
}