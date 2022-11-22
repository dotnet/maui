using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DistanceTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var distance = new Distance(25);
			Assert.Equal(25, distance.Meters);
		}

		[Fact]
		public void ConstructFromKilometers()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromKilometers(2);

			Assert.True(Math.Abs(distance.Kilometers - 2) < EPSILON);
			Assert.True(Math.Abs(distance.Meters - 2000) < EPSILON);
			Assert.True(Math.Abs(distance.Miles - 1.24274) < EPSILON);
		}

		[Fact]
		public void ConstructFromMeters()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMeters(10560);

			Assert.True(Math.Abs(distance.Meters - 10560) < EPSILON);
			Assert.True(Math.Abs(distance.Miles - 6.5616798) < EPSILON);
			Assert.True(Math.Abs(distance.Kilometers - 10.56) < EPSILON);
		}

		[Fact]
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

		[Fact]
		public void ConstructFromPositions()
		{
			const double EPSILON = 0.001;

			var position1 = new Devices.Sensors.Location(37.403992, -122.034988);
			var position2 = new Devices.Sensors.Location(37.776691, -122.416534);

			var distance = Distance.BetweenPositions(position1, position2);

			Assert.True(Math.Abs(distance.Meters - 53363.08) < EPSILON);
			Assert.True(Math.Abs(distance.Kilometers - 53.36308) < EPSILON);
			Assert.True(Math.Abs(distance.Miles - 33.15828) < EPSILON);
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 5, 9, MemberType = typeof(TestDataHelpers))]
		public void EqualityOp(double x, double y)
		{
			bool result = Distance.FromMeters(x) == Distance.FromMeters(y);

			if (x == y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 3, 7, MemberType = typeof(TestDataHelpers))]
		public void EqualsTest(double x, double y)
		{
			bool result = Distance.FromMiles(x).Equals(Distance.FromMiles(y));
			if (x == y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Fact]
		public void EqualsNull()
		{
			Assert.False(Distance.FromMeters(5).Equals(null));
		}

		[Fact]
		public void GettingAndSettingKilometers()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromKilometers(1891);
			Assert.True(Math.Abs(distance.Kilometers - 1891) < EPSILON);
		}

		[Fact]
		public void GettingAndSettingMeters()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMeters(123434);
			Assert.True(Math.Abs(distance.Meters - 123434) < EPSILON);
		}

		[Fact]
		public void GettingAndSettingMiles()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMiles(515);
			Assert.True(Math.Abs(distance.Miles - 515) < EPSILON);
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 4, 5, MemberType = typeof(TestDataHelpers))]
		public void HashCode(double x, double y)
		{
			Distance distance1 = Distance.FromMiles(x);
			Distance distance2 = Distance.FromMiles(y);

			bool result = distance1.GetHashCode() == distance2.GetHashCode();

			if (x == y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 5, 9, MemberType = typeof(TestDataHelpers))]
		public void InequalityOp(double x, double y)
		{
			bool result = Distance.FromMeters(x) != Distance.FromMeters(y);

			if (x != y)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Fact]
		public void ObjectInitializerKilometers()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromKilometers(10);
			Assert.True(Math.Abs(distance.Meters - 10000) < EPSILON);
		}

		[Fact]
		public void ObjectInitializerMeters()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMeters(1057);
			Assert.True(Math.Abs(distance.Kilometers - 1.057) < EPSILON);
		}

		[Fact]
		public void ObjectInitializerMiles()
		{
			const double EPSILON = 0.001;

			Distance distance = Distance.FromMiles(100);
			Assert.True(Math.Abs(distance.Meters - 160934.4) < EPSILON);
		}

		[Fact]
		public void ClampFromMeters()
		{
			var distance = Distance.FromMeters(-1);

			Assert.Equal(0, distance.Meters);
		}

		[Fact]
		public void ClampFromMiles()
		{
			var distance = Distance.FromMiles(-1);

			Assert.Equal(0, distance.Meters);
		}

		[Fact]
		public void ClampFromKilometers()
		{
			var distance = Distance.FromKilometers(-1);

			Assert.Equal(0, distance.Meters);
		}

		[Fact]
		public void EqualsTest2()
		{
			Assert.True(Distance.FromMiles(2).Equals((object)Distance.FromMiles(2)));
		}
	}
}
