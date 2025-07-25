using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[TypeConverter(typeof(Gh4215VMTypeConverter))]
public class Gh4215VM
{
	public static implicit operator DateTime(Gh4215VM value) => DateTime.UtcNow;
	public static implicit operator string(Gh4215VM value) => "foo";
	public static implicit operator long(Gh4215VM value) => long.MaxValue;
	public static implicit operator Rect(Gh4215VM value) => new Rect();

	private sealed class Gh4215VMTypeConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(DateTime) || destinationType == typeof(string) || destinationType == typeof(long) || destinationType == typeof(Rect);
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is Gh4215VM vm)
			{
				if (destinationType == typeof(DateTime))
					return (DateTime)vm;
				if (destinationType == typeof(string))
					return (string)vm;
				if (destinationType == typeof(long))
					return (long)vm;
				if (destinationType == typeof(Rect))
					return (Rect)vm;
			}

			throw new NotSupportedException();
		}
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => throw new NotSupportedException();
	}
}

[XamlProcessing(XamlInflator.Default, true)]
public partial class Gh4215 : ContentPage
{
	public Gh4215() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void AvoidAmbiguousMatch([Values] XamlInflator inflator)
		{
			var layout = new Gh4215(inflator);
			Assert.DoesNotThrow(() => layout.BindingContext = new Gh4215VM());
			Assert.That(layout.l0.Text, Is.EqualTo("foo"));
		}
	}
}
