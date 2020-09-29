using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SizeTests : BaseTestFixture
	{
		[Test]
		public void TestSizeIsZero()
		{
			var size = new Size();

			Assert.True(size.IsZero);

			size = new Size(10, 10);

			Assert.False(size.IsZero);
		}

		[Test]
		public void TestSizeAdd()
		{
			var size1 = new Size(10, 10);
			var size2 = new Size(20, 20);

			var result = size1 + size2;

			Assert.AreEqual(new Size(30, 30), result);
		}

		[Test]
		public void TestSizeSubtract()
		{
			var size1 = new Size(10, 10);
			var size2 = new Size(2, 2);

			var result = size1 - size2;

			Assert.AreEqual(new Size(8, 8), result);
		}

		[Test]
		public void TestPointFromSize([Range(0, 2)] double x, [Range(0, 2)] double y)
		{
			var size = new Size(x, y);
			var point = (Point)size;

			Assert.AreEqual(x, point.X);
			Assert.AreEqual(y, point.Y);
		}

		[Test]
		public void HashCode([Range(3, 5)] double w1, [Range(3, 5)] double h1, [Range(3, 5)] double w2, [Range(3, 5)] double h2)
		{
			bool result = new Size(w1, h1).GetHashCode() == new Size(w2, h2).GetHashCode();

			if (w1 == w2 && h1 == h2)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Test]
		public void Equality()
		{
			Assert.False(new Size().Equals(null));
			Assert.False(new Size().Equals("Size"));
			Assert.True(new Size(2, 3).Equals(new Size(2, 3)));

			Assert.True(new Size(2, 3) == new Size(2, 3));
			Assert.True(new Size(2, 3) != new Size(3, 2));
		}

		[Test]
		[TestCase(0, 0, ExpectedResult = "{Width=0 Height=0}")]
		[TestCase(1, 5, ExpectedResult = "{Width=1 Height=5}")]
		public string TestToString(double w, double h)
		{
			return new Size(w, h).ToString();
		}

		[Test]
		public void MultiplyByScalar([Range(12, 15)] int w, [Range(12, 15)] int h, [Values(0.0, 2.0, 7.0, 0.25)] double scalar)
		{
			var size = new Size(w, h);
			var result = size * scalar;

			Assert.AreEqual(w * scalar, result.Width);
			Assert.AreEqual(h * scalar, result.Height);
		}
	}
}