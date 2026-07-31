using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.ApplicationModel
{
	static class PlatformUtils
	{
#if __IOS__
		[DllImport(Constants.SystemLibrary, EntryPoint = "sysctlbyname")]
#else
		[DllImport(Constants.libSystemLibrary, EntryPoint = "sysctlbyname")]
#endif
		internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, ref nuint oldLen, IntPtr newp, nuint newlen);

		internal static string GetSystemLibraryProperty(string property)
		{
			nuint propertyLength = 0;
			if (SysctlByName(property, IntPtr.Zero, ref propertyLength, IntPtr.Zero, 0) != 0 || propertyLength == 0)
			{
				throw new InvalidOperationException("Unable to read length of property.");
			}

			var valuePtr = Marshal.AllocHGlobal(checked((nint)propertyLength));
			try
			{
				if (SysctlByName(property, valuePtr, ref propertyLength, IntPtr.Zero, 0) != 0)
				{
					throw new InvalidOperationException("Unable to read property.");
				}

				return Marshal.PtrToStringAnsi(valuePtr);
			}
			finally
			{
				Marshal.FreeHGlobal(valuePtr);
			}
		}

		internal static void BeginInvokeOnMainThread(Action action)
		{
			NSRunLoop.Main.BeginInvokeOnMainThread(action);
		}
	}
}
