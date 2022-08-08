using System;
using Microsoft.Maui.Controls.Maps;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class PositionTests : BaseTestFixture
	{
		[Fact]
		public void Construction()
		{
			Position position = new Position();
			Assert.Equal(0, position.Latitude);
			Assert.Equal(0, position.Longitude);
		}

		[Fact]
		public void CopyConstructor()
		{
			var position = new Position(12, 13);
			var copy = position;

			Assert.Equal(12, copy.Latitude);
			Assert.Equal(13, copy.Longitude);
		}

		[Fact]
		public void EqualityOp()
		{
			var position1 = new Position(-28, 180);
			var position2 = new Position(-28, 180);
			Assert.True(position1 == position2);
		}

		[Fact]
		public void InEqualityOp()
		{
			var position1 = new Position(-8, 180);
			var position2 = new Position(-28, 180);
			Assert.True(position1 != position2);
		}


		[Fact]
		public void EqualsTest()
		{
			var position1 = new Position(78, 167);
			var position2 = new Position(78, 167);
			Assert.True(position1.Equals(position2));
			Assert.False(position2.Equals(null));
			Assert.True(position2.Equals(position2));
			Assert.False(position2.Equals("position2"));
		}

		[Fact]
		public void LatitudeClamping()
		{
			var position = new Position(-90.1, 0);
			Assert.Equal(position.Latitude, -90);

			position = new Position(165, 0);
			Assert.Equal(90, position.Latitude);

			position = new Position(15.0, 0);
			Assert.Equal(15.0, position.Latitude);
		}

		[Fact]
		public void LongitudeClamping()
		{
			var position = new Position(0, -180.001);
			Assert.Equal(position.Longitude, -180.0);

			position = new Position(0, 1000);
			Assert.Equal(180, position.Longitude);

			position = new Position(0, 0);
			Assert.Equal(0, position.Longitude);
		}

		[Fact]
		public void Hashcode()
		{
			var position = new Position(20, 25);
			var position2 = new Position(25, 20);
			Assert.NotEqual(position.GetHashCode(), position2.GetHashCode());
		}
	}
}
