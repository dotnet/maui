using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Text")]
	public sealed class Span : INotifyPropertyChanged, IFontElement
	{
		class BindableSpan : BindableObject, IFontElement
		{
			Span _span;
			public BindableSpan(Span span)
			{
				_span = span;
			}

			public Font Font {
				get { return (Font)GetValue(FontElement.FontProperty); }
				set { SetValue(FontElement.FontProperty, value); }
			}

			public FontAttributes FontAttributes {
				get { return (FontAttributes)GetValue(FontElement.FontAttributesProperty); }
				set { SetValue(FontElement.FontAttributesProperty, value); }
			}

			public string FontFamily {
				get { return (string)GetValue(FontElement.FontFamilyProperty); }
				set { SetValue(FontElement.FontFamilyProperty, value); }
			}

			[TypeConverter(typeof(FontSizeConverter))]
			public double FontSize {
				get { return (double)GetValue(FontElement.FontSizeProperty); }
				set { SetValue(FontElement.FontSizeProperty, value); }
			}

			double IFontElement.FontSizeDefaultValueCreator() =>
				((IFontElement)_span).FontSizeDefaultValueCreator();

			public void OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) =>
				((IFontElement)_span).OnFontAttributesChanged(oldValue, newValue);

			public void OnFontChanged(Font oldValue, Font newValue) =>
				((IFontElement)_span).OnFontChanged(oldValue, newValue);

			public void OnFontFamilyChanged(string oldValue, string newValue) =>
				((IFontElement)_span).OnFontFamilyChanged(oldValue, newValue);

			public void OnFontSizeChanged(double oldValue, double newValue) =>
				((IFontElement)_span).OnFontSizeChanged(oldValue, newValue);

			protected override void OnPropertyChanged(string propertyName = null)
			{
				base.OnPropertyChanged(propertyName);
				_span.OnPropertyChanged(propertyName);
			}
		}

		Color _backgroundColor;

		BindableObject _fontElement;

		Color _foregroundColor;

		string _text;

		public Span()
		{
			_fontElement = new BindableSpan(this);
		}

		public Color BackgroundColor
		{
			get { return _backgroundColor; }
			set
			{
				if (_backgroundColor == value)
					return;
				_backgroundColor = value;
				OnPropertyChanged();
			}
		}

		[Obsolete("Font is obsolete as of version 1.3.0. Please use the Font properties directly.")]
		public Font Font {
			get { return (Font)_fontElement.GetValue(FontElement.FontProperty); }
			set { _fontElement.SetValue(FontElement.FontProperty, value); }
		}

		public Color ForegroundColor
		{
			get { return _foregroundColor; }
			set
			{
				if (_foregroundColor == value)
					return;
				_foregroundColor = value;
				OnPropertyChanged();
			}
		}

		public string Text
		{
			get { return _text; }
			set
			{
				if (_text == value)
					return;
				_text = value;
				OnPropertyChanged();
			}
		}

		public FontAttributes FontAttributes
		{
			get { return (FontAttributes)_fontElement.GetValue(FontElement.FontAttributesProperty); }
			set { _fontElement.SetValue(FontElement.FontAttributesProperty, value); }
		}

		public string FontFamily
		{
			get { return (string)_fontElement.GetValue(FontElement.FontFamilyProperty); }
			set { _fontElement.SetValue(FontElement.FontFamilyProperty, value); }
		}

		[TypeConverter(typeof(FontSizeConverter))]
		public double FontSize
		{
			get { return (double)_fontElement.GetValue(FontElement.FontSizeProperty); }
			set { _fontElement.SetValue(FontElement.FontSizeProperty, value); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		void IFontElement.OnFontFamilyChanged(string oldValue, string newValue)
		{
		}

		void IFontElement.OnFontSizeChanged(double oldValue, double newValue)
		{
		}

		double IFontElement.FontSizeDefaultValueCreator() =>
			Device.GetNamedSize(NamedSize.Default, new Label());

		void IFontElement.OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
		{
		}

		void IFontElement.OnFontChanged(Font oldValue, Font newValue)
		{
		}
	}
}