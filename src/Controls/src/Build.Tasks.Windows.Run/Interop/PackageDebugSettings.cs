using System.Runtime.InteropServices;

namespace Microsoft.Maui.Windows.Run.Interop;

/// <summary>
/// COM interface for controlling packaged app debug settings and process lifecycle.
/// See: https://learn.microsoft.com/windows/win32/api/shobjidl_core/nn-shobjidl_core-ipackagedebugsettings
/// </summary>
[ComImport]
[Guid("F27C3930-8029-4AD1-94E3-3DBA417810C1")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPackageDebugSettings
{
	int EnableDebugging(
		[MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
		[MarshalAs(UnmanagedType.LPWStr)] string? debuggerCommandLine,
		[MarshalAs(UnmanagedType.LPWStr)] string? environment);

	int DisableDebugging(
		[MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

	int Suspend(
		[MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

	int Resume(
		[MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

	int TerminateAllProcesses(
		[MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
}

[ComImport]
[Guid("B1AEC16F-2383-4852-B0E9-8F0B1DC66B4D")]
internal class PackageDebugSettings
{
}
