using System;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        public static string Model => GetModel();

        public static string Manufacturer => GetManufacturer();

        public static string Name => GetDeviceName();

        public static string VersionString => GetVersionString();

        public static Version Version => Utils.ParseVersion(VersionString);

        public static string Platform => GetPlatform();

        public static string Idiom => GetIdiom();

        public static DeviceType DeviceType => GetDeviceType();

        public static class Idioms
        {
            // try to match Xamarin.Forms:
            // https://github.com/xamarin/Xamarin.Forms/blob/2.5.1/Xamarin.Forms.Core/TargetIdiom.cs

            public const string Phone = "Phone";
            public const string Tablet = "Tablet";
            public const string Desktop = "Desktop";
            public const string TV = "TV";
            public const string Watch = "Watch";

            public const string Unsupported = "Unsupported";
        }

        public static class Platforms
        {
            // try to match Xamarin.Forms:
            // https://github.com/xamarin/Xamarin.Forms/blob/2.5.1/Xamarin.Forms.Core/Device.cs#L14-L19

            public const string iOS = "iOS";
            public const string Android = "Android";
            public const string UWP = "UWP";
            public const string Tizen = "Tizen";

            public const string Unsupported = "Unsupported";
        }
    }

    public enum DeviceType
    {
        Physical,
        Virtual
    }
}
