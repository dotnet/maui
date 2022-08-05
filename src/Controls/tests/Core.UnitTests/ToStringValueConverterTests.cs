using System;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ToStringValueConverterTests : BaseTestFixture
	{
		static readonly CultureInfo _enUsCulture = CultureInfo.GetCultureInfo("en-US");
		static readonly CultureInfo _skSkCulture = CultureInfo.GetCultureInfo("sk-SK");


		public ToStringValueConverterTests()
		{

			System.Threading.Thread.CurrentThread.CurrentCulture = _enUsCulture;
		}

		[Fact]
		public void NullObjectConvertsToNull()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(null, null, null, null);

			Assert.Null(result);
		}

		[Fact]
		public void NullObjectWithTargetTypeConvertsToNull()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(null, typeof(string), null, null);

			Assert.Null(result);
		}

		[Fact]
		public void ObjectConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(new object(), typeof(string), null, null);

			Assert.Equal("System.Object", result);
		}

		[Fact]
		public void EmptyStringConvertsToEmptyString()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(string.Empty, typeof(string), null, null);

			Assert.Equal(result, string.Empty);
		}

		[Fact]
		public void StringConvertsToString()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert("Hello, World!", typeof(string), null, null);

			Assert.Equal("Hello, World!", result);
		}

		private class ToStringObject
		{
			public ToStringObject(string value)
			{
				Value = value;
			}

			public string Value { get; set; }

			public int ToStringCounter { get; private set; }

			public override string ToString()
			{
				ToStringCounter++;
				return Value;
			}
		}

		[Fact]
		public void CustomObjectWithNullValueConvertsToNull()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject(null);
			object result = toStringValueConverter.Convert(value, typeof(string), null, null);

			Assert.Null(result);
			Assert.Equal(1, value.ToStringCounter);
		}

		[Fact]
		public void CustomObjectWithEmptyStringConvertsToEmptyString()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject(string.Empty);
			object result = toStringValueConverter.Convert(value, typeof(string), null, null);

			Assert.Equal(result, string.Empty);
			Assert.Equal(1, value.ToStringCounter);
		}

		[Fact]
		public void CustomObjectWithStringConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject("Test string");
			object result = toStringValueConverter.Convert(value, typeof(string), null, null);

			Assert.Equal("Test string", result);
			Assert.Equal(1, value.ToStringCounter);
		}

		[Fact]
		public void ExecutesToStringTwice()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject("Test string");
			object result = toStringValueConverter.Convert(value, typeof(string), null, _skSkCulture);

			Assert.Equal("Test string", result);

			value.Value = "Hello, World!";
			result = toStringValueConverter.Convert(value, typeof(string), null, _skSkCulture);

			Assert.Equal("Hello, World!", result);
			Assert.Equal(2, value.ToStringCounter);
		}

		[Fact]
		public void DoubleValueConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), null, null);

			Assert.Equal("99123.567", result);
		}

		[Fact]
		public void DoubleValueWithSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), null, _skSkCulture);

			Assert.Equal("99123,567", result);
		}

		[Fact]
		public void DoubleValueWithEmptyParameterConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), string.Empty, null);

			Assert.Equal("99123.567", result);
		}

		[Fact]
		public void DoubleValueWithEmptyParameterAndSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), string.Empty, _skSkCulture);

			Assert.Equal("99123,567", result);
		}

		[Fact]
		public void DoubleValueWithNumberFormatConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), "N2", null);

			Assert.Equal("99,123.57", result);
		}

		[Fact]
		public void DoubleValueWithNumberFormatAndSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), "N2", _skSkCulture);

			Assert.Equal("99 123,57", result);
		}

		[Fact]
		public void DoubleValueWithSpecificFormatConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var format = new ToStringObject("#,##0.000");
			object result = toStringValueConverter.Convert(99123.56, typeof(string), format, null);

			Assert.Equal("99,123.560", result);
		}

		[Fact]
		public void DoubleValueWithSpecificFormatAndSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var format = new ToStringObject("#,##0.000");
			object result = toStringValueConverter.Convert(99123.56, typeof(string), format, _skSkCulture);

			Assert.Equal("99 123,560", result);
		}

		[Fact]
		public void NullObjectConvertsBackThrowsException()
		{
			var toStringValueConverter = new ToStringValueConverter();

			Action action = () => toStringValueConverter.ConvertBack(null, null, null, null);

			Assert.Throws<NotSupportedException>(action);
		}

		[Fact]
		public void ObjectConvertsBackThrowsException()
		{
			var toStringValueConverter = new ToStringValueConverter();

			Action action = () => toStringValueConverter.ConvertBack(new object(), typeof(string), null, null);

			Assert.Throws<NotSupportedException>(action);
		}
	}
}
