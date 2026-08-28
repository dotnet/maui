using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Maui.Resizetizer
{
	internal sealed class WindowsStaleOutputDeletionSession : IStaleOutputDeletionSession
	{
		const uint DeleteAccess = 0x00010000;
		const uint FileListDirectory = 0x00000001;
		const uint FileReadAttributes = 0x00000080;
		const uint Synchronize = 0x00100000;
		const uint FileShareRead = 0x00000001;
		const uint FileShareWrite = 0x00000002;
		const uint OpenExisting = 3;
		const uint FileFlagBackupSemantics = 0x02000000;
		const uint FileOpen = 1;
		const uint FileDirectoryFile = 0x00000001;
		const uint FileSynchronousIoNonAlert = 0x00000020;
		const uint FileNonDirectoryFile = 0x00000040;
		const uint FileOpenReparsePoint = 0x00200000;
		const uint ObjectCaseInsensitive = 0x00000040;
		const int FileDispositionInfo = 4;

		readonly SafeFileHandle root;

		WindowsStaleOutputDeletionSession(SafeFileHandle root, string rootPath)
		{
			this.root = root;
			RootPath = rootPath;
		}

		public string RootPath { get; }

		public static WindowsStaleOutputDeletionSession TryOpen(string rootPath, out string error)
		{
			var handle = CreateFile(
				rootPath,
				FileListDirectory | FileReadAttributes,
				FileShareRead | FileShareWrite,
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

			return new WindowsStaleOutputDeletionSession(handle, physicalPath);
		}

		public IValidatedStaleFile TryValidate(string relativePath, out string error)
		{
			var segments = relativePath.Split(Path.DirectorySeparatorChar);
			var parents = new List<SafeFileHandle>();
			var parent = root;

			try
			{
				for (var i = 0; i < segments.Length - 1; i++)
				{
					var next = OpenRelative(
						parent,
						segments[i],
						FileListDirectory | FileReadAttributes | Synchronize,
						FileDirectoryFile | FileOpenReparsePoint | FileSynchronousIoNonAlert,
						out error);

					if (next is null)
						return null;

					if (!TryGetInformation(next, out var information, out error))
					{
						next.Dispose();
						return null;
					}

					if ((information.FileAttributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
					{
						error = $"The parent component '{segments[i]}' is a reparse point.";
						next.Dispose();
						return null;
					}

					parents.Add(next);
					parent = next;
				}

				var leaf = OpenRelative(
					parent,
					segments[segments.Length - 1],
					DeleteAccess | FileReadAttributes | Synchronize,
					FileNonDirectoryFile | FileOpenReparsePoint | FileSynchronousIoNonAlert,
					out error);

				if (leaf is null)
					return null;

				if (!TryGetInformation(leaf, out var leafInformation, out error))
				{
					leaf.Dispose();
					return null;
				}

				if ((leafInformation.FileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
				{
					error = "The candidate is a directory.";
					leaf.Dispose();
					return null;
				}

				var result = new WindowsValidatedStaleFile(parents, leaf);
				parents = null;
				return result;
			}
			finally
			{
				if (parents is not null)
				{
					foreach (var handle in parents)
						handle.Dispose();
				}
			}
		}

		public void Dispose() => root.Dispose();

		static SafeFileHandle OpenRelative(
			SafeFileHandle parent,
			string name,
			uint desiredAccess,
			uint options,
			out string error)
		{
			var nameBuffer = Marshal.StringToHGlobalUni(name);
			var nameStructure = IntPtr.Zero;
			var parentRef = false;

			try
			{
				var unicodeName = new UnicodeString
				{
					Length = checked((ushort)(name.Length * sizeof(char))),
					MaximumLength = checked((ushort)(name.Length * sizeof(char))),
					Buffer = nameBuffer,
				};

				nameStructure = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnicodeString)));
				Marshal.StructureToPtr(unicodeName, nameStructure, fDeleteOld: false);

				parent.DangerousAddRef(ref parentRef);
				var attributes = new ObjectAttributes
				{
					Length = Marshal.SizeOf(typeof(ObjectAttributes)),
					RootDirectory = parent.DangerousGetHandle(),
					ObjectName = nameStructure,
					Attributes = ObjectCaseInsensitive,
				};

				var status = NtCreateFile(
					out var rawHandle,
					desiredAccess,
					ref attributes,
					out _,
					IntPtr.Zero,
					0,
					FileShareRead | FileShareWrite,
					FileOpen,
					options,
					IntPtr.Zero,
					0);

				if (status < 0)
				{
					error = GetNtErrorMessage(status);
					return null;
				}

				error = null;
				return new SafeFileHandle(rawHandle, ownsHandle: true);
			}
			finally
			{
				if (parentRef)
					parent.DangerousRelease();

				if (nameStructure != IntPtr.Zero)
					Marshal.FreeHGlobal(nameStructure);

				Marshal.FreeHGlobal(nameBuffer);
			}
		}

		static bool TryGetInformation(
			SafeFileHandle handle,
			out ByHandleFileInformation information,
			out string error)
		{
			if (!GetFileInformationByHandle(handle, out information))
			{
				error = GetErrorMessage();
				return false;
			}

			error = null;
			return true;
		}

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

		static string GetNtErrorMessage(int status)
		{
			var error = unchecked((int)RtlNtStatusToDosError(status));
			return $"{new Win32Exception(error).Message} ({error}; NTSTATUS 0x{status:X8})";
		}

		sealed class WindowsValidatedStaleFile : IValidatedStaleFile
		{
			readonly List<SafeFileHandle> parents;
			readonly SafeFileHandle handle;

			public WindowsValidatedStaleFile(
				List<SafeFileHandle> parents,
				SafeFileHandle handle)
			{
				this.parents = parents;
				this.handle = handle;
			}

			public StaleFileDeletionResult Delete(Action afterIdentityValidation, out string error)
			{
				afterIdentityValidation?.Invoke();

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

			public void Dispose()
			{
				handle.Dispose();
				foreach (var parent in parents)
					parent.Dispose();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		struct UnicodeString
		{
			public ushort Length;
			public ushort MaximumLength;
			public IntPtr Buffer;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct ObjectAttributes
		{
			public int Length;
			public IntPtr RootDirectory;
			public IntPtr ObjectName;
			public uint Attributes;
			public IntPtr SecurityDescriptor;
			public IntPtr SecurityQualityOfService;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct IoStatusBlock
		{
			public IntPtr Status;
			public IntPtr Information;
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
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		static extern SafeFileHandle CreateFile(
			string path,
			uint desiredAccess,
			uint shareMode,
			IntPtr securityAttributes,
			uint creationDisposition,
			uint flagsAndAttributes,
			IntPtr templateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetFileInformationByHandle(
			SafeFileHandle handle,
			out ByHandleFileInformation information);

		[DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		static extern uint GetFinalPathNameByHandle(
			SafeFileHandle handle,
			[Out] char[] path,
			uint pathLength,
			uint flags);

		[DllImport("kernel32.dll", SetLastError = true)]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetFileInformationByHandle(
			SafeFileHandle handle,
			int informationClass,
			ref FileDispositionInformation information,
			uint bufferSize);

		[DllImport("ntdll.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		static extern int NtCreateFile(
			out IntPtr fileHandle,
			uint desiredAccess,
			ref ObjectAttributes objectAttributes,
			out IoStatusBlock ioStatusBlock,
			IntPtr allocationSize,
			uint fileAttributes,
			uint shareAccess,
			uint createDisposition,
			uint createOptions,
			IntPtr eaBuffer,
			uint eaLength);

		[DllImport("ntdll.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		static extern uint RtlNtStatusToDosError(int status);
	}
}
