using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Maui.Resizetizer
{
	internal sealed class WindowsStaleOutputDeletionSession : IStaleOutputDeletionSession
	{
		const uint DeleteAccess = 0x00010000;
		const uint FileReadAttributes = 0x00000080;
		const uint FileShareRead = 0x00000001;
		const uint FileShareWrite = 0x00000002;
		const uint FileShareDelete = 0x00000004;
		const uint OpenExisting = 3;
		const uint FileFlagBackupSemantics = 0x02000000;
		const uint FileFlagOpenReparsePoint = 0x00200000;
		const int FileDispositionInfo = 4;

		readonly SafeFileHandle root;
		readonly string lexicalRoot;

		WindowsStaleOutputDeletionSession(SafeFileHandle root, string rootPath, string lexicalRoot)
		{
			this.root = root;
			this.lexicalRoot = lexicalRoot;
			RootPath = rootPath;
		}

		public string RootPath { get; }

		public static WindowsStaleOutputDeletionSession TryOpen(string rootPath, out string error)
		{
			var handle = CreateFile(
				rootPath,
				FileReadAttributes,
				FileShareRead | FileShareWrite | FileShareDelete,
				IntPtr.Zero,
				OpenExisting,
				FileFlagBackupSemantics,
				IntPtr.Zero);

			if (handle.IsInvalid)
			{
				error = GetErrorMessage();
				handle.Dispose();
				return null;
			}

			if (!TryGetPath(handle, out var physicalPath, out error))
			{
				handle.Dispose();
				return null;
			}

			string lexical;
			try
			{
				lexical = Path.GetFullPath(rootPath);
			}
			catch (Exception ex)
			{
				error = ex.Message;
				handle.Dispose();
				return null;
			}

			return new WindowsStaleOutputDeletionSession(handle, physicalPath, lexical);
		}

		public IValidatedStaleFile TryValidate(string relativePath, out string error)
		{
			var path = Path.Combine(lexicalRoot, relativePath);
			var handle = CreateFile(
				path,
				DeleteAccess | FileReadAttributes,
				FileShareRead | FileShareWrite | FileShareDelete,
				IntPtr.Zero,
				OpenExisting,
				FileFlagBackupSemantics | FileFlagOpenReparsePoint,
				IntPtr.Zero);

			if (handle.IsInvalid)
			{
				error = GetErrorMessage();
				handle.Dispose();
				return null;
			}

			if (!GetFileInformationByHandle(handle, out var information))
			{
				error = GetErrorMessage();
				handle.Dispose();
				return null;
			}

			if ((information.FileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				error = "The candidate is a directory.";
				handle.Dispose();
				return null;
			}

			if (!TryGetPath(root, out var rootPath, out error) ||
				!TryGetPath(handle, out var candidatePath, out error))
			{
				handle.Dispose();
				return null;
			}

			if (!PathCanonicalizer.IsUnder(candidatePath, rootPath))
			{
				error = $"The opened candidate '{candidatePath}' is outside the retained root '{rootPath}'.";
				handle.Dispose();
				return null;
			}

			return new WindowsValidatedStaleFile(handle);
		}

		public void Dispose() => root.Dispose();

		static bool TryGetPath(SafeFileHandle handle, out string path, out string error)
		{
			var buffer = new char[512];
			var length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Length, 0);

			if (length == 0)
			{
				path = null;
				error = GetErrorMessage();
				return false;
			}

			if (length >= buffer.Length)
			{
				buffer = new char[length + 1];
				length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Length, 0);
				if (length == 0 || length >= buffer.Length)
				{
					path = null;
					error = GetErrorMessage();
					return false;
				}
			}

			path = NormalizeDevicePath(new string(buffer, 0, (int)length));
			error = null;
			return true;
		}

		static string NormalizeDevicePath(string path)
		{
			const string uncPrefix = @"\\?\UNC\";
			const string devicePrefix = @"\\?\";

			if (path.StartsWith(uncPrefix, StringComparison.Ordinal))
				return @"\\" + path.Substring(uncPrefix.Length);

			return path.StartsWith(devicePrefix, StringComparison.Ordinal)
				? path.Substring(devicePrefix.Length)
				: path;
		}

		static string GetErrorMessage()
		{
			var error = Marshal.GetLastWin32Error();
			return $"{new Win32Exception(error).Message} ({error})";
		}

		sealed class WindowsValidatedStaleFile : IValidatedStaleFile
		{
			readonly SafeFileHandle handle;

			public WindowsValidatedStaleFile(SafeFileHandle handle)
			{
				this.handle = handle;
			}

			public StaleFileDeletionResult Delete(out string error)
			{
				var disposition = new FileDispositionInformation
				{
					DeleteFile = 1,
				};

				if (!SetFileInformationByHandle(
					handle,
					FileDispositionInfo,
					ref disposition,
					(uint)Marshal.SizeOf(typeof(FileDispositionInformation))))
				{
					error = GetErrorMessage();
					return StaleFileDeletionResult.Failed;
				}

				error = null;
				return StaleFileDeletionResult.Deleted;
			}

			public void Dispose() => handle.Dispose();
		}

		[StructLayout(LayoutKind.Sequential)]
		struct FileDispositionInformation
		{
			public byte DeleteFile;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct ByHandleFileInformation
		{
			public FileAttributes FileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
			public uint VolumeSerialNumber;
			public uint FileSizeHigh;
			public uint FileSizeLow;
			public uint NumberOfLinks;
			public uint FileIndexHigh;
			public uint FileIndexLow;
		}

		[DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern SafeFileHandle CreateFile(
			string path,
			uint desiredAccess,
			uint shareMode,
			IntPtr securityAttributes,
			uint creationDisposition,
			uint flagsAndAttributes,
			IntPtr templateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetFileInformationByHandle(
			SafeFileHandle handle,
			out ByHandleFileInformation information);

		[DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern uint GetFinalPathNameByHandle(
			SafeFileHandle handle,
			[Out] char[] path,
			uint pathLength,
			uint flags);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetFileInformationByHandle(
			SafeFileHandle handle,
			int informationClass,
			ref FileDispositionInformation information,
			uint bufferSize);
	}
}
