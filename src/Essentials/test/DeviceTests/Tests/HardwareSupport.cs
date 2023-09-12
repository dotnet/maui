using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	static class HardwareSupport
	{
		public static bool HasAccelerometer =>
#if __ANDROID__
			// android emulates the accelerometer
			true;
#elif MACCATALYST
			false;
#elif __IOS__
			// all iOS devices (and only devices) have an accelerometer
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// UWP does not emulate, and only some devices have, an accelerometer
			global::Windows.Devices.Sensors.Accelerometer.GetDefault() != null;
#endif

		public static bool HasMagnetometer =>
#if __ANDROID__
			// android emulates the magnetometer
			true;
#elif MACCATALYST
			false;
#elif __IOS__
			// all iOS devices (and only devices) have a magnetometer
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// UWP does not emulate, and only some devices have, a magnetometer
			global::Windows.Devices.Sensors.Magnetometer.GetDefault() != null;
#endif

		public static bool HasGyroscope =>
#if __ANDROID__
			// Android emulators and devices have gyros
			Android.App.Application.Context.GetSystemService(Android.Content.Context.SensorService) is Android.Hardware.SensorManager sensorManager &&
			sensorManager.GetDefaultSensor(Android.Hardware.SensorType.Gyroscope) is not null;
#elif MACCATALYST
			false;
#elif __IOS__
			// all iOS devices (and only devices) have a gyroscope
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// UWP does not emulate, and only some devices have, a gyroscope
			global::Windows.Devices.Sensors.Gyrometer.GetDefault() != null;
#endif

		public static bool HasCompass =>
#if __ANDROID__
			// android emulates the compass
			true;
#elif MACCATALYST
			false;
#elif __IOS__
			// all iOS devices (and only devices) have a compass
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// UWP does not emulate, and only some devices have, a compass
			global::Windows.Devices.Sensors.Compass.GetDefault() != null;
#endif

		public static bool HasBattery =>
#if __ANDROID__
			// android emulates the battery
			true;
#elif __IOS__
			// all iOS devices (and only devices) have a battery
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// UWP appears to emulate a battery
			// TODO: verify this
			true;
#endif

		public static bool HasFlash =>
#if __ANDROID__
			// TODO: android emulates the lamp, I think...
			PlatformUtils.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCameraFlash);
#elif MACCATALYST
			false;
#elif __IOS__
			// all iOS devices (and only devices) have a camera
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// TODO: most UWP devices don't have a camera lamp (mobile devices do, we we don't care about those)
			false;
#endif

		public static bool HasBarometer =>
#if __ANDROID__
			true;
#elif MACCATALYST
			false;
#elif __IOS__
			// iphone 6 and never have a barometer. looking in how to test this.
			DeviceInfo.DeviceType == DeviceType.Physical;
#elif WINDOWS_UWP || WINDOWS
			// TODO: most UWP devices don't have a barometer (mobile devices do, we we don't care about those)
			false;
#endif
	}
}
