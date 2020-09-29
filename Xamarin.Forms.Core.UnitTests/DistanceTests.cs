using System;
using NUnit.Framework;
using Xamarin.Forms.Maps;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DistanceTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var distance = new Distance(25);
			Assert.AreEqual(25, distance.Meters);
		}

		[Test]
		public void ConstructFromKilometers()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromKilometers(2);

			Assert.True(Math.Abs(distance.Kilometers - 2) < EPSILON);
			Assert.True(Math.Abs(distance.Meters - 2000) < EPSILON);
			Assert.True(Math.Abs(distance.Miles - 1.24274) < EPSILON);
		}

		[Test]
		public void ConstructFromMeters()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMeters(10560);

			Assert.True(Math.Abs(distance.Meters - 10560) < EPSILON);
			Assert.True(Math.Abs(distance.Miles - 6.5616798) < EPSILON);
			Assert.True(Math.Abs(distance.Kilometers - 10.56) < EPSILON);
		}

		[Test]
		public void ConstructFromMiles()
		{
			const double EPSILON = 0.001;

			// Reached the limit of double precision using the number
			// of miles of the earth's circumference
			const double EPSILON_FOR_LARGE_MILES_TO_METERS = 16;

			// Reached the limit of double precision
			const double EPSILON_FOR_LARGE_MILES_TO_KM = 0.1;

			Distance distance = Distance.FromMiles(3963.1676);

			Assert.True(Math.Abs(distance.Miles - 3963.1676) < EPSILON);
			Assert.True(Math.Abs(distance.Meters - 6378099.99805) < EPSILON_FOR_LARGE_MILES_TO_METERS);
			Assert.True(Math.Abs(distance.Kilometers - 6378.09999805) < EPSILON_FOR_LARGE_MILES_TO_KM);
		}

		[Test]
		public void ConstructFromPositions()
		{
			const double EPSILON = 0.001;

			Position position1 = new Position(37.403992, -122.034988);
			Position position2 = new Position(37.776691, -122.416534);

			Distance distance = Distance.BetweenPositions(position1, position2);

			Assert.True(Math.Abs(distance.Meters - 53363.08) < EPSILON);
			Assert.True(Math.Abs(distance.Kilometers - 53.36308) < EPSILON);
			Assert.True(Math.Abs(distance.Miles - 33.15828) < EPSILON);
		}

		[Test]
		public void EqualityOp([Range(5, 9)] double x, [Range(5, 9)] double y)
		{
			bool result = Distance.FromMeters(x) == Distance.FromMeters(y);

			if (x == y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Test]
		public void Equals([Range(3, 7)] double x, [Range(3, 7)] double y)
		{
			bool result = Distance.FromMiles(x).Equals(Distance.FromMiles(y));
			if (x == y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Test]
		public void EqualsNull()
		{
			Assert.False(Distance.FromMeters(5).Equals(null));
		}

		[Test]
		public void GettingAndSettingKilometers()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromKilometers(1891);
			Assert.True(Math.Abs(distance.Kilometers - 1891) < EPSILON);
		}

		[Test]
		public void GettingAndSettingMeters()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMeters(123434);
			Assert.True(Math.Abs(distance.Meters - 123434) < EPSILON);
		}

		[Test]
		public void GettingAndSettingMiles()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMiles(515);
			Assert.True(Math.Abs(distance.Miles - 515) < EPSILON);
		}

		[Test]
		public void HashCode([Range(4, 5)] double x, [Range(4, 5)] double y)
		{
			Distance distance1 = Distance.FromMiles(x);
			Distance distance2 = Distance.FromMiles(y);

			bool result = distance1.GetHashCode() == distance2.GetHashCode();

			if (x == y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Test]
		public void InequalityOp([Range(5, 9)] double x, [Range(5, 9)] double y)
		{
			bool result = Distance.FromMeters(x) != Distance.FromMeters(y);

			if (x != y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Test]
		public void ObjectInitializerKilometers()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromKilometers(10);
			Assert.True(Math.Abs(distance.Meters - 10000) < EPSILON);
		}

		[Test]
		public void ObjectInitializerMeters()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMeters(1057);
			Assert.True(Math.Abs(distance.Kilometers - 1.057) < EPSILON);
		}

		[Test]
		public void ObjectInitializerMiles()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMiles(100);
			Assert.True(Math.Abs(distance.Meters - 160934.4) < EPSILON);
		}

		[Test]
		public void ClampFromMeters()
		{
			var distance = Distance.FromMeters(-1);

			Assert.AreEqual(0, distance.Meters);
		}

		[Test]
		public void ClampFromMiles()
		{
			var distance = Distance.FromMiles(-1);

			Assert.AreEqual(0, distance.Meters);
		}

		[Test]
		public void ClampFromKilometers()
		{
			var distance = Distance.FromKilometers(-1);

			Assert.AreEqual(0, distance.Meters);
		}

		[Test]
		public void Equals()
		{
			Assert.True(Distance.FromMiles(2).Equals((object)Distance.FromMiles(2)));
		}
	}
}