using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class SizeTests : BaseTestFixture
	{
		[Fact]
		public void TestSizeIsZero()
		{
			var size = new Size();

			Assert.True(size.IsZero);

			size = new Size(10, 10);

			Assert.False(size.IsZero);
		}

		[Fact]
		public void TestSizeAdd()
		{
			var size1 = new Size(10, 10);
			var size2 = new Size(20, 20);

			var result = size1 + size2;

			Assert.Equal(new Size(30, 30), result);
		}

		[Fact]
		public void TestSizeSubtract()
		{
			var size1 = new Size(10, 10);
			var size2 = new Size(2, 2);

			var result = size1 - size2;

			Assert.Equal(new Size(8, 8), result);
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 0, 2, MemberType = typeof(TestDataHelpers))]
		public void TestPointFromSize(double x, double y)
		{
			var size = new Size(x, y);
			var point = (Point)size;

			Assert.Equal(x, point.X);
			Assert.Equal(y, point.Y);
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 3, 5, 4, MemberType = typeof(TestDataHelpers))]
		public void HashCodeTest(double w1, double h1, double w2, double h2)
		{
			bool result = new Size(w1, h1).GetHashCode() == new Size(w2, h2).GetHashCode();

			if (w1 == w2 && h1 == h2)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Fact]
		public void Equality()
		{
			Assert.False(new Size().Equals(null));
			Assert.False(new Size().Equals("Size"));
			Assert.True(new Size(2, 3).Equals(new Size(2, 3)));

			Assert.True(new Size(2, 3) == new Size(2, 3));
			Assert.True(new Size(2, 3) != new Size(3, 2));
		}

		[Theory]
		[InlineData(0, 0, "{Width=0 Height=0}")]
		[InlineData(1, 5, "{Width=1 Height=5}")]
		public void TestToString(double w, double h, string expectedResult)
		{
			var result = new Size(w, h).ToString();
			Assert.Equal(expectedResult, result);

		}

		public static IEnumerable<object[]> MultiplyByScalarData()
		{
			var range = Enumerable.Range(12, 3);
			var values = new List<object> { 0.0, 2.0, 7.0, 0.25 };

			foreach (var p1 in range)
			{
				foreach (var p2 in range)
				{
					foreach (var p3 in values)
					{
						yield return new object[] { p1, p2, p3 };
					}
				}
			}
		}

		[Theory, MemberData(nameof(MultiplyByScalarData))]
		public void MultiplyByScalar(int w, int h, double scalar)
		{
			var size = new Size(w, h);
			var result = size * scalar;

			Assert.Equal(w * scalar, result.Width);
			Assert.Equal(h * scalar, result.Height);
		}
	}
}
