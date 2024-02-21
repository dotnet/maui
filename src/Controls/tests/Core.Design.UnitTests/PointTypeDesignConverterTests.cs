using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PointTypeDesignConverterTests
	{
		[Theory]
		[InlineData("1,2")]
		[InlineData(" -3.5, NaN")]
		public void PointTypeDesignConverter_Valid(string value)
		{
			PointTypeDesignConverter converter = new PointTypeDesignConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1")]
		[InlineData("1,2,3")]
		[InlineData("a,b")]
		public void PointTypeDesignConverter_Invalid(string value)
		{
			VisibilityDesignTypeConverter converter = new VisibilityDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
