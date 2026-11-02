using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Microsoft.Maui.Resizetizer.Tests
{
	/// <summary>
	/// A junction is the other kind of Windows directory redirection a project can sit behind, and unlike
	/// a symbolic link it does not need Developer Mode or elevation. .NET has no API to create one, so
	/// the reparse point is written through the Win32 device control interface.
	/// </summary>
	[SupportedOSPlatform("windows")]
	static class Junction
	{
		const uint FsctlSetReparsePoint = 0x000900A4;
		const uint IoReparseTagMountPoint = 0xA0000003;
		const uint GenericWrite = 0x40000000;
		const uint FileShareAll = 0x00000001 | 0x00000002 | 0x00000004;
		const uint OpenExisting = 3;
		const uint FileFlagBackupSemantics = 0x02000000;
		const uint FileFlagOpenReparsePoint = 0x00200000;

		public static bool TryCreate(string junction, string target, out string error)
		{
			try
			{
				Create(junction, target);
				error = null;
				return true;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or PlatformNotSupportedException)
			{
				error = ex.Message;
				return false;
			}
		}

		static void Create(string junction, string target)
		{
			Directory.CreateDirectory(junction);

			// A mount point stores its target as an NT object path.
			var printName = Path.GetFullPath(target);
			var substituteName = @"\??\" + printName;

			var substituteBytes = System.Text.Encoding.Unicode.GetBytes(substituteName);
			var printBytes = System.Text.Encoding.Unicode.GetBytes(printName);

			// REPARSE_DATA_BUFFER: 8 byte header, 8 bytes of mount point offsets/lengths, then the two
			// null terminated names.
			var pathBufferLength = substituteBytes.Length + 2 + printBytes.Length + 2;
			var buffer = new byte[8 + 8 + pathBufferLength];

			BitConverter.GetBytes(IoReparseTagMountPoint).CopyTo(buffer, 0);
			BitConverter.GetBytes((ushort)(8 + pathBufferLength)).CopyTo(buffer, 4);
			BitConverter.GetBytes((ushort)0).CopyTo(buffer, 6);
			BitConverter.GetBytes((ushort)0).CopyTo(buffer, 8);
			BitConverter.GetBytes((ushort)substituteBytes.Length).CopyTo(buffer, 10);
			BitConverter.GetBytes((ushort)(substituteBytes.Length + 2)).CopyTo(buffer, 12);
			BitConverter.GetBytes((ushort)printBytes.Length).CopyTo(buffer, 14);
			substituteBytes.CopyTo(buffer, 16);
			printBytes.CopyTo(buffer, 16 + substituteBytes.Length + 2);

			using var handle = CreateFile(
				junction,
				GenericWrite,
				FileShareAll,
				IntPtr.Zero,
				OpenExisting,
				FileFlagBackupSemantics | FileFlagOpenReparsePoint,
				IntPtr.Zero);

			if (handle.IsInvalid)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			if (!DeviceIoControl(handle, FsctlSetReparsePoint, buffer, buffer.Length, IntPtr.Zero, 0, out _, IntPtr.Zero))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool DeviceIoControl(
			Microsoft.Win32.SafeHandles.SafeFileHandle hDevice,
			uint dwIoControlCode,
			byte[] lpInBuffer,
			int nInBufferSize,
			IntPtr lpOutBuffer,
			int nOutBufferSize,
			out int lpBytesReturned,
			IntPtr lpOverlapped);
	}
}
