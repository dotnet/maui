#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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

		public static string GetRegisteredPackagePublisher(string packageFullName)
		{
			if (!OperatingSystem.IsWindows())
				throw new PlatformNotSupportedException("Installed package lookup requires Windows.");
			uint length = 0;
			var error = GetPackagePathByFullName(packageFullName, ref length, null);
			if (error != 122)
				throw new System.ComponentModel.Win32Exception(error, "Could not locate the registered package.");
			var path = new StringBuilder((int)length);
			error = GetPackagePathByFullName(packageFullName, ref length, path);
			if (error != 0)
				throw new System.ComponentModel.Win32Exception(error);
			var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
			using var reader = XmlReader.Create(Path.Combine(path.ToString(), "AppxManifest.xml"), settings);
			var manifest = XDocument.Load(reader);
			var identity = manifest.Root?.Elements().SingleOrDefault(element => element.Name.LocalName == "Identity");
			return (string?)identity?.Attribute("Publisher") ??
				throw new InvalidDataException("The registered package manifest has no publisher.");
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern int GetPackagePathByFullName(string packageFullName, ref uint length, StringBuilder? path);

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
