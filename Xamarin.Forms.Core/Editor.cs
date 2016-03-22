using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_EditorRenderer))]
	public class Editor : InputView, IFontElement
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Editor), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var editor = (Editor)bindable;
			if (editor.TextChanged != null)
				editor.TextChanged(editor, new TextChangedEventArgs((string)oldValue, (string)newValue));
		});

		public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create("FontFamily", typeof(string), typeof(Editor), default(string));

		public static readonly BindableProperty FontSizeProperty = BindableProperty.Create("FontSize", typeof(double), typeof(Editor), -1.0,
			defaultValueCreator: bindable => Device.GetNamedSize(NamedSize.Default, (Editor)bindable));

		public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(Editor), FontAttributes.None);

		public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(Editor), Color.Default);

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

		public event EventHandler Completed;

		public event EventHandler<TextChangedEventArgs> TextChanged;

		internal void SendCompleted()
		{
			EventHandler handler = Completed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}
	}
}