using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ButtonContentDesignTypeConverterTests
	{
		[Theory]
		[InlineData("Left")]
		[InlineData(" TOP ")]
		[InlineData("2, right")]
		[InlineData("Left, -25 ")]
		[InlineData(" Bottom, .3 ")]
		public void ButtonContentDesignTypeConverter_Valid_Common(string value)
		{
			ButtonContentDesignTypeConverter converter = new ButtonContentDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(",1")]
		[InlineData("9,15")]
		[InlineData(",10,,,Top,")]
		[InlineData(" 4,,,,")]
		[InlineData("Left,")]
		[InlineData(" 15")]
		public void ButtonContentDesignTypeConverter_Valid_Unusual(string value)
		{
			// ButtonContentConverter.ConvertFrom allows these cases
			ButtonContentDesignTypeConverter converter = new ButtonContentDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(".2,Top")]
		[InlineData("Left,Right")]
		[InlineData(".3,.4")]
		public void ButtonContentDesignTypeConverter_Invalid(string value)
		{
			ButtonContentDesignTypeConverter converter = new ButtonContentDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
