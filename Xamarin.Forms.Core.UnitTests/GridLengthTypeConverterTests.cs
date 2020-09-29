using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class GridLengthTypeConverterTests : BaseTestFixture
	{
		[Test]
		public void TestAbsolute()
		{
			var converter = new GridLengthTypeConverter();

			Assert.AreEqual(new GridLength(42), converter.ConvertFromInvariantString("42"));
			Assert.AreEqual(new GridLength(42.2), converter.ConvertFromInvariantString("42.2"));

			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("foo"));
		}

		[Test]
		public void TestAuto()
		{
			var converter = new GridLengthTypeConverter();

			Assert.AreEqual(GridLength.Auto, converter.ConvertFromInvariantString("auto"));
			Assert.AreEqual(GridLength.Auto, converter.ConvertFromInvariantString(" AuTo "));
		}

		[Test]
		public void TestStar()
		{
			var converter = new GridLengthTypeConverter();

			Assert.AreEqual(new GridLength(1, GridUnitType.Star), converter.ConvertFromInvariantString("*"));
			Assert.AreEqual(new GridLength(42, GridUnitType.Star), converter.ConvertFromInvariantString("42*"));

		}

		[Test]
		public void TestValue()
		{
			var converter = new GridLengthTypeConverter();
			Assert.AreEqual(new GridLength(3.3), converter.ConvertFromInvariantString("3.3"));
		}

		[Test]
		public void TestValueStar()
		{
			var converter = new GridLengthTypeConverter();
			Assert.AreEqual(new GridLength(32.3, GridUnitType.Star), converter.ConvertFromInvariantString("32.3*"));
		}
	}
}