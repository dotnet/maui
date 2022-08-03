using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Devices
{
	class DeviceInfoImplementation : IDeviceInfo
	{
		[DllImport(Constants.SystemConfigurationLibrary)]
		static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

		[DllImport(Constants.CoreFoundationLibrary)]
		static extern void CFRelease(IntPtr cf);

		public string Model =>
			IOKit.GetPlatformExpertPropertyValue<NSData>("model")?.ToString() ?? string.Empty;

		public string Manufacturer => "Apple";

		public string Name
		{
			get
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
		}

		public string VersionString
		{
			get
			{
				using var info = new NSProcessInfo();
				return info.OperatingSystemVersion.ToString();
			}
		}

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform => DevicePlatform.macOS;

		public DeviceIdiom Idiom => DeviceIdiom.Desktop;

		public DeviceType DeviceType => DeviceType.Physical;
	}
}
