using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class RectTypeDesignConverterTests
	{
		[Theory]
		[InlineData("1,2,3,4")]
		[InlineData(" -3.5, NaN, 7, Infinity")]
		public void RectTypeDesignConverter_Valid(string value)
		{
			RectTypeDesignConverter converter = new RectTypeDesignConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1")]
		[InlineData("1,2")]
		[InlineData("1,2,3,4,5")]
		[InlineData("a,b")]
		public void RectTypeDesignConverter_Invalid(string value)
		{
			RectTypeDesignConverter converter = new RectTypeDesignConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
