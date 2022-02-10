using Plat = Microsoft.Maui.Essentials.Platform;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class DeviceInfoImplementation : IDeviceInfo
	{
		public string Model
			=> Plat.GetSystemInfo("model_name");

		public string Manufacturer
			=> Plat.GetSystemInfo("manufacturer");

		public string Name
			=> Plat.GetSystemInfo("device_name");

		public string VersionString
			=> Plat.GetFeatureInfo("platform.version");

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform
			=> DevicePlatform.Tizen;

		public DeviceIdiom Idiom
		{
			get
			{
				var profile = Plat.GetFeatureInfo("profile")?.ToUpperInvariant();

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
				var arch = Plat.GetFeatureInfo("platform.core.cpu.arch");
				var armv7 = Plat.GetFeatureInfo<bool>("platform.core.cpu.arch.armv7");
				var x86 = Plat.GetFeatureInfo<bool>("platform.core.cpu.arch.x86");

				if (arch != null && arch.Equals("armv7") && armv7 && !x86)
					return DeviceType.Physical;
				else if (arch != null && arch.Equals("x86") && !armv7 && x86)
					return DeviceType.Virtual;
				else
					return DeviceType.Unknown;
			}
		}
	}
}
