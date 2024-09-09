using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ImageSourceDesignTypeConverterTests
	{
		[Theory]
		[InlineData("http://consoto.com")]
		[InlineData("file:///x:/logo.png")]
		[InlineData("consoto.com")]
		[InlineData("/app/logo.png")]
		[InlineData("logo.png")]
		[InlineData("")]
		[InlineData(" ")]
		public void ImageSourceDesignTypeConverter_Valid(string value)
		{
			ImageSourceDesignTypeConverter converter = new ImageSourceDesignTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("a:b:c:d")]
		public void ImageSourceDesignTypeConverter_Invalid(string value)
		{
			ImageSourceDesignTypeConverter converter = new ImageSourceDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}
