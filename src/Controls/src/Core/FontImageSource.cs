#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.FontImageSource']/Docs/*" />
	[ContentProperty(nameof(Glyph))]
	public partial class FontImageSource : ImageSource
	{
		/// <summary>Indicates whether the <see cref="Microsoft.Maui.Controls.FontImageSource"/> property is null or empty.</summary>
		public override bool IsEmpty => string.IsNullOrEmpty(Glyph);

		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(FontImageSource), default(Color),
			propertyChanged: (b, o, n) => ((FontImageSource)b).OnSourceChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>Bindable property for <see cref="FontFamily"/>.</summary>
		public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(FontImageSource), default(string),
			propertyChanged: (b, o, n) => ((FontImageSource)b).OnSourceChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="//Member[@MemberName='FontFamily']/Docs/*" />
		public string FontFamily
		{
			get => (string)GetValue(FontFamilyProperty);
			set => SetValue(FontFamilyProperty, value);
		}

		/// <summary>Bindable property for <see cref="Glyph"/>.</summary>
		public static readonly BindableProperty GlyphProperty = BindableProperty.Create(nameof(Glyph), typeof(string), typeof(FontImageSource), default(string),
			propertyChanged: (b, o, n) => ((FontImageSource)b).OnSourceChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="//Member[@MemberName='Glyph']/Docs/*" />
		public string Glyph
		{
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		/// <summary>Bindable property for <see cref="Size"/>.</summary>
		public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(double), typeof(FontImageSource), 30d,
			propertyChanged: (b, o, n) => ((FontImageSource)b).OnSourceChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="//Member[@MemberName='Size']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
		public double Size
		{
			get => (double)GetValue(SizeProperty);
			set => SetValue(SizeProperty, value);
		}

		/// <summary>Bindable property for <see cref="FontAutoScalingEnabled"/>.</summary>
		public static readonly BindableProperty FontAutoScalingEnabledProperty =
			BindableProperty.Create(nameof(FontAutoScalingEnabled), typeof(bool), typeof(FontImageSource), false,
				propertyChanged: (b, o, n) => ((FontImageSource)b).OnSourceChanged());

		public bool FontAutoScalingEnabled
		{
			get => (bool)GetValue(FontAutoScalingEnabledProperty);
			set => SetValue(FontAutoScalingEnabledProperty, value);
		}
	}
}
