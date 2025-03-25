using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class VisibilityDesignTypeConverterTests
	{
		[Fact]
		public void VisibilityDesignTypeConverter_StandartValues()
		{
			VisibilityDesignTypeConverter converter = new VisibilityDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.GetStandardValuesSupported();
			Assert.True(actual);

			actual = converter.GetStandardValuesExclusive();
			Assert.True(actual);

			var values = converter.GetStandardValues();
			Assert.Equal(5, values.Count);
		}

		[Theory]
		[InlineData("true")]
		[InlineData("  FALSE ")]
		[InlineData("Collapse")]
		[InlineData("hidden ")]
		[InlineData(" VISIBLE")]
		public void VisibilityDesignTypeConverter_Valid(string value)
		{
			VisibilityDesignTypeConverter converter = new VisibilityDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("Collapse Hidden")]
		[InlineData("foo")]
		public void VisibilityDesignTypeConverter_Invalid(string value)
		{
			VisibilityDesignTypeConverter converter = new VisibilityDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
