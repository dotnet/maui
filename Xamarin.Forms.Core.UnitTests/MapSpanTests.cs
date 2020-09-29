using NUnit.Framework;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MapSpanTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var span = new MapSpan(new Position(0, 0), 1, 1);

			Assert.AreEqual(new Position(0, 0), span.Center);
			Assert.AreEqual(1, span.LatitudeDegrees);
			Assert.AreEqual(1, span.LongitudeDegrees);
			Assert.IsTrue(span.Radius.Kilometers > 54 && span.Radius.Kilometers < 56);
		}

		[Test]
		public void Equals()
		{
			Assert.True(new MapSpan(new Position(1, 2), 3, 4) == new MapSpan(new Position(1, 2), 3, 4));
			Assert.True(new MapSpan(new Position(1, 2), 3, 4) != new MapSpan(new Position(2, 3), 4, 5));
			Assert.True(new MapSpan(new Position(1, 2), 3, 4).Equals(new MapSpan(new Position(1, 2), 3, 4)));
			Assert.False(new MapSpan(new Position(1, 2), 3, 4).Equals("MapSpan"));
			Assert.False(new MapSpan(new Position(1, 2), 3, 4).Equals(null));
		}

		[Test]
		public void HashCode()
		{
			Assert.AreEqual(new MapSpan(new Position(1, 2), 3, 4).GetHashCode(), new MapSpan(new Position(1, 2), 3, 4).GetHashCode());
		}

		[Test]
		public void RangeClamping()
		{
			var span = new MapSpan(new Position(0, 0), -1, -2);
			Assert.IsTrue(span.LatitudeDegrees > 0);
			Assert.IsTrue(span.LongitudeDegrees > 0);
		}
	}
}