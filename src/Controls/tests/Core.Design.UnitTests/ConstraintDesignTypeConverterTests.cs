using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ConstraintDesignTypeConverterTests
	{
		[Theory]
		[InlineData("1")]
		[InlineData(" -2.3")]
		[InlineData("  NaN ")]
		[InlineData("Infinity")]
		public void ConstraintDesignTypeConverter_Valid(string value)
		{
			ConstraintDesignTypeConverter converter = new ConstraintDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		[InlineData("1a")]
		public void ConstraintDesignTypeConverter_Invalid(string value)
		{
			ConstraintDesignTypeConverter converter = new ConstraintDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
