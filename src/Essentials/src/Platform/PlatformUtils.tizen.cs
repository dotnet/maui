#pragma warning disable CS0618 // Type or member is obsolete
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Maps;
using Tizen.Sensor;
using Tizen.System;
using TizenAccelerometer = Tizen.Sensor.Accelerometer;
using TizenBarometer = Tizen.Sensor.PressureSensor;
using TizenCompass = Tizen.Sensor.OrientationSensor;
using TizenGyroscope = Tizen.Sensor.Gyroscope;
using TizenMagnetometer = Tizen.Sensor.Magnetometer;
using TizenOrientationSensor = Tizen.Sensor.RotationVectorSensor;

namespace Microsoft.Maui.ApplicationModel
{
	static class PlatformUtils
	{
		static TizenAccelerometer accelerometer = null;
		static TizenBarometer barometer = null;
		static TizenCompass compass = null;
		static TizenGyroscope gyroscope = null;
		static TizenMagnetometer magnetometer = null;
		static TizenOrientationSensor orientationSensor = null;
		static MapService mapService = null;

		internal static string GetSystemInfo(string item) => GetSystemInfo<string>(item);

		internal static T GetSystemInfo<T>(string item)
		{
			Information.TryGetValue<T>($"http://tizen.org/system/{item}", out var value);
			return value;
		}

		internal static string GetFeatureInfo(string item) => GetFeatureInfo<string>(item);

		internal static T GetFeatureInfo<T>(string item)
		{
			Information.TryGetValue<T>($"http://tizen.org/feature/{item}", out var value);
			return value;
		}

		internal static Sensor GetDefaultSensor(SensorType type)
		{
			switch (type)
			{
				case SensorType.Accelerometer:
					if (PlatformUtils.accelerometer == null)
						PlatformUtils.accelerometer = new TizenAccelerometer();
					return PlatformUtils.accelerometer;
				case SensorType.Barometer:
					if (PlatformUtils.barometer == null)
						PlatformUtils.barometer = new TizenBarometer();
					return PlatformUtils.barometer;
				case SensorType.Compass:
					if (PlatformUtils.compass == null)
						PlatformUtils.compass = new TizenCompass();
					return PlatformUtils.compass;
				case SensorType.Gyroscope:
					if (PlatformUtils.gyroscope == null)
						PlatformUtils.gyroscope = new TizenGyroscope();
					return PlatformUtils.gyroscope;
				case SensorType.Magnetometer:
					if (PlatformUtils.magnetometer == null)
						PlatformUtils.magnetometer = new TizenMagnetometer();
					return PlatformUtils.magnetometer;
				case SensorType.OrientationSensor:
					if (PlatformUtils.orientationSensor == null)
						PlatformUtils.orientationSensor = new TizenOrientationSensor();
					return PlatformUtils.orientationSensor;
				default:
					return null;
			}
		}

		internal static async Task<MapService> GetMapServiceAsync(string key)
		{
			if (mapService == null)
			{
				mapService = new MapService("HERE", key);
				await mapService.RequestUserConsent();
			}
			return mapService;
		}
	}

	enum SensorType
	{
		Accelerometer,
		Barometer,
		Compass,
		Gyroscope,
		Magnetometer,
		OrientationSensor
	}
}
