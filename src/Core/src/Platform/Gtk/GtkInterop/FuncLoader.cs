using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.GtkInterop
{

	class FuncLoader
	{

		class Windows
		{

			[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

			[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
			public static extern IntPtr LoadLibraryW(string lpszLib);

		}

		class Linux
		{

			[DllImport("libdl.so.2")]
			public static extern IntPtr dlopen(string path, int flags);

			[DllImport("libdl.so.2")]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);

		}

		class OSX
		{

			[DllImport("/usr/lib/libSystem.dylib")]
			public static extern IntPtr dlopen(string path, int flags);

			[DllImport("/usr/lib/libSystem.dylib")]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);

		}

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		const int RTLD_LAZY = 0x0001;
		const int RTLD_GLOBAL = 0x0100;

		public static bool IsWindows, IsOSX;

		static FuncLoader()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					IsWindows = true;

					break;
				case PlatformID.MacOSX:
					IsOSX = true;

					break;
				case PlatformID.Unix:
					try
					{
						var buf = Marshal.AllocHGlobal(8192);

						if (uname(buf) == 0 && Marshal.PtrToStringAnsi(buf) == "Darwin")
							IsOSX = true;

						Marshal.FreeHGlobal(buf);
					}
					catch { }

					break;
			}
		}

		public static IntPtr LoadLibrary(string libname)
		{
			if (IsWindows)
				return Windows.LoadLibraryW(libname);

			if (IsOSX)
				return OSX.dlopen(libname, RTLD_GLOBAL | RTLD_LAZY);

			return Linux.dlopen(libname, RTLD_GLOBAL | RTLD_LAZY);
		}

		public static IntPtr GetProcAddress(IntPtr library, string function)
		{
			var ret = IntPtr.Zero;

			if (IsWindows)
				ret = Windows.GetProcAddress(library, function);
			else if (IsOSX)
				ret = OSX.dlsym(library, function);
			else
				ret = Linux.dlsym(library, function);

			return ret;
		}

		public static T LoadFunction<T>(IntPtr procaddress)
		{
			return procaddress == IntPtr.Zero ? default(T)! : Marshal.GetDelegateForFunctionPointer<T>(procaddress);

		}

	}

}