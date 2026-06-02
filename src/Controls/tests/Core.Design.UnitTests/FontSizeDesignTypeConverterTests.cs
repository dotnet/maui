using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class FontSizeDesignTypeConverterTests
	{
		[Fact]
		public void FontSizeDesignTypeConverter_StandardValuesNotSupported()
		{
			FontSizeDesignTypeConverter converter = new FontSizeDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			// Standard values should not be supported to prevent IDE autocomplete
			// from suggesting obsolete named font size values
			bool actual = converter.GetStandardValuesSupported();
			Assert.False(actual);
		}

		[Fact]
		public void FontSizeDesignTypeConverter_StandardValuesEmpty()
		{
			FontSizeDesignTypeConverter converter = new FontSizeDesignTypeConverter();

			// GetStandardValues should return an empty collection
			var values = converter.GetStandardValues();
			Assert.Empty(values);
		}

		[Theory]
		[InlineData("Default")]
		[InlineData("Micro")]
		[InlineData("Small")]
		[InlineData("Medium")]
		[InlineData("Large")]
		[InlineData("Body")]
		[InlineData("Header")]
		[InlineData("Title")]
		[InlineData("Subtitle")]
		[InlineData("Caption")]
		public void FontSizeDesignTypeConverter_NamedValuesStillValid(string value)
		{
			// Named values are obsolete but should still be valid for backward compatibility
			FontSizeDesignTypeConverter converter = new FontSizeDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData("12")]
		[InlineData("0")]
		[InlineData("100")]
		public void FontSizeDesignTypeConverter_NumericValuesValid(string value)
		{
			FontSizeDesignTypeConverter converter = new FontSizeDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("foo")]
		[InlineData("extra large")]
		[InlineData("12px")]
		public void FontSizeDesignTypeConverter_InvalidValues(string value)
		{
			FontSizeDesignTypeConverter converter = new FontSizeDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
