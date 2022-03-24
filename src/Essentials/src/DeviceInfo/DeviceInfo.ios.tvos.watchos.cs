using System;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
#if __WATCHOS__
using WatchKit;
using UIDevice = WatchKit.WKInterfaceDevice;
#else
using UIKit;
#endif

using ObjCRuntime;

namespace Microsoft.Maui.Devices
{
	class DeviceInfoImplementation : IDeviceInfo
	{
		public string Model
		{
			get
			{
				try
				{
					return PlatformUtils.GetSystemLibraryProperty("hw.machine");
				}
				catch (Exception)
				{
					Debug.WriteLine("Unable to query hardware model, returning current device model.");
				}
				return UIDevice.CurrentDevice.Model;
			}
		}

		public string Manufacturer => "Apple";

		public string Name => UIDevice.CurrentDevice.Name;

		public string VersionString => UIDevice.CurrentDevice.SystemVersion;

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform =>
#if MACCATALYST
			DevicePlatform.MacCatalyst;
#elif IOS
			DevicePlatform.iOS;
#elif __TVOS__
			DevicePlatform.tvOS;
#elif __WATCHOS__
			DevicePlatform.watchOS;
#endif

		public DeviceIdiom Idiom
		{
			get
			{
#if __WATCHOS__
			return DeviceIdiom.Watch;
#elif MACCATALYST
			return DeviceIdiom.Desktop;
#else
				switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
				{
					case UIUserInterfaceIdiom.Pad:
						return DeviceIdiom.Tablet;
					case UIUserInterfaceIdiom.Phone:
						return DeviceIdiom.Phone;
					case UIUserInterfaceIdiom.TV:
						return DeviceIdiom.TV;
					case UIUserInterfaceIdiom.CarPlay:
					case UIUserInterfaceIdiom.Unspecified:
					default:
						return DeviceIdiom.Unknown;
				}
#endif
			}
		}

		public DeviceType DeviceType =>
#if MACCATALYST || MACOS
			DeviceType.Physical;
#else
			Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;
#endif
	}
}
