using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz45299Control : ContentView
	{
		public static readonly BindableProperty PortraitLayoutProperty =
		   BindableProperty.Create(nameof(PortraitLayout), typeof(Bz45299OrientationLayout), typeof(Bz45299Control));
		public Bz45299OrientationLayout PortraitLayout
		{
			get { return (Bz45299OrientationLayout)GetValue(PortraitLayoutProperty); }
			set { this.SetValue(PortraitLayoutProperty, value); }
		}

	}

	public class Bz45299OrientationLayout : BindableObject
	{
		public static readonly BindableProperty SizeProperty =
		   BindableProperty.Create(nameof(Size), typeof(Bz45299UISize), typeof(Bz45299OrientationLayout), Bz45299UISize.Zero);
		public Bz45299UISize Size
		{
			get { return (Bz45299UISize)GetValue(SizeProperty); }
			set { SetValue(SizeProperty, value); }
		}

		public static readonly BindableProperty SpacingProperty =
		   BindableProperty.Create(nameof(Spacing), typeof(Bz45299UILength), typeof(Bz45299OrientationLayout), Bz45299UILength.Zero);
		public Bz45299UILength Spacing
		{
			get { return (Bz45299UILength)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}

		public static readonly BindableProperty CountProperty =
		   BindableProperty.Create(nameof(Count), typeof(int), typeof(Bz45299OrientationLayout), 1);
		public int Count
		{
			get { return (int)GetValue(CountProperty); }
			set { SetValue(CountProperty, value); }
		}
	}

	[System.ComponentModel.TypeConverter(typeof(Bz45299UILengthTypeConverter))]
	public class Bz45299UILength
	{
		public static Bz45299UILength Zero => new Bz45299UILength { Value = 0 };

		public double Value { get; set; }

		public static implicit operator string(Bz45299UILength uiLength) => uiLength.Value.ToString();
		public static implicit operator double(Bz45299UILength uiLength) => uiLength.Value;

		public static implicit operator Bz45299UILength(string value) => Zero;
		public static implicit operator Bz45299UILength(long value) => Zero;
		public static implicit operator Bz45299UILength(ulong value) => Zero;
		public static implicit operator Bz45299UILength(int value) => Zero;
		public static implicit operator Bz45299UILength(uint value) => Zero;
		public static implicit operator Bz45299UILength(double value) => Zero;
		public static implicit operator Bz45299UILength(float value) => Zero;
	}

	public class Bz45299UILengthTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string)
				|| sourceType == typeof(long)
				|| sourceType == typeof(ulong)
				|| sourceType == typeof(int)
				|| sourceType == typeof(uint)
				|| sourceType == typeof(double)
				|| sourceType == typeof(float);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			=> value switch
			{
				string str => (Bz45299UILength)str,
				long l => (Bz45299UILength)l,
				ulong ul => (Bz45299UILength)ul,
				int i => (Bz45299UILength)i,
				uint ui => (Bz45299UILength)ui,
				double d => (Bz45299UILength)d,
				float f => (Bz45299UILength)f,
				_ => throw new NotSupportedException(),
			};

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string)
				|| destinationType == typeof(double);

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is Bz45299UILength uiLength)
			{
				if (destinationType == typeof(string))
					return (string)uiLength;
				if (destinationType == typeof(double))
					return (double)uiLength;
			}

			throw new NotSupportedException();
		}
	}

	[System.ComponentModel.TypeConverter(typeof(Bz45299UISizeTypeConverter))]
	public class Bz45299UISize
	{
		public static Bz45299UISize Zero => new Bz45299UISize { Width = 0, Height = 0 };

		public Bz45299UILength Width { get; set; }
		public Bz45299UILength Height { get; set; }

		public static implicit operator Bz45299UISize(string value) => Zero;
		public static implicit operator Size(Bz45299UISize uiSize) => new Size(uiSize.Width, uiSize.Height);
		public static implicit operator Bz45299UISize(Size size) => new Bz45299UISize { Width = size.Width, Height = size.Height };
	}

	public class Bz45299UISizeTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string)
				|| sourceType == typeof(Size);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			=> value switch
			{
				string str => (Bz45299UISize)str,
				Size size => (Bz45299UISize)size,
				_ => throw new NotSupportedException(),
			};

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(Size);

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is Bz45299UISize uiSize)
			{
				if (destinationType == typeof(Size))
					return (Size)uiSize;
			}

			throw new NotSupportedException();
		}
	}

	public partial class Bz45299 : ContentPage
	{
		public Bz45299() => InitializeComponent();
		public Bz45299(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void XamlCCustomTypeConverter(bool useCompiledXaml)
			{
				var p = new Bz45299(useCompiledXaml);
				Assert.AreEqual(0d, p.ctrl.PortraitLayout.Spacing.Value);
			}
		}
	}
}