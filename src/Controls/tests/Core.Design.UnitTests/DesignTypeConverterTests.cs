using Microsoft.Maui.Controls.Design;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class DesignTypeConverterTests
	{
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1")]
		[InlineData("Sta")]
		[InlineData("START")]
		public void LayoutOptionsDesignTypeConverter_Invalid(string value)
		{
			LayoutOptionsDesignTypeConverter converter = new LayoutOptionsDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData("Start")]
		[InlineData("End")]
		[InlineData("FillAndExpand")]
		public void LayoutOptionsDesignTypeConverter_Valid(string value)
		{
			LayoutOptionsDesignTypeConverter converter = new LayoutOptionsDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1")]
		[InlineData("Lin")]
		[InlineData("LINEAR")]
		public void EasingDesignTypeConverter_Invalid(string value)
		{
			EasingDesignTypeConverter converter = new EasingDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData("Linear")]
		[InlineData("BounceIn")]
		[InlineData("CubicOut")]
		public void EasingDesignTypeConverter_Valid(string value)
		{
			EasingDesignTypeConverter converter = new EasingDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("1")]
		[InlineData("Tele")]
		[InlineData("TELEPHONE")]
		public void KeyboardDesignTypeConverter_Invalid(string value)
		{
			KeyboardDesignTypeConverter converter = new KeyboardDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData("Plain")]
		[InlineData("Numeric")]
		[InlineData("Telephone")]
		public void KeyboardDesignTypeConverter_Valid(string value)
		{
			KeyboardDesignTypeConverter converter = new KeyboardDesignTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}
	}
}
