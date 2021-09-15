using Plat = Microsoft.Maui.Essentials.Platform;

namespace Microsoft.Maui.Essentials
{
	partial class PlatformDeviceInfo
	{
		string GetModel()
			=> Plat.GetSystemInfo("model_name");

		string GetManufacturer()
			=> Plat.GetSystemInfo("manufacturer");

		string GetDeviceName()
			=> Plat.GetSystemInfo("device_name");

		string GetVersionString()
			=> Plat.GetFeatureInfo("platform.version");

		DevicePlatform GetPlatform()
			=> DevicePlatform.Tizen;

		DeviceIdiom GetIdiom()
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

		DeviceType GetDeviceType()
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
