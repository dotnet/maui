using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class UnitConverters_Tests
    {
        [Theory]
        [InlineData(-1, -0.0175)]
        [InlineData(0.1, 0.0017)]
        [InlineData(0.5, 0.0087)]
        [InlineData(1, 0.0175)]
        [InlineData(2, 0.0349)]
        [InlineData(3, 0.0524)]
        [InlineData(10, 0.1745)]
        [InlineData(57.2958, 1)]
        [InlineData(114.5916, 2)]
        [InlineData(171.8873, 3)]
        [InlineData(572.9578, 10)]
        [InlineData(1000, 17.4533)]
        [InlineData(57295.7795, 1000)]
        [InlineData(0, 0)]
        public void DegreesPerSecondToRadiansPerSecond(double degrees, double radians)
        {
            Assert.Equal(radians, UnitConverters.DegreesPerSecondToRadiansPerSecond(degrees), 4);
        }

        [Theory]
        [InlineData(-1, -57.2958)]
        [InlineData(0.0017, 0.0974)]
        [InlineData(0.0087, 0.4985)]
        [InlineData(0.0175, 1.0027)]
        [InlineData(0.0349, 1.9996)]
        [InlineData(0.0524, 3.0023)]
        [InlineData(0.1745, 9.9981)]
        [InlineData(1, 57.2958)]
        [InlineData(2, 114.5916)]
        [InlineData(3, 171.8873)]
        [InlineData(10, 572.9578)]
        [InlineData(17.4533, 1000.0004)]
        [InlineData(1000, 57295.7795)]
        [InlineData(0, 0)]
        public void RadiansPerSecondToDegreesPerSecond(double radians, double degrees)
        {
            Assert.Equal(degrees, UnitConverters.RadiansPerSecondToDegreesPerSecond(radians), 4);
        }

        [Theory]
        [InlineData(-1, -10)]
        [InlineData(0.1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 100)]
        [InlineData(0, 0)]
        public void KilopascalsToHectopascals(double kpa, double hpa)
        {
            Assert.Equal(hpa, UnitConverters.KilopascalsToHectopascals(kpa), 4);
        }

        [Theory]
        [InlineData(-10, -1)]
        [InlineData(1, 0.1)]
        [InlineData(10, 1)]
        [InlineData(100, 10)]
        [InlineData(0, 0)]
        public void HectopascalsKilopascals(double hpa, double kpa)
        {
            Assert.Equal(kpa, UnitConverters.HectopascalsToKilopascals(hpa), 4);
        }

        [Theory]
        [InlineData(-1, -0.0175)]
        [InlineData(0.1, 0.0017)]
        [InlineData(1, 0.0175)]
        [InlineData(10, 0.1745)]
        [InlineData(180, 3.1416)]
        [InlineData(360, 6.2832)]
        [InlineData(10313.2403, 180)]
        [InlineData(0, 0)]
        public void DegreesToRadians(double deg, double rad)
        {
            Assert.Equal(rad, UnitConverters.DegreesToRadians(deg), 4);
        }

        [Theory]
        [InlineData(-1, -57.2958)]
        [InlineData(0.1, 5.7296)]
        [InlineData(1, 57.2958)]
        [InlineData(3.1416, 180.0004)]
        [InlineData(6.2832, 360.0008)]
        [InlineData(10, 572.9578)]
        [InlineData(180, 10313.2403)]
        [InlineData(360, 20626.4806)]
        [InlineData(0, 0)]
        public void RadiansToDegrees(double rad, double deg)
        {
            Assert.Equal(deg, UnitConverters.RadiansToDegrees(rad), 4);
        }

        [Theory]
        [InlineData(-1, -1.6093)]
        [InlineData(0.1, 0.1609)]
        [InlineData(1, 1.6093)]
        [InlineData(2, 3.2187)]
        [InlineData(3, 4.828)]
        [InlineData(10, 16.0934)]
        [InlineData(0, 0)]
        public void MilesToKilometers(double miles, double km)
        {
            Assert.Equal(km, UnitConverters.MilesToKilometers(miles), 4);
        }

        [Theory]
        [InlineData(-1, -0.6214)]
        [InlineData(0.1, 0.0621)]
        [InlineData(1, 0.6214)]
        [InlineData(2, 1.2427)]
        [InlineData(3, 1.8641)]
        [InlineData(10, 6.2137)]
        [InlineData(0, 0)]
        public void KilometersToMiles(double km, double miles)
        {
            Assert.Equal(miles, UnitConverters.KilometersToMiles(km), 4);
        }

        [Theory]
        [InlineData(55.85781, -4.24253, 51.509865, -0.118092, 554.3128)] // glasgow -> london
        [InlineData(36.12, -86.67, 33.94, -118.40, 2886.4444)] // nashville, tn -> los angeles, ca
        [InlineData(51.509865, -0.118092, -33.92528, 18.42389, 9671.1251)] // london -> cape town
        [InlineData(51.509865, -0.118092, 40.42028, -3.70577, 1263.4938)] // london -> madrid
        [InlineData(42.93708, -75.6107, -33.92528, 18.42389, 12789.5628)] // new york -> cape town
        [InlineData(45.80721, 15.96757, 19.432608, -99.133209, 10264.4796)] // zagreb -> mexico city
        [InlineData(43.623409, -79.368683, 42.35866, -71.05674, 690.2032)] // toronto -> boston, ma
        [InlineData(37.720134, -122.182552, 37.720266, -122.181969, .0533)]
        public void CoordinatesToKilometers(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            Assert.Equal(distance, UnitConverters.CoordinatesToKilometers(lat1, lon1, lat2, lon2), 4);
            var location1 = new Location(lat1, lon1);
            var location2 = new Location(lat2, lon2);
            Assert.Equal(distance, Location.CalculateDistance(location1, location2, DistanceUnits.Kilometers), 4);
            Assert.Equal(distance, Location.CalculateDistance(location2, location1, DistanceUnits.Kilometers), 4);
            Assert.Equal(distance, location1.CalculateDistance(location2, DistanceUnits.Kilometers), 4);
            Assert.Equal(distance, location2.CalculateDistance(location1, DistanceUnits.Kilometers), 4);
            Assert.Equal(distance, LocationExtensions.CalculateDistance(location1, location2, DistanceUnits.Kilometers), 4);
            Assert.Equal(distance, LocationExtensions.CalculateDistance(location2, location1, DistanceUnits.Kilometers), 4);
        }

        [Theory]
        [InlineData(55.85781, -4.24253, 51.509865, -0.118092, 554.3128)] // glasgow -> london
        [InlineData(36.12, -86.67, 33.94, -118.40, 2886.4444)] // nashville, tn -> los angeles, ca
        [InlineData(51.509865, -0.118092, -33.92528, 18.42389, 9671.1251)] // london -> cape town
        [InlineData(51.509865, -0.118092, 40.42028, -3.70577, 1263.4938)] // london -> madrid
        [InlineData(42.93708, -75.6107, -33.92528, 18.42389, 12789.5628)] // new york -> cape town
        [InlineData(45.80721, 15.96757, 19.432608, -99.133209, 10264.4796)] // zagreb -> mexico city
        [InlineData(43.623409, -79.368683, 42.35866, -71.05674, 690.2032)] // toronto -> boston, ma
        [InlineData(37.720134, -122.182552, 37.720266, -122.181969, .0533)]
        public void CoordinatesToMiles(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            distance = UnitConverters.KilometersToMiles(distance);
            Assert.Equal(distance, UnitConverters.CoordinatesToMiles(lat1, lon1, lat2, lon2), 3);
            var location1 = new Location(lat1, lon1);
            var location2 = new Location(lat2, lon2);
            Assert.Equal(distance, Location.CalculateDistance(location1, location2, DistanceUnits.Miles), 3);
            Assert.Equal(distance, Location.CalculateDistance(location2, location1, DistanceUnits.Miles), 3);
            Assert.Equal(distance, location1.CalculateDistance(location2, DistanceUnits.Miles), 3);
            Assert.Equal(distance, location2.CalculateDistance(location1, DistanceUnits.Miles), 3);
            Assert.Equal(distance, LocationExtensions.CalculateDistance(location1, location2, DistanceUnits.Miles), 3);
            Assert.Equal(distance, LocationExtensions.CalculateDistance(location2, location1, DistanceUnits.Miles), 3);
        }

        [Theory]
        [InlineData(1.0, 101325)]
        [InlineData(1.5, 151987.5)]
        [InlineData(2.0, 202650)]
        [InlineData(2.5, 253312.5)]
        public void AtmospheresToPascals(double atm, double pascal) =>
            Assert.Equal(UnitConverters.AtmospheresToPascals(atm), pascal);

        [Theory]
        [InlineData(101325, 1.0)]
        [InlineData(151987.5, 1.5)]
        [InlineData(202650, 2.0)]
        [InlineData(253312.5, 2.5)]
        public void PascalsToAtmospheres(double pascal, double atm) =>
            Assert.Equal(UnitConverters.PascalsToAtmospheres(pascal), atm);

        [Theory]
        [InlineData(3048, 10000)]
        public void MetersToInternationalFeet(double meters, double internationalFeet)
        {
            Assert.Equal(internationalFeet, UnitConverters.MetersToInternationalFeet(meters));
        }

        [Theory]
        [InlineData(20000, 6096)]
        public void InternationalFeetToMeters(double internationalFeet, double meters)
        {
            Assert.Equal(meters, UnitConverters.InternationalFeetToMeters(internationalFeet));
        }

        [Theory]
        [InlineData(1200, 3937)]
        public void MetersToUSSurveyFeet(double meters, double usFeet)
        {
            Assert.Equal(usFeet, UnitConverters.MetersToUSSurveyFeet(meters));
        }

        [Theory]
        [InlineData(7874, 2400)]
        public void USSurveyFeetToMeters(double usFeet, double meters)
        {
            Assert.Equal(meters, UnitConverters.USSurveyFeetToMeters(usFeet));
        }
    }
}
