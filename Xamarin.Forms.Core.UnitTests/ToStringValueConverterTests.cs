using System;
using System.Globalization;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ToStringValueConverterTests : BaseTestFixture
	{
		static readonly CultureInfo _enUsCulture = CultureInfo.GetCultureInfo("en-US");
		static readonly CultureInfo _skSkCulture = CultureInfo.GetCultureInfo("sk-SK");

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			System.Threading.Thread.CurrentThread.CurrentCulture = _enUsCulture;
		}

		[Test]
		public void NullObjectConvertsToNull()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(null, null, null, null);

			Assert.That(result, Is.Null);
		}

		[Test]
		public void NullObjectWithTargetTypeConvertsToNull()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(null, typeof(string), null, null);

			Assert.That(result, Is.Null);
		}

		[Test]
		public void ObjectConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(new object(), typeof(string), null, null);

			Assert.That(result, Is.EqualTo("System.Object"));
		}

		[Test]
		public void EmptyStringConvertsToEmptyString()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(string.Empty, typeof(string), null, null);

			Assert.That(result, Is.EqualTo(string.Empty));
		}

		[Test]
		public void StringConvertsToString()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert("Hello, World!", typeof(string), null, null);

			Assert.That(result, Is.EqualTo("Hello, World!"));
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

		[Test]
		public void CustomObjectWithNullValueConvertsToNull()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject(null);
			object result = toStringValueConverter.Convert(value, typeof(string), null, null);

			Assert.That(result, Is.Null);
			Assert.That(value.ToStringCounter, Is.EqualTo(1));
		}

		[Test]
		public void CustomObjectWithEmptyStringConvertsToEmptyString()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject(string.Empty);
			object result = toStringValueConverter.Convert(value, typeof(string), null, null);

			Assert.That(result, Is.EqualTo(string.Empty));
			Assert.That(value.ToStringCounter, Is.EqualTo(1));
		}

		[Test]
		public void CustomObjectWithStringConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject("Test string");
			object result = toStringValueConverter.Convert(value, typeof(string), null, null);

			Assert.That(result, Is.EqualTo("Test string"));
			Assert.That(value.ToStringCounter, Is.EqualTo(1));
		}

		[Test]
		public void ExecutesToStringTwice()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var value = new ToStringObject("Test string");
			object result = toStringValueConverter.Convert(value, typeof(string), null, _skSkCulture);

			Assert.That(result, Is.EqualTo("Test string"));

			value.Value = "Hello, World!";
			result = toStringValueConverter.Convert(value, typeof(string), null, _skSkCulture);

			Assert.That(result, Is.EqualTo("Hello, World!"));
			Assert.That(value.ToStringCounter, Is.EqualTo(2));
		}

		[Test]
		public void DoubleValueConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), null, null);

			Assert.That(result, Is.EqualTo("99123.567"));
		}

		[Test]
		public void DoubleValueWithSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), null, _skSkCulture);

			Assert.That(result, Is.EqualTo("99123,567"));
		}

		[Test]
		public void DoubleValueWithEmptyParameterConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), string.Empty, null);

			Assert.That(result, Is.EqualTo("99123.567"));
		}

		[Test]
		public void DoubleValueWithEmptyParameterAndSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), string.Empty, _skSkCulture);

			Assert.That(result, Is.EqualTo("99123,567"));
		}

		[Test]
		public void DoubleValueWithNumberFormatConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), "N2", null);

			Assert.That(result, Is.EqualTo("99,123.57"));
		}

		[Test]
		public void DoubleValueWithNumberFormatAndSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			object result = toStringValueConverter.Convert(99123.567, typeof(string), "N2", _skSkCulture);

			Assert.That(result, Is.EqualTo("99 123,57"));
		}

		[Test]
		public void DoubleValueWithSpecificFormatConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var format = new ToStringObject("#,##0.000");
			object result = toStringValueConverter.Convert(99123.56, typeof(string), format, null);

			Assert.That(result, Is.EqualTo("99,123.560"));
		}

		[Test]
		public void DoubleValueWithSpecificFormatAndSkCultureConvertsToStringValue()
		{
			var toStringValueConverter = new ToStringValueConverter();

			var format = new ToStringObject("#,##0.000");
			object result = toStringValueConverter.Convert(99123.56, typeof(string), format, _skSkCulture);

			Assert.That(result, Is.EqualTo("99 123,560"));
		}

		[Test]
		public void NullObjectConvertsBackThrowsException()
		{
			var toStringValueConverter = new ToStringValueConverter();

			TestDelegate action = () => toStringValueConverter.ConvertBack(null, null, null, null);

			Assert.That(action, Throws.InstanceOf<NotSupportedException>());
		}

		[Test]
		public void ObjectConvertsBackThrowsException()
		{
			var toStringValueConverter = new ToStringValueConverter();

			TestDelegate action = () => toStringValueConverter.ConvertBack(new object(), typeof(string), null, null);

			Assert.That(action, Throws.InstanceOf<NotSupportedException>());
		}
	}
}