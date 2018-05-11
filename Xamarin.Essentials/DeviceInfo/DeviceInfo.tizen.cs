using Plat = Xamarin.Essentials.Platform;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        static string GetModel()
           => Plat.GetSystemInfo("model_name");

        static string GetManufacturer()
            => Plat.GetSystemInfo("manufacturer");

        static string GetDeviceName()
            => Plat.GetSystemInfo("device_name");

        static string GetVersionString()
            => Plat.GetFeatureInfo("platform.version");

        static string GetPlatform()
            => Platforms.Tizen;

        static string GetIdiom()
        {
            var profile = Plat.GetFeatureInfo("profile")?.ToUpperInvariant();

            if (profile == null)
                return Idioms.Unsupported;

            if (profile.StartsWith("M"))
                return Idioms.Phone;
            else if (profile.StartsWith("W"))
                return Idioms.Watch;
            else if (profile.StartsWith("T"))
                return Idioms.TV;
            else
                return Idioms.Unsupported;

            // if (profile.StartsWith("I"))
            //     return Idioms.Car;
        }

        static DeviceType GetDeviceType()
        {
            var arch = Plat.GetFeatureInfo("platform.core.cpu.arch");
            var armv7 = Plat.GetFeatureInfo<bool>("platform.core.cpu.arch.armv7");
            var x86 = Plat.GetFeatureInfo<bool>("platform.core.cpu.arch.x86");

            if (arch != null && arch.Equals("armv7") && armv7 && !x86)
                return DeviceType.Physical;
            else if (arch != null && arch.Equals("x86") && !armv7 && x86)
                return DeviceType.Virtual;
            else
                return DeviceType.Virtual;
        }
    }
}
