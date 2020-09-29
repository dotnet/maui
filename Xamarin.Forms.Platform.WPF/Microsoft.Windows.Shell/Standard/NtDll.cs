namespace Standard
{
	using System;
	using System.Runtime.InteropServices;

	public static class NtDll
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct OSVERSIONINFOEX
		{
			public uint dwOSVersionInfoSize;
			public uint dwMajorVersion;
			public uint dwMinorVersion;
			public uint dwBuildNumber;
			public uint dwPlatformId;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string szCSDVersion;
			public ushort wServicePackMajor;
			public ushort wServicePackMinor;
			public ushort wSuiteMask;
			public byte wProductType;
			public byte wReserved;
		}

		public static class NativeMethods
		{
			[DllImport("ntdll.dll", CharSet = CharSet.Unicode)]
			public static extern int RtlGetVersion([In, Out] ref OSVERSIONINFOEX version);
		}

		public static Version RtlGetVersion()
		{
			var v = default(OSVERSIONINFOEX);
			v.dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(OSVERSIONINFOEX));
			if (NativeMethods.RtlGetVersion(ref v) == 0)
			{
				return new Version((int)v.dwMajorVersion, (int)v.dwMinorVersion, (int)v.dwBuildNumber, 0);
			}
			// didn't work ???
			return default(Version);//Environment.OSVersion.Version;
		}
	}
}