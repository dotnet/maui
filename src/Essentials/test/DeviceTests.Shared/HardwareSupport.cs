namespace DeviceTests
{
    static class HardwareSupport
    {
        public static bool HasAccelerometer =>
#if __ANDROID__
            // android emulates the accelerometer
            true;
#elif __IOS__
            // all iOS devices (and only devices) have an accelerometer
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // UWP does not emulate, and only some devices have, an accelerometer
            Windows.Devices.Sensors.Accelerometer.GetDefault() != null;
#endif

        public static bool HasMagnetometer =>
#if __ANDROID__
            // android emulates the magnetometer
            true;
#elif __IOS__
            // all iOS devices (and only devices) have a magnetometer
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // UWP does not emulate, and only some devices have, a magnetometer
            Windows.Devices.Sensors.Magnetometer.GetDefault() != null;
#endif

        public static bool HasGyroscope =>
#if __ANDROID__
            // Android emulators and devices have gyros
            Xamarin.Essentials.Platform.SensorManager?.GetDefaultSensor(Android.Hardware.SensorType.Gyroscope) != null;
#elif __IOS__
            // all iOS devices (and only devices) have a gyroscope
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // UWP does not emulate, and only some devices have, a gyroscope
            Windows.Devices.Sensors.Gyrometer.GetDefault() != null;
#endif

        public static bool HasCompass =>
#if __ANDROID__
            // android emulates the compass
            true;
#elif __IOS__
            // all iOS devices (and only devices) have a compass
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // UWP does not emulate, and only some devices have, a compass
            Windows.Devices.Sensors.Compass.GetDefault() != null;
#endif

        public static bool HasBattery =>
#if __ANDROID__
            // android emulates the battery
            true;
#elif __IOS__
            // all iOS devices (and only devices) have a battery
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // UWP appears to emulate a battery
            // TODO: verify this
            true;
#endif

        public static bool HasFlash =>
#if __ANDROID__
            // TODO: android emulates the lamp, I think...
            Xamarin.Essentials.Platform.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCameraFlash);
#elif __IOS__
            // all iOS devices (and only devices) have a camera
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // TODO: most UWP devices don't have a camera lamp (mobile devices do, we we don't care about those)
            false;
#endif

        public static bool HasBarometer =>
#if __ANDROID__
            true;
#elif __IOS__
            // iphone 6 and never have a barometer. looking in how to test this.
            Xamarin.Essentials.DeviceInfo.DeviceType == Xamarin.Essentials.DeviceType.Physical;
#elif WINDOWS_UWP
            // TODO: most UWP devices don't have a barometer (mobile devices do, we we don't care about those)
            false;
#endif
    }
}
