using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_EditorRenderer))]
	public class Editor : InputView, IEditorController, IFontElement, ITextElement, IElementConfiguration<Editor>
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Editor), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var editor = (Editor)bindable;
			if (editor.TextChanged != null)
				editor.TextChanged(editor, new TextChangedEventArgs((string)oldValue, (string)newValue));
		});

		public static readonly BindableProperty FontFamilyProperty = FontElement.FontFamilyProperty;

		public static readonly BindableProperty FontSizeProperty = FontElement.FontSizeProperty;

		public static readonly BindableProperty FontAttributesProperty = FontElement.FontAttributesProperty;

		public static readonly BindableProperty TextColorProperty = TextElement.TextColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<Editor>> _platformConfigurationRegistry;

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

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue)
		{
		}

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue)
		{
		}

		void IFontElement.OnFontChanged(Font oldValue, Font newValue)
		{
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, (Editor)this);

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
		{
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
		{
			EventHandler handler = Completed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		void ITextElement.OnTextColorPropertyChanged(Color oldValue, Color newValue)
		{
		}
	}
}