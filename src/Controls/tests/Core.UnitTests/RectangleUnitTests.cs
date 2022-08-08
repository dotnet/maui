using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class RectangleUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestRectConstruction()
		{
			var rect = new Rect();
			Assert.Equal(0, rect.X);
			Assert.Equal(0, rect.Y);
			Assert.Equal(0, rect.Width);
			Assert.Equal(0, rect.Height);

			rect = new Rect(2, 3, 4, 5);
			Assert.Equal(2, rect.X);
			Assert.Equal(3, rect.Y);
			Assert.Equal(4, rect.Width);
			Assert.Equal(5, rect.Height);

			rect = new Rect(new Point(2, 3), new Size(4, 5));
			Assert.Equal(2, rect.X);
			Assert.Equal(3, rect.Y);
			Assert.Equal(4, rect.Width);
			Assert.Equal(5, rect.Height);
		}

		[Fact]
		public void TestRectFromLTRB()
		{
			var rect = Rect.FromLTRB(10, 10, 30, 40);

			Assert.Equal(new Rect(10, 10, 20, 30), rect);
		}

		[Fact]
		public void TestRectCalculatedPoints()
		{
			var rect = new Rect(2, 3, 4, 5);
			Assert.Equal(2, rect.Left);
			Assert.Equal(3, rect.Top);
			Assert.Equal(6, rect.Right);
			Assert.Equal(8, rect.Bottom);

			Assert.Equal(new Size(4, 5), rect.Size);
			Assert.Equal(new Point(2, 3), rect.Location);

			Assert.Equal(new Point(4, 5.5), rect.Center);

			rect.Left = 1;
			Assert.Equal(1, rect.X);

			rect.Right = 3;
			Assert.Equal(2, rect.Width);

			rect.Top = 1;
			Assert.Equal(1, rect.Y);

			rect.Bottom = 2;
			Assert.Equal(1, rect.Height);
		}

		[Fact]
		public void TestRectContains()
		{
			var rect = new Rect(0, 0, 10, 10);
			Assert.True(rect.Contains(5, 5));
			Assert.True(rect.Contains(new Point(5, 5)));
			Assert.True(rect.Contains(new Rect(1, 1, 3, 3)));

			Assert.True(rect.Contains(0, 0));
			Assert.False(rect.Contains(10, 10));
		}

		[Fact]
		public void TestRectInflate()
		{
			var rect = new Rect(0, 0, 10, 10);
			rect = rect.Inflate(5, 5);

			Assert.Equal(new Rect(-5, -5, 20, 20), rect);

			rect = rect.Inflate(new Size(-5, -5));

			Assert.Equal(new Rect(0, 0, 10, 10), rect);
		}

		[Fact]
		public void TestRectOffset()
		{
			var rect = new Rect(0, 0, 10, 10);
			rect = rect.Offset(10, 10);

			Assert.Equal(new Rect(10, 10, 10, 10), rect);

			rect = rect.Offset(new Point(-10, -10));

			Assert.Equal(new Rect(0, 0, 10, 10), rect);
		}

		[Fact]
		public void TestRectRound()
		{
			var rect = new Rect(0.2, 0.3, 0.6, 0.7);

			Assert.Equal(new Rect(0, 0, 1, 1), rect.Round());
		}

		[Fact]
		public void TestRectIntersect()
		{
			var rect1 = new Rect(0, 0, 10, 10);

			var rect2 = new Rect(2, 2, 6, 6);

			var intersection = rect1.Intersect(rect2);

			Assert.Equal(rect2, intersection);

			rect2 = new Rect(2, 2, 12, 12);
			intersection = rect1.Intersect(rect2);

			Assert.Equal(new Rect(2, 2, 8, 8), intersection);

			rect2 = new Rect(20, 20, 2, 2);
			intersection = rect1.Intersect(rect2);

			Assert.Equal(Rect.Zero, intersection);
		}

		[Theory]
		[InlineData(0, 0, true)]
		[InlineData(0, 5, true)]
		[InlineData(5, 0, true)]
		[InlineData(2, 3, false)]
		public void TestIsEmpty(int w, int h, bool isEmpty)
		{
			Assert.Equal(isEmpty, new Rect(0, 0, w, h).IsEmpty);
		}

		[Theory]
		[InlineData(0, 0, 8, 8, 0, 0, 5, 5, true)]
		[InlineData(0, 0, 5, 5, 5, 5, 5, 5, false)]
		[InlineData(0, 0, 2, 2, 3, 0, 5, 5, false)]
		public void TestIntersectsWith(double x1, double y1, double w1, double h1, double x2, double y2, double w2, double h2, bool expected)
		{
			Assert.Equal(expected, new Rect(x1, y1, w1, h1).IntersectsWith(new Rect(x2, y2, w2, h2)));
		}

		[Fact]
		public void TestSetSize()
		{
			var rect = new Rect();
			rect.Size = new Size(10, 20);

			Assert.Equal(new Rect(0, 0, 10, 20), rect);
		}

		[Fact]
		public void TestSetLocation()
		{
			var rect = new Rect();
			rect.Location = new Point(10, 20);

			Assert.Equal(new Rect(10, 20, 0, 0), rect);
		}

		[Fact]
		public void TestUnion()
		{
			Assert.Equal(new Rect(0, 3, 13, 10), new Rect(3, 3, 10, 10).Union(new Rect(0, 5, 2, 2)));
		}

		[Theory]
		[InlineData(0, 0, 2, 2, "{X=0 Y=0 Width=2 Height=2}")]
		[InlineData(1, 0, 3, 2, "{X=1 Y=0 Width=3 Height=2}")]
		public void TestRectToString(double x, double y, double w, double h, string expected)
		{
			Assert.Equal(expected, new Rect(x, y, w, h).ToString());
		}

		[Fact]
		public void TestRectEquals()
		{
			Assert.True(new Rect(0, 0, 10, 10).Equals(new Rect(0, 0, 10, 10)));
			Assert.False(new Rect(0, 0, 10, 10).Equals("Rect"));
			Assert.False(new Rect(0, 0, 10, 10).Equals(null));

			Assert.True(new Rect(0, 0, 10, 10) == new Rect(0, 0, 10, 10));
			Assert.True(new Rect(0, 0, 10, 10) != new Rect(0, 0, 10, 5));
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 3, 4, 8, MemberType = typeof(TestDataHelpers))]
		public void TestRectGetHashCode(double x1, double y1, double w1, double h1,
											   double x2, double y2, double w2, double h2)
		{
			bool result = new Rect(x1, y1, w1, h1).GetHashCode() == new Rect(x2, y2, w2, h2).GetHashCode();

			if (x1 == x2 && y1 == y2 && w1 == w2 && h1 == h2)
				Assert.True(result);
			else
				Assert.False(result);
		}
	}
}
