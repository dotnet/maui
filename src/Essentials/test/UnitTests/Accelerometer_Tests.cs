using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class Accelerometer_Tests
	{
		[Fact]
		public void Accelerometer_Start() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.Stop());

		[Fact]
		public void Accelerometer_Stop() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.Start(SensorSpeed.Default));

		[Fact]
		public void Accelerometer_IsMonitoring() =>
			Assert.False(Accelerometer.IsMonitoring);

		[Fact]
		public void Accelerometer_IsSupported() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.IsSupported);

		[Theory]
		[InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, true)]
		[InlineData(0.0, 0.0, 0.0, 1.0, 0.0, 0.0, false)]
		[InlineData(0.0, 0.0, 0.0, 0.0, 1.0, 0.0, false)]
		[InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, false)]
		public void Accelerometer_Comparison(
			  float x1,
			  float y1,
			  float z1,
			  float x2,
			  float y2,
			  float z2,
			  bool equals)
		{
			var data1 = new AccelerometerData(x1, y1, z1);
			var data2 = new AccelerometerData(x2, y2, z2);
			if (equals)
			{
				Assert.True(data1.Equals(data2));
				Assert.True(data1 == data2);
				Assert.False(data1 != data2);
				Assert.Equal(data1, data2);
				Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
			}
			else
			{
				Assert.False(data1.Equals(data2));
				Assert.False(data1 == data2);
				Assert.True(data1 != data2);
				Assert.NotEqual(data1, data2);
				Assert.NotEqual(data1.GetHashCode(), data2.GetHashCode());
			}
		}

		[Fact]
		public void InitialShaking()
		{
			var q = new AccelerometerQueue();
			Assert.False(q.IsShaking);
		}

		[Fact]
		public void ShakingTests()
		{
			var now = DateTime.UtcNow;
			var q = new AccelerometerQueue();
			q.Add(GetShakeTime(now, 0), false);
			q.Add(GetShakeTime(now, .3), false);
			q.Add(GetShakeTime(now, .6), false);
			q.Add(GetShakeTime(now, .9), false);
			Assert.False(q.IsShaking);

			// The oldest two entries will be removed.
			q.Add(GetShakeTime(now, 1.2), true);
			q.Add(GetShakeTime(now, 1.5), true);
			Assert.False(q.IsShaking);

			// Another entry should be removed, now 3 out of 4 are true.
			q.Add(GetShakeTime(now, 1.8), true);
			Assert.True(q.IsShaking);

			q.Add(GetShakeTime(now, 2.1), false);
			Assert.True(q.IsShaking);

			q.Add(GetShakeTime(now, 2.4), false);
			Assert.False(q.IsShaking);
		}

		[Fact]
		public void ClearQueue()
		{
			var now = DateTime.UtcNow;
			var q = new AccelerometerQueue();
			q.Add(GetShakeTime(now, 0), true);
			q.Add(GetShakeTime(now, .1), true);
			q.Add(GetShakeTime(now, .3), true);
			Assert.True(q.IsShaking);
			q.Clear();
			Assert.False(q.IsShaking);
		}

		long GetShakeTime(DateTime now, double seconds) =>
			Nanoseconds(now.AddSeconds(seconds));

		static long Nanoseconds(DateTime time) =>
				(time.Ticks / TimeSpan.TicksPerMillisecond) * 1_000_000;

		[Fact]
		public void BaseInterfacesWork_IDeviceCapability()
		{
			var stub = new StubAccelerometer();
			IDeviceCapability deviceCapability = stub;
			IAccelerometer accelerometer = stub;

			stub.IsSupported = true;

			Assert.True(accelerometer.IsSupported);
			Assert.True(deviceCapability.IsSupported);

			stub.IsSupported = false;

			Assert.False(accelerometer.IsSupported);
			Assert.False(deviceCapability.IsSupported);
		}

		[Fact]
		public void BaseInterfacesWork_ISensor()
		{
			var stub = new StubAccelerometer();
			ISensor sensor = stub;
			IAccelerometer accelerometer = stub;

			stub.Start(SensorSpeed.Default);

			Assert.True(sensor.IsMonitoring);
			Assert.True(accelerometer.IsMonitoring);

			stub.Stop();

			Assert.False(sensor.IsMonitoring);
			Assert.False(accelerometer.IsMonitoring);
		}

		class StubAccelerometer : IAccelerometer
		{
			public bool IsSupported { get; set; }

			public bool IsMonitoring  { get; set; }

#pragma warning disable CS0067 // The event is never used
			public event EventHandler<AccelerometerChangedEventArgs> ReadingChanged;
#pragma warning restore CS0067 // The event is never used

#pragma warning disable CS0067 // The event is never used
			public event EventHandler ShakeDetected;
#pragma warning restore CS0067 // The event is never used

			public void Start(SensorSpeed sensorSpeed) => IsMonitoring = true;
	
			public void Stop() => IsMonitoring = false;
		}
	}
}
