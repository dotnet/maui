using System;
using System.Diagnostics;
#if __WATCHOS__
using WatchKit;
using UIDevice = WatchKit.WKInterfaceDevice;
#else
using UIKit;
#endif

using ObjCRuntime;

namespace Microsoft.Maui.Essentials
{
	public static partial class DeviceInfo
	{
		static string GetModel()
		{
			try
			{
				return Essentials.Platform.GetSystemLibraryProperty("hw.machine");
			}
			catch (Exception)
			{
				Debug.WriteLine("Unable to query hardware model, returning current device model.");
			}
			return UIDevice.CurrentDevice.Model;
		}

		static string GetManufacturer() => "Apple";

		static string GetDeviceName() => UIDevice.CurrentDevice.Name;

		static string GetVersionString() => UIDevice.CurrentDevice.SystemVersion;

		static DevicePlatform GetPlatform() =>
#if __IOS__
			DevicePlatform.iOS;
#elif __TVOS__
            DevicePlatform.tvOS;
#elif __WATCHOS__
            DevicePlatform.watchOS;
#endif

		static DeviceIdiom GetIdiom()
		{
#if __WATCHOS__
            return DeviceIdiom.Watch;
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

		static DeviceType GetDeviceType()
			=> Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;
	}
}
