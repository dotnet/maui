using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GridLengthDesignTypeConverterTests
	{
		[Fact]
		public void GridLength_CanConvertFromString()
		{
			GridLengthDesignTypeConverter converter = new GridLengthDesignTypeConverter();
			bool canConvert = converter.CanConvertFrom(typeof(string));
			Assert.True(canConvert);
		}

		[Theory]
		[InlineData("auto")]
		[InlineData("Auto ")]
		[InlineData("	AUTO")]
		[InlineData("	AutO	")]
		public void GridLength_Valid_Auto(string value)
		{
			bool actual = (new GridLengthDesignTypeConverter()).IsValid(value);
			Assert.True(actual);

			actual = (new GridLengthCollectionDesignTypeConverter()).IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData("*")]
		[InlineData(" * ")]
		[InlineData("	0*")]
		[InlineData("2*	")]
		[InlineData("1.234567*")]
		public void GridLength_Valid_Start(string value)
		{
			bool actual = (new GridLengthDesignTypeConverter()).IsValid(value);
			Assert.True(actual);

			actual = (new GridLengthCollectionDesignTypeConverter()).IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData("-0.00001*")]
		[InlineData("-2*")]
		[InlineData("NaN*")]
		public void GridLength_Invalid_Start(string value)
		{
			bool actual = (new GridLengthDesignTypeConverter()).IsValid(value);
			Assert.False(actual);

			actual = (new GridLengthCollectionDesignTypeConverter()).IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData("0")]
		[InlineData(" 2 ")]
		[InlineData("1.234")]
		public void GridLength_Valid_Absolute(string value)
		{
			bool actual = (new GridLengthDesignTypeConverter()).IsValid(value);
			Assert.True(actual);

			actual = (new GridLengthCollectionDesignTypeConverter()).IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData("-0.00001")]
		[InlineData("-2")]
		[InlineData("NaN")]
		public void GridLength_Invalid_Absolute(string value)
		{
			bool actual = (new GridLengthDesignTypeConverter()).IsValid(value);
			Assert.False(actual);

			actual = (new GridLengthCollectionDesignTypeConverter()).IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public void GridLength_Invalid_Empty(string value)
		{
			bool actual = (new GridLengthDesignTypeConverter()).IsValid(value);
			Assert.False(actual);

			actual = (new GridLengthCollectionDesignTypeConverter()).IsValid(value);
			Assert.False(actual);
		}
	}
}
