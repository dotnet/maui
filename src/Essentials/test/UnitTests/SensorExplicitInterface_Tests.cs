using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class SensorExplicitInterface_Tests
	{
		[Fact]
		public void Compass_CanCastToISensor()
		{
			var compass = Compass.Default;
			
			// Verify we can cast to ISensor
			Assert.IsAssignableFrom<ISensor>(compass);
			
			var sensor = (ISensor)compass;
			
			// Verify explicit interface implementation delegates correctly
			Assert.Equal(compass.IsMonitoring, sensor.IsMonitoring);
		}

		[Fact]
		public void Compass_CanCastToIDeviceCapabilities()
		{
			var compass = Compass.Default;
			
			// Verify we can cast to IDeviceCapabilities
			Assert.IsAssignableFrom<IDeviceCapabilities>(compass);
		}

		[Fact]
		public void Gyroscope_CanCastToISensor()
		{
			var gyroscope = Gyroscope.Default;
			
			// Verify we can cast to ISensor
			Assert.IsAssignableFrom<ISensor>(gyroscope);
			
			var sensor = (ISensor)gyroscope;
			
			// Verify explicit interface implementation delegates correctly
			Assert.Equal(gyroscope.IsMonitoring, sensor.IsMonitoring);
		}

		[Fact]
		public void Gyroscope_CanCastToIDeviceCapabilities()
		{
			var gyroscope = Gyroscope.Default;
			
			// Verify we can cast to IDeviceCapabilities
			Assert.IsAssignableFrom<IDeviceCapabilities>(gyroscope);
		}

		[Fact]
		public void Accelerometer_CanCastToISensor()
		{
			var accelerometer = Accelerometer.Default;
			
			// Verify we can cast to ISensor
			Assert.IsAssignableFrom<ISensor>(accelerometer);
			
			var sensor = (ISensor)accelerometer;
			
			// Verify explicit interface implementation delegates correctly
			Assert.Equal(accelerometer.IsMonitoring, sensor.IsMonitoring);
		}

		[Fact]
		public void Accelerometer_CanCastToIDeviceCapabilities()
		{
			var accelerometer = Accelerometer.Default;
			
			// Verify we can cast to IDeviceCapabilities
			Assert.IsAssignableFrom<IDeviceCapabilities>(accelerometer);
		}

		[Fact]
		public void Magnetometer_CanCastToISensor()
		{
			var magnetometer = Magnetometer.Default;
			
			// Verify we can cast to ISensor
			Assert.IsAssignableFrom<ISensor>(magnetometer);
			
			var sensor = (ISensor)magnetometer;
			
			// Verify explicit interface implementation delegates correctly
			Assert.Equal(magnetometer.IsMonitoring, sensor.IsMonitoring);
		}

		[Fact]
		public void Magnetometer_CanCastToIDeviceCapabilities()
		{
			var magnetometer = Magnetometer.Default;
			
			// Verify we can cast to IDeviceCapabilities
			Assert.IsAssignableFrom<IDeviceCapabilities>(magnetometer);
		}

		[Fact]
		public void Barometer_CanCastToISensor()
		{
			var barometer = Barometer.Default;
			
			// Verify we can cast to ISensor
			Assert.IsAssignableFrom<ISensor>(barometer);
			
			var sensor = (ISensor)barometer;
			
			// Verify explicit interface implementation delegates correctly
			Assert.Equal(barometer.IsMonitoring, sensor.IsMonitoring);
		}

		[Fact]
		public void Barometer_CanCastToIDeviceCapabilities()
		{
			var barometer = Barometer.Default;
			
			// Verify we can cast to IDeviceCapabilities
			Assert.IsAssignableFrom<IDeviceCapabilities>(barometer);
		}
	}
}