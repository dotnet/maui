using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ColorDesignTypeConverterTests
	{
		[Theory]
		[InlineData("red")]
		[InlineData("Red")]
		[InlineData("RED")]
		[InlineData("DEFAULT")]
		[InlineData("default")]
		public void ColorConverter_ColorName_Valid_CaseInsensitive(string value)
		{
			ColorDesignTypeConverter converter = new ColorDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData("redd")]
		[InlineData("Redd")]
		public void ColorConverter_ColorName_Invalid(string value)
		{
			ColorDesignTypeConverter converter = new ColorDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData("default")]
		[InlineData(" Default    ")]
		public void ColorConverter_ColorName_Default(string value)
		{
			ColorDesignTypeConverter converter = new ColorDesignTypeConverter();
			bool actual = converter.IsValid(value);

			// Example:
			//   <Grid BackgroundColor="Default"/>
			// The "Default" value is okay to use in XAML. It builds and has default(Color)
			// value at runtime. Color.TryParse recognizes it but returns false. 
			Assert.True(actual);
		}
	}
}
