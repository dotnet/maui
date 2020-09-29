using System;
using NUnit.Framework;
using Xamarin.Forms.Maps;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class PositionTests : BaseTestFixture
	{
		[Test]
		public void Construction()
		{
			Position position = new Position();
			Assert.AreEqual(position.Latitude, 0);
			Assert.AreEqual(position.Longitude, 0);
		}

		[Test]
		public void CopyConstructor()
		{
			var position = new Position(12, 13);
			var copy = position;

			Assert.AreEqual(12, copy.Latitude);
			Assert.AreEqual(13, copy.Longitude);
		}

		[Test]
		public void EqualityOp()
		{
			var position1 = new Position(-28, 180);
			var position2 = new Position(-28, 180);
			Assert.True(position1 == position2);
		}

		[Test]
		public void InEqualityOp()
		{
			var position1 = new Position(-8, 180);
			var position2 = new Position(-28, 180);
			Assert.True(position1 != position2);
		}


		[Test]
		public void Equals()
		{
			var position1 = new Position(78, 167);
			var position2 = new Position(78, 167);
			Assert.True(position1.Equals(position2));
			Assert.False(position2.Equals(null));
			Assert.True(position2.Equals(position2));
			Assert.False(position2.Equals("position2"));
		}

		[Test]
		public void LatitudeClamping()
		{
			var position = new Position(-90.1, 0);
			Assert.AreEqual(position.Latitude, -90);

			position = new Position(165, 0);
			Assert.AreEqual(position.Latitude, 90);

			position = new Position(15.0, 0);
			Assert.AreEqual(position.Latitude, 15.0);
		}

		[Test]
		public void LongitudeClamping()
		{
			var position = new Position(0, -180.001);
			Assert.AreEqual(position.Longitude, -180.0);

			position = new Position(0, 1000);
			Assert.AreEqual(position.Longitude, 180);

			position = new Position(0, 0);
			Assert.AreEqual(position.Longitude, 0);
		}

		[Test]
		public void Hashcode()
		{
			var position = new Position(20, 25);
			var position2 = new Position(25, 20);
			Assert.AreNotEqual(position.GetHashCode(), position2.GetHashCode());
		}
	}
}