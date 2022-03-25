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
		internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

		internal static string GetSystemLibraryProperty(string property)
		{
			var lengthPtr = Marshal.AllocHGlobal(sizeof(int));
			SysctlByName(property, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0);

			var propertyLength = Marshal.ReadInt32(lengthPtr);

			if (propertyLength == 0)
			{
				Marshal.FreeHGlobal(lengthPtr);
				throw new InvalidOperationException("Unable to read length of property.");
			}

			var valuePtr = Marshal.AllocHGlobal(propertyLength);
			SysctlByName(property, valuePtr, lengthPtr, IntPtr.Zero, 0);

			var returnValue = Marshal.PtrToStringAnsi(valuePtr);

			Marshal.FreeHGlobal(lengthPtr);
			Marshal.FreeHGlobal(valuePtr);

			return returnValue;
		}

		internal static void BeginInvokeOnMainThread(Action action)
		{
			NSRunLoop.Main.BeginInvokeOnMainThread(action);
		}
	}
}
