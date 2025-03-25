using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class CornerRadiusDesignTypeConverterTests
	{
		[Theory]
		[InlineData("1.6")]
		[InlineData("1, 2.7")]
		[InlineData("1, 2, 3.8")]
		[InlineData("1,2,3,4.9")]
		[InlineData("1 2.7")]
		[InlineData("1 2 3.8")]
		[InlineData("1 2 3 4.9")]
		public void CornerRadiusDesignTypeConverter_Valid_Common(string value)
		{
			CornerRadiusDesignTypeConverter converter = new CornerRadiusDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory] // CornerRadiusTypeConverter has unusual behavior for 2 or 3 comma separated tokens; design converter matches its behavior
		[InlineData("1,")]
		[InlineData("2,,")]
		[InlineData("3,hello")]
		[InlineData("4,hello,world")]
		public void CornerRadiusDesignTypeConverter_Valid_Unusual(string value)
		{
			CornerRadiusDesignTypeConverter converter = new CornerRadiusDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1    2")] // CornerRadiusTypeConverter is sensitive to spaces; design converter matches its behavior
		[InlineData("1,2,3,4,5")]
		[InlineData("1 2 3 4 5")]
		public void CornerRadiusDesignTypeConverter_Invalid(string value)
		{
			CornerRadiusDesignTypeConverter converter = new CornerRadiusDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
