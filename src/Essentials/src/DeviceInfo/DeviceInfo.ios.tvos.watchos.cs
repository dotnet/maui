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
	partial class PlatformDeviceInfo
	{
		string GetModel()
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

		string GetManufacturer() => "Apple";

		string GetDeviceName() => UIDevice.CurrentDevice.Name;

		string GetVersionString() => UIDevice.CurrentDevice.SystemVersion;

		DevicePlatform GetPlatform() =>
#if __IOS__
			DevicePlatform.iOS;
#elif __TVOS__
			DevicePlatform.tvOS;
#elif __WATCHOS__
			DevicePlatform.watchOS;
#endif

		DeviceIdiom GetIdiom()
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

		DeviceType GetDeviceType()
			=> Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;
	}
}
