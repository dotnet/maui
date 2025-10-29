using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
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

	public partial class Gh4215 : ContentPage
	{
		public Gh4215()
		{
			InitializeComponent();
		}

		public Gh4215(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void AvoidAmbiguousMatch(bool useCompiledXaml)
			{
				var layout = new Gh4215(useCompiledXaml);
				Assert.DoesNotThrow(() => layout.BindingContext = new Gh4215VM());
				Assert.Equal("foo", layout.l0.Text);
			}
		}
	}
}
