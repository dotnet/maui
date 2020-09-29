using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class BrushUnitTests : BaseTestFixture
	{
		BrushTypeConverter _converter;

		[SetUp]
		public void SetUp()
		{
			_converter = new BrushTypeConverter();
		}

		[Test]
		[TestCase("rgb(6, 201, 198)")]
		[TestCase("rgba(6, 201, 188, 0.2)")]
		[TestCase("hsl(6, 20%, 45%)")]
		[TestCase("hsla(6, 20%, 45%,0.75)")]
		[TestCase("rgb(100%, 32%, 64%)")]
		[TestCase("rgba(100%, 32%, 64%,0.27)")]
		public void TestBrushTypeConverterWithColorDefinition(string colorDefinition)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(colorDefinition));
		}

		[Test]
		[TestCase("#ff00ff")]
		[TestCase("#00FF33")]
		[TestCase("#00FFff 40%")]
		public void TestBrushTypeConverterWithColorHex(string colorHex)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(colorHex));
		}

		[Test]
		[TestCase("Color.yellow")]
		[TestCase("Color.Blue")]
		[TestCase("Color.pink.50%")]
		[TestCase("Color.Red.50%")]
		public void TestBrushTypeConverterWithColorName(string colorHex)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(colorHex));
		}

		[Test]
		[TestCase("linear-gradient(90deg, rgb(255, 0, 0),rgb(255, 153, 51))")]
		[TestCase("radial-gradient(circle, rgb(255, 0, 0) 25%, rgb(0, 255, 0) 50%, rgb(0, 0, 255) 75%)")]
		public void TestBrushTypeConverterWithBrush(string brush)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(brush));
		}
	}
}