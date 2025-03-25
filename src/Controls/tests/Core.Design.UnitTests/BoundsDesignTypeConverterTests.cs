using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BoundsDesignTypeConverterTests
	{
		[Theory]
		[InlineData("1,2 ")]
		[InlineData("  3.1, -4.2, 5, -6")]
		[InlineData("  14,17, AutoSize, -20  ")]
		[InlineData("5,6,7,AUTOSIZE")]
		[InlineData("11,-12, autosize, AutoSize")]
		public void BoundsDesignTypeConverter_Valid(string value)
		{
			BoundsDesignTypeConverter converter = new BoundsDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		[InlineData("1")]
		[InlineData("2,3,4")]
		[InlineData(",7,8")]
		[InlineData("9,10,")]
		[InlineData("11,12,13,14,15")]
		[InlineData("AutoSize,AutoSize")]
		[InlineData("AutoSize,AutoSize,AutoSize,AutoSize")]
		public void BoundsDesignTypeConverter_Invalid(string value)
		{
			BoundsDesignTypeConverter converter = new BoundsDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
