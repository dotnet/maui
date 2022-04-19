using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	class DeviceInfoImplementation : IDeviceInfo
	{
		public string Model => PlatformUtils.GetSystemInfo("model_name");

		public string Manufacturer => PlatformUtils.GetSystemInfo("manufacturer");

		public string Name => PlatformUtils.GetSystemInfo("device_name");

		public string VersionString => PlatformUtils.GetFeatureInfo("platform.version");

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform => DevicePlatform.Tizen;

		public DeviceIdiom Idiom
		{
			get
			{
				var profile = PlatformUtils.GetFeatureInfo("profile")?.ToUpperInvariant();

				if (profile == null)
					return DeviceIdiom.Unknown;

				if (profile.StartsWith("M"))
					return DeviceIdiom.Phone;
				else if (profile.StartsWith("W"))
					return DeviceIdiom.Watch;
				else if (profile.StartsWith("T"))
					return DeviceIdiom.TV;
				else
					return DeviceIdiom.Unknown;
			}
		}

		public DeviceType DeviceType
		{
			get
			{
				var arch = PlatformUtils.GetFeatureInfo("platform.core.cpu.arch");
				var armv7 = PlatformUtils.GetFeatureInfo<bool>("platform.core.cpu.arch.armv7");
				var x86 = PlatformUtils.GetFeatureInfo<bool>("platform.core.cpu.arch.x86");

				if (arch != null && arch.Equals("armv7", StringComparison.Ordinal) && armv7 && !x86)
					return DeviceType.Physical;
				else if (arch != null && arch.Equals("x86", StringComparison.Ordinal) && !armv7 && x86)
					return DeviceType.Virtual;
				else
					return DeviceType.Unknown;
			}
		}
	}
}
