using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Text")]
	[RenderWith(typeof(_LabelRenderer))]
	public class Label : View, IFontElement, IElementConfiguration<Label>
	{
		public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create("HorizontalTextAlignment", typeof(TextAlignment), typeof(Label), TextAlignment.Start,
			propertyChanged: OnHorizontalTextAlignmentPropertyChanged);

		[Obsolete("XAlignProperty is obsolete. Please use HorizontalTextAlignmentProperty instead.")]
		public static readonly BindableProperty XAlignProperty = HorizontalTextAlignmentProperty;

		public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create("VerticalTextAlignment", typeof(TextAlignment), typeof(Label), TextAlignment.Start,
			propertyChanged: OnVerticalTextAlignmentPropertyChanged);

		[Obsolete("YAlignProperty is obsolete. Please use VerticalTextAlignmentProperty instead.")]
		public static readonly BindableProperty YAlignProperty = VerticalTextAlignmentProperty;

		public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(Label), Color.Default);

		public static readonly BindableProperty FontProperty = BindableProperty.Create("Font", typeof(Font), typeof(Label), default(Font), propertyChanged: FontStructPropertyChanged);

		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Label), default(string), propertyChanged: OnTextPropertyChanged);

		public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create("FontFamily", typeof(string), typeof(Label), default(string), propertyChanged: OnFontFamilyChanged);

		public static readonly BindableProperty FontSizeProperty = BindableProperty.Create("FontSize", typeof(double), typeof(Label), -1.0, propertyChanged: OnFontSizeChanged,
			defaultValueCreator: bindable => Device.GetNamedSize(NamedSize.Default, (Label)bindable));

		public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(Label), FontAttributes.None,
			propertyChanged: OnFontAttributesChanged);

		public static readonly BindableProperty FormattedTextProperty = BindableProperty.Create("FormattedText", typeof(FormattedString), typeof(Label), default(FormattedString),
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					((FormattedString)oldvalue).PropertyChanged -= ((Label)bindable).OnFormattedTextChanged;
			}, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (newvalue != null)
					((FormattedString)newvalue).PropertyChanged += ((Label)bindable).OnFormattedTextChanged;
				((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				if (newvalue != null)
					((Label)bindable).Text = null;
			});

		public static readonly BindableProperty LineBreakModeProperty = BindableProperty.Create("LineBreakMode", typeof(LineBreakMode), typeof(Label), LineBreakMode.WordWrap,
			propertyChanged: (bindable, oldvalue, newvalue) => ((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		readonly Lazy<PlatformConfigurationRegistry<Label>> _platformConfigurationRegistry;

		public Label()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Label>>(() => new PlatformConfigurationRegistry<Label>(this));
		}

		bool _cancelEvents;

		[Obsolete("Please use the Font attributes which are on the class itself. Obsoleted in v1.3.0")]
		public Font Font
		{
			get { return (Font)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		public FormattedString FormattedText
		{
			get { return (FormattedString)GetValue(FormattedTextProperty); }
			set { SetValue(FormattedTextProperty, value); }
		}

		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalTextAlignmentProperty); }
			set { SetValue(HorizontalTextAlignmentProperty, value); }
		}

		public LineBreakMode LineBreakMode
		{
			get { return (LineBreakMode)GetValue(LineBreakModeProperty); }
			set { SetValue(LineBreakModeProperty, value); }
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

		public TextAlignment VerticalTextAlignment
		{
			get { return (TextAlignment)GetValue(VerticalTextAlignmentProperty); }
			set { SetValue(VerticalTextAlignmentProperty, value); }
		}

		[Obsolete("XAlign is obsolete. Please use HorizontalTextAlignment instead.")]
		public TextAlignment XAlign
		{
			get { return (TextAlignment)GetValue(XAlignProperty); }
			set { SetValue(XAlignProperty, value); }
		}

		[Obsolete("YAlign is obsolete. Please use VerticalTextAlignment instead.")]
		public TextAlignment YAlign
		{
			get { return (TextAlignment)GetValue(YAlignProperty); }
			set { SetValue(YAlignProperty, value); }
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

		static void FontStructPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;
			if (label._cancelEvents)
				return;

			label._cancelEvents = true;

			var font = (Font)newValue;
			if (font == Font.Default)
			{
				label.FontFamily = null;
				label.FontSize = Device.GetNamedSize(NamedSize.Default, label);
				label.FontAttributes = FontAttributes.None;
			}
			else
			{
				label.FontFamily = font.FontFamily;
				if (font.UseNamedSize)
				{
					label.FontSize = Device.GetNamedSize(font.NamedSize, label.GetType(), true);
				}
				else
				{
					label.FontSize = font.FontSize;
				}
				label.FontAttributes = font.FontAttributes;
			}

			label._cancelEvents = false;

			label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;

			if (label._cancelEvents)
				return;

			label._cancelEvents = true;

			var attributes = (FontAttributes)newValue;

			object[] values = label.GetValues(FontFamilyProperty, FontSizeProperty);
			var family = (string)values[0];
			if (family != null)
			{
#pragma warning disable 0618 // retain until Font removed
				label.Font = Font.OfSize(family, (double)values[1]).WithAttributes(attributes);
#pragma warning restore 0618
			}
			else
			{
#pragma warning disable 0618 // retain until Font removed
				label.Font = Font.SystemFontOfSize((double)values[1], attributes);
#pragma warning restore 0618
			}

			label._cancelEvents = false;

			label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;
			if (label._cancelEvents)
				return;

			label._cancelEvents = true;

			object[] values = label.GetValues(FontSizeProperty, FontAttributesProperty);

			var family = (string)newValue;
			if (family != null)
			{
#pragma warning disable 0618 // retain until Font removed
				label.Font = Font.OfSize(family, (double)values[0]).WithAttributes((FontAttributes)values[1]);
#pragma warning restore 0618
			}
			else
			{
#pragma warning disable 0618 // retain until Font removed
				label.Font = Font.SystemFontOfSize((double)values[0], (FontAttributes)values[1]);
#pragma warning restore 0618
			}

			label._cancelEvents = false;
			label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;
			if (label._cancelEvents)
				return;

			label._cancelEvents = true;

			object[] values = label.GetValues(FontFamilyProperty, FontAttributesProperty);

			var size = (double)newValue;
			var family = (string)values[0];
			if (family != null)
			{
#pragma warning disable 0618 // retain until Font removed
				label.Font = Font.OfSize(family, size).WithAttributes((FontAttributes)values[1]);
#pragma warning restore 0618
			}
			else
			{
#pragma warning disable 0618 // retain until Font removed
				label.Font = Font.SystemFontOfSize(size, (FontAttributes)values[1]);
#pragma warning restore 0618
			}

			label._cancelEvents = false;

			label.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void OnFormattedTextChanged(object sender, PropertyChangedEventArgs e)
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			OnPropertyChanged("FormattedText");
		}

		static void OnHorizontalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;
#pragma warning disable 0618 // retain until XAlign removed
			label.OnPropertyChanged(nameof(XAlign));
#pragma warning restore 0618
		}

		static void OnTextPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var label = (Label)bindable;
			LineBreakMode breakMode = label.LineBreakMode;
			bool isVerticallyFixed = (label.Constraint & LayoutConstraint.VerticallyFixed) != 0;
			bool isSingleLine = !(breakMode == LineBreakMode.CharacterWrap || breakMode == LineBreakMode.WordWrap);
			if (!isVerticallyFixed || !isSingleLine)
				((Label)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			if (newvalue != null)
				((Label)bindable).FormattedText = null;
		}

		static void OnVerticalTextAlignmentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var label = (Label)bindable;
#pragma warning disable 0618 // retain until YAlign removed
			label.OnPropertyChanged(nameof(YAlign));
#pragma warning restore 0618
		}

		public IPlatformElementConfiguration<T, Label> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}