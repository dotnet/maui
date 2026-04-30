using System.Runtime.InteropServices;

namespace Microsoft.Maui.Windows.Run.Interop;

/// <summary>
/// COM interface for launching MSIX/UWP apps by their Application User Model ID.
/// See: https://learn.microsoft.com/windows/win32/api/shobjidl_core/nn-shobjidl_core-iapplicationactivationmanager
/// </summary>
[ComImport]
[Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IApplicationActivationManager
{
	int ActivateApplication(
		[MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
		[MarshalAs(UnmanagedType.LPWStr)] string? arguments,
		uint options,
		out uint processId);

	int ActivateForFile(
		[MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
		IntPtr itemArray,
		[MarshalAs(UnmanagedType.LPWStr)] string? verb,
		out uint processId);

	int ActivateForProtocol(
		[MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
		IntPtr itemArray,
		out uint processId);
}

[ComImport]
[Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
internal class ApplicationActivationManager
{
}

/// <summary>
/// ActivateOptions flags for IApplicationActivationManager.ActivateApplication.
/// </summary>
[Flags]
internal enum ActivateOptions : uint
{
	None = 0x0,
	DesignMode = 0x1,
	NoErrorUI = 0x2,
	NoSplashScreen = 0x4,
}
