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

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        static TizenAccelerometer accelerometer = null;
        static TizenBarometer barometer = null;
        static TizenCompass compass = null;
        static TizenGyroscope gyroscope = null;
        static TizenMagnetometer magnetometer = null;
        static TizenOrientationSensor orientationSensor = null;
        static MapService mapService = null;

        public static void Init(ElmSharp.Window window)
        {
            MainWindow = window;
        }

        internal static ElmSharp.Window MainWindow { get; private set; }

        internal static Package CurrentPackage
        {
            get
            {
                var packageId = Application.Current.ApplicationInfo.PackageId;
                return PackageManager.GetPackage(packageId);
            }
        }

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
                    if (Platform.accelerometer == null)
                        Platform.accelerometer = new TizenAccelerometer();
                    return Platform.accelerometer;
                case SensorType.Barometer:
                    if (Platform.barometer == null)
                        Platform.barometer = new TizenBarometer();
                    return Platform.barometer;
                case SensorType.Compass:
                    if (Platform.compass == null)
                        Platform.compass = new TizenCompass();
                    return Platform.compass;
                case SensorType.Gyroscope:
                    if (Platform.gyroscope == null)
                        Platform.gyroscope = new TizenGyroscope();
                    return Platform.gyroscope;
                case SensorType.Magnetometer:
                    if (Platform.magnetometer == null)
                        Platform.magnetometer = new TizenMagnetometer();
                    return Platform.magnetometer;
                case SensorType.OrientationSensor:
                    if (Platform.orientationSensor == null)
                        Platform.orientationSensor = new TizenOrientationSensor();
                    return Platform.orientationSensor;
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

        public static string MapServiceToken { get; set; }
    }

    public enum SensorType
    {
        Accelerometer,
        Barometer,
        Compass,
        Gyroscope,
        Magnetometer,
        OrientationSensor
    }
}
