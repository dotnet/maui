using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using RuntimeDeviceType = Xamarin.Essentials.DeviceType;
using XUnitFilter = UnitTests.HeadlessRunner.Xunit.XUnitFilter;

namespace DeviceTests
{
    static class Traits
    {
        public const string DeviceType = "DeviceType";
        public const string InteractionType = "InteractionType";

        internal static class Hardware
        {
            public const string Accelerometer = "HardwareAccelerometer";
            public const string Compass = "HardwareCompass";
            public const string Gyroscope = "HardwareGyroscope";
            public const string Magnetometer = "HardwareMagnetometer";
            public const string Battery = "HardwareBattery";
            public const string Flash = "HardwareFlash";
        }

        internal static class DeviceTypes
        {
            public const string Physical = "Physical";
            public const string Virtual = "Virtual";

            internal static string ToExclude =>
                DeviceInfo.DeviceType == RuntimeDeviceType.Physical ? Virtual : Physical;
        }

        internal static class InteractionTypes
        {
            public const string Human = "Human";
            public const string Machine = "Machine";

            internal static string ToExclude => Human;
        }

        internal static class FeatureSupport
        {
            public const string Supported = "Supported";
            public const string NotSupported = "NotSupported";

            internal static string ToExclude(bool hardware) =>
                hardware ? NotSupported : Supported;
        }

        internal static List<XUnitFilter> GetCommonTraits(params XUnitFilter[] additionalFilters)
        {
            var filters = new List<XUnitFilter>
            {
                new XUnitFilter(DeviceType, DeviceTypes.ToExclude, true),
                new XUnitFilter(InteractionType, InteractionTypes.ToExclude, true),
                new XUnitFilter(Hardware.Accelerometer, FeatureSupport.ToExclude(HardwareSupport.HasAccelerometer), true),
                new XUnitFilter(Hardware.Compass, FeatureSupport.ToExclude(HardwareSupport.HasCompass), true),
                new XUnitFilter(Hardware.Gyroscope, FeatureSupport.ToExclude(HardwareSupport.HasGyroscope), true),
                new XUnitFilter(Hardware.Magnetometer, FeatureSupport.ToExclude(HardwareSupport.HasMagnetometer), true),
                new XUnitFilter(Hardware.Battery, FeatureSupport.ToExclude(HardwareSupport.HasBattery), true),
                new XUnitFilter(Hardware.Flash, FeatureSupport.ToExclude(HardwareSupport.HasFlash), true),
            };

            if (additionalFilters != null && additionalFilters.Any())
                filters.AddRange(additionalFilters);

            return filters;
        }
    }
}
