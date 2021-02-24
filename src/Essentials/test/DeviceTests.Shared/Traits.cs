using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Essentials;
using RuntimeDeviceType = Microsoft.Maui.Essentials.DeviceType;
using XUnitFilter = UnitTests.HeadlessRunner.Xunit.XUnitFilter;

namespace DeviceTests
{
	static class Traits
	{
		public const string DeviceType = "DeviceType";
		public const string InteractionType = "InteractionType";
		public const string UI = "UI";
		public const string FileProvider = "FileProvider";

		internal static class Hardware
		{
			public const string Accelerometer = "HardwareAccelerometer";
			public const string Barometer = "HardwareBarometer";
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

			internal static string ToExclude(bool hasFeature) =>
				hasFeature ? NotSupported : Supported;
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

		internal static IEnumerable<string> GetSkipTraits(IEnumerable<string> additionalFilters = null)
		{
			yield return $"{DeviceType}={DeviceTypes.ToExclude}";
			yield return $"{InteractionType}={InteractionTypes.ToExclude}";
			yield return $"{UI}={FeatureSupport.ToExclude(false)}";
			yield return $"{Hardware.Accelerometer}={FeatureSupport.ToExclude(HardwareSupport.HasAccelerometer)}";
			yield return $"{Hardware.Compass}={FeatureSupport.ToExclude(HardwareSupport.HasCompass)}";
			yield return $"{Hardware.Gyroscope}={FeatureSupport.ToExclude(HardwareSupport.HasGyroscope)}";
			yield return $"{Hardware.Magnetometer}={FeatureSupport.ToExclude(HardwareSupport.HasMagnetometer)}";
			yield return $"{Hardware.Battery}={FeatureSupport.ToExclude(HardwareSupport.HasBattery)}";
			yield return $"{Hardware.Flash}={FeatureSupport.ToExclude(HardwareSupport.HasFlash)}";

			if (additionalFilters != null)
			{
				foreach (var filter in additionalFilters)
				{
					yield return filter;
				}
			}
		}
	}
}
