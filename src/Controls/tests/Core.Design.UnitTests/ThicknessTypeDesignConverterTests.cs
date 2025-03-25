using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ThicknessTypeDesignConverterTests
	{
		[Theory]
		[InlineData("-5")]
		[InlineData("1,2")]
		[InlineData("1,2, 3, 4 ")]
		[InlineData("1 2")]
		[InlineData("1 2 3")]
		[InlineData("1 2 3 4")]
		public void ThicknessTypeDesignConverter_Valid(string value)
		{
			ThicknessTypeDesignConverter converter = new ThicknessTypeDesignConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("1,2,3")]
		[InlineData("1,2,3,4,5")]
		[InlineData("1   2")] // ThicknessConverter is sensitive to spaces; design converter matches its behavior
		[InlineData("1 2 3 4 5")]
		[InlineData("a,b")]
		public void ThicknessTypeDesignConverter_Invalid(string value)
		{
			ThicknessTypeDesignConverter converter = new ThicknessTypeDesignConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
