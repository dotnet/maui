using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class GridLengthTypeConverterTests : BaseTestFixture
	{
		[Fact]
		public void TestAbsolute()
		{
			var converter = new GridLengthTypeConverter();

			Assert.Equal(new GridLength(42));
			Assert.Equal(new GridLength(42.2));

			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("foo"));
		}

		[Fact]
		public void TestAuto()
		{
			var converter = new GridLengthTypeConverter();

			Assert.Equal(GridLength.Auto, converter.ConvertFromInvariantString("auto");
			Assert.Equal(GridLength.Auto, converter.ConvertFromInvariantString(" AuTo ");
		}

		[Fact]
		public void TestStar()
		{
			var converter = new GridLengthTypeConverter();

			Assert.Equal(new GridLength(1, GridUnitType.Star));
			Assert.Equal(new GridLength(42, GridUnitType.Star));

		}

		[Fact]
		public void TestValue()
		{
			var converter = new GridLengthTypeConverter();
			Assert.Equal(new GridLength(3.3));
		}

		[Fact]
		public void TestValueStar()
		{
			var converter = new GridLengthTypeConverter();
			Assert.Equal(new GridLength(32.3, GridUnitType.Star));
		}
	}
}