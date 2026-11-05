#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Maui.TestUtils.WindowsTesting
{
	public static class WindowsPackageInterop
	{
		public static uint Activate(string appUserModelId, string arguments)
		{
			if (!OperatingSystem.IsWindows())
				throw new PlatformNotSupportedException("MSIX activation requires Windows.");
			var manager = (IApplicationActivationManager)new ApplicationActivationManager();
			try
			{
				var error = manager.ActivateApplication(appUserModelId, arguments, 0, out var processId);
				Marshal.ThrowExceptionForHR(error);
				if (processId == 0)
					throw new InvalidOperationException("Package activation returned PID 0.");
				return processId;
			}
			finally
			{
				Marshal.ReleaseComObject(manager);
			}
		}

		public static string GetProcessPackageFullName(int processId)
		{
			using (var process = Process.GetProcessById(processId))
			{
				uint length = 0;
				var error = GetPackageFullName(process.Handle, ref length, null);
				if (error != 122)
					throw new System.ComponentModel.Win32Exception(error, "Could not read the activated process package identity.");
				var name = new StringBuilder((int)length);
				error = GetPackageFullName(process.Handle, ref length, name);
				if (error != 0)
					throw new System.ComponentModel.Win32Exception(error);
				return name.ToString();
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern int GetPackageFullName(IntPtr process, ref uint length, StringBuilder? name);

		[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IApplicationActivationManager
		{
			[PreserveSig]
			int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
				[MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint processId);
		}

		[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
		class ApplicationActivationManager { }
	}
}
