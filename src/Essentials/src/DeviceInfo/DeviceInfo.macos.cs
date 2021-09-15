using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Essentials
{
	partial class PlatformDeviceInfo
	{
		[DllImport(Constants.SystemConfigurationLibrary)]
		static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

		[DllImport(Constants.CoreFoundationLibrary)]
		static extern void CFRelease(IntPtr cf);

		string GetModel() =>
			IOKit.GetPlatformExpertPropertyValue<NSData>("model")?.ToString() ?? string.Empty;

		string GetManufacturer() => "Apple";

		string GetDeviceName()
		{
			var computerNameHandle = SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero);

			if (computerNameHandle == IntPtr.Zero)
				return null;

			try
			{
#pragma warning disable CS0618 // Type or member is obsolete
				return NSString.FromHandle(computerNameHandle);
#pragma warning restore CS0618 // Type or member is obsolete
			}
			finally
			{
				CFRelease(computerNameHandle);
			}
		}

		string GetVersionString()
		{
			using var info = new NSProcessInfo();
			return info.OperatingSystemVersion.ToString();
		}

		DevicePlatform GetPlatform() => DevicePlatform.macOS;

		DeviceIdiom GetIdiom() => DeviceIdiom.Desktop;

		DeviceType GetDeviceType() => DeviceType.Physical;
	}
}
