using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class MapSpanTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var span = new MapSpan(new Location(0, 0), 1, 1);

			Assert.Equal(new Location(0, 0), span.Center);
			Assert.Equal(1, span.LatitudeDegrees);
			Assert.Equal(1, span.LongitudeDegrees);
			Assert.True(span.Radius.Kilometers > 54 && span.Radius.Kilometers < 56);
		}

		[Fact]
		public void EqualsTest()
		{
			Assert.True(new MapSpan(new Location(1, 2), 3, 4) == new MapSpan(new Location(1, 2), 3, 4));
			Assert.True(new MapSpan(new Location(1, 2), 3, 4) != new MapSpan(new Location(2, 3), 4, 5));
			Assert.True(new MapSpan(new Location(1, 2), 3, 4).Equals(new MapSpan(new Location(1, 2), 3, 4)));
			Assert.False(new MapSpan(new Location(1, 2), 3, 4).Equals("MapSpan"));
			Assert.False(new MapSpan(new Location(1, 2), 3, 4).Equals(null));
		}

		[Fact]
		public void HashCode()
		{
			Assert.Equal(new MapSpan(new Location(1, 2), 3, 4).GetHashCode(), new MapSpan(new Location(1, 2), 3, 4).GetHashCode());
		}

		[Fact]
		public void RangeClamping()
		{
			var span = new MapSpan(new Location(0, 0), -1, -2);
			Assert.True(span.LatitudeDegrees > 0);
			Assert.True(span.LongitudeDegrees > 0);
		}
	}
}
