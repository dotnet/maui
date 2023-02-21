using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GridLengthCollectionDesignTypeConverterTests
	{
		[Fact]
		public void GridLengthCollection_CanConvertFromString()
		{
			GridLengthCollectionDesignTypeConverter converter = new GridLengthCollectionDesignTypeConverter();
			bool canConvert = converter.CanConvertFrom(typeof(string));
			Assert.True(canConvert);
		}

		[Theory]
		[InlineData("*,auto,123")]
		[InlineData("  *, AUTO  ,    123 ")]
		public void GridLengthCollection_Valid(string value)
		{
			GridLengthCollectionDesignTypeConverter converter = new GridLengthCollectionDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(",1")]
		[InlineData("1,")]
		[InlineData("1,,2")]
		[InlineData("1,-2,3")]
		public void GridLengthCollection_Invalid(string value)
		{
			GridLengthCollectionDesignTypeConverter converter = new GridLengthCollectionDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
