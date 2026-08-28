using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Maui.Resizetizer
{
	internal sealed class UnixStaleOutputDeletionSession : IStaleOutputDeletionSession
	{
		const int MacOCloseExec = 0x01000000;
		const int MacODirectory = 0x00100000;
		const int MacONoFollow = 0x00000100;
		const int MacOSymlink = 0x00200000;
		const uint MacRenameExclusive = 0x00000004;
		const uint MacRenameNoFollowAny = 0x00000010;
		const uint MacRenameResolveBeneath = 0x00000020;
		const int LinuxOCloseExec = 0x00080000;
		const int LinuxODirectory = 0x00010000;
		const int LinuxONoFollow = 0x00020000;
		const int LinuxOPath = 0x00200000;
		const uint LinuxRenameNoReplace = 0x00000001;

		readonly SafeUnixHandle root;

		UnixStaleOutputDeletionSession(SafeUnixHandle root, string rootPath)
		{
			this.root = root;
			RootPath = rootPath;
		}

		public string RootPath { get; }

		public static UnixStaleOutputDeletionSession TryOpen(string rootPath, out string error)
		{
			var handle = Open(rootPath, RootOpenFlags, 0);
			if (handle.IsInvalid)
			{
				error = GetErrorMessage();
				handle.Dispose();
				return null;
			}

			string physicalPath;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				physicalPath = new PathCanonicalizer().CanonicalizeDirectory(rootPath);
				if (physicalPath is null)
				{
					error = $"The root '{rootPath}' could not be canonicalized.";
					handle.Dispose();
					return null;
				}
			}
			else if (!TryGetLinuxPath(handle, out physicalPath, out error))
			{
				handle.Dispose();
				return null;
			}

			error = null;
			return new UnixStaleOutputDeletionSession(handle, physicalPath);
		}

		public IValidatedStaleFile TryValidate(string relativePath, out string error)
		{
			error = null;
			var segments = relativePath.Split(Path.DirectorySeparatorChar);
			if (segments.Length == 0)
			{
				error = "The candidate has no path components.";
				return null;
			}

			var parent = Duplicate(root);
			if (parent.IsInvalid)
			{
				error = GetErrorMessage();
				parent.Dispose();
				return null;
			}

			try
			{
				for (var i = 0; i < segments.Length - 1; i++)
				{
					var next = OpenAt(parent, segments[i], ParentOpenFlags, 0);
					if (next.IsInvalid)
					{
						error = GetErrorMessage();
						next.Dispose();
						return null;
					}

					parent.Dispose();
					parent = next;
				}

				var leaf = OpenAt(parent, segments[segments.Length - 1], LeafOpenFlags, 0);
				if (leaf.IsInvalid)
				{
					error = GetErrorMessage();
					leaf.Dispose();
					return null;
				}

				DarwinFileIdentity? identity = null;
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					if (!TryGetDarwinIdentity(leaf, out var darwinIdentity, out error))
					{
						leaf.Dispose();
						return null;
					}

					if (darwinIdentity.IsDirectory)
					{
						error = "The candidate is a directory.";
						leaf.Dispose();
						return null;
					}

					identity = darwinIdentity;
				}
				else if (!TryGetLinuxPath(leaf, out _, out error))
				{
					leaf.Dispose();
					return null;
				}

				var result = new UnixValidatedStaleFile(parent, leaf, segments[segments.Length - 1], identity);
				parent = null;
				return result;
			}
			finally
			{
				parent?.Dispose();
			}
		}

		public void Dispose() => root.Dispose();

		static int RootOpenFlags =>
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
				? MacODirectory | MacOCloseExec
				: LinuxODirectory | LinuxOCloseExec;

		static int ParentOpenFlags =>
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
				? MacODirectory | MacONoFollow | MacOCloseExec
				: LinuxODirectory | LinuxONoFollow | LinuxOCloseExec;

		static int LeafOpenFlags =>
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
				? MacOSymlink | MacOCloseExec
				: LinuxOPath | LinuxONoFollow | LinuxOCloseExec;

		static bool TryGetLinuxPath(SafeUnixHandle handle, out string path, out string error)
		{
			var descriptorPath = $"/proc/self/fd/{handle.DangerousGetHandle().ToInt64()}";
			var buffer = new byte[4096];
			var length = LinuxReadLink(descriptorPath, buffer, new UIntPtr((uint)buffer.Length)).ToInt64();

			if (length < 0)
			{
				path = null;
				error = GetErrorMessage();
				return false;
			}

			if (length == buffer.Length)
			{
				path = null;
				error = $"The descriptor path for '{descriptorPath}' exceeded {buffer.Length} bytes.";
				return false;
			}

			path = Encoding.UTF8.GetString(buffer, 0, (int)length);
			error = null;
			return true;
		}

		static bool TryGetDarwinIdentity(SafeUnixHandle handle, out DarwinFileIdentity identity, out string error)
		{
			if (MacFStat(handle, out var status) != 0)
			{
				identity = default;
				error = GetErrorMessage();
				return false;
			}

			identity = new DarwinFileIdentity(status.Device, status.Inode, status.Mode);
			error = null;
			return true;
		}

		static bool MoveNoReplace(
			SafeUnixHandle parent,
			string source,
			string destination,
			out int nativeError,
			out string error)
		{
			int result;
			try
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					result = MacRenameAt(
						parent,
						source,
						parent,
						destination,
						MacRenameExclusive | MacRenameNoFollowAny | MacRenameResolveBeneath);
				}
				else
				{
					result = LinuxRenameAt(
						parent,
						source,
						parent,
						destination,
						LinuxRenameNoReplace);
				}
			}
			catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
			{
				nativeError = 0;
				error = ex.Message;
				return false;
			}

			if (result == 0)
			{
				nativeError = 0;
				error = null;
				return true;
			}

			nativeError = Marshal.GetLastWin32Error();
			error = GetErrorMessage(nativeError);
			return false;
		}

		static string GetErrorMessage()
		{
			var error = Marshal.GetLastWin32Error();
			return GetErrorMessage(error);
		}

		static string GetErrorMessage(int error) =>
			$"{new Win32Exception(error).Message} ({error})";

		static bool IsAtomicRenameUnavailable(int error)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return error == 22 || error == 45 || error == 78 || error == 102;

			return error == 22 || error == 38 || error == 95;
		}

		static SafeUnixHandle Duplicate(SafeUnixHandle handle) =>
			UnixDuplicate(handle);

		sealed class UnixValidatedStaleFile : IValidatedStaleFile
		{
			readonly SafeUnixHandle parent;
			readonly SafeUnixHandle leaf;
			readonly string leafName;
			readonly DarwinFileIdentity? darwinIdentity;

			public UnixValidatedStaleFile(
				SafeUnixHandle parent,
				SafeUnixHandle leaf,
				string leafName,
				DarwinFileIdentity? darwinIdentity)
			{
				this.parent = parent;
				this.leaf = leaf;
				this.leafName = leafName;
				this.darwinIdentity = darwinIdentity;
			}

			public StaleFileDeletionResult Delete(out string error)
			{
				var temporaryName = $".maui-resizetizer-delete-{Guid.NewGuid():N}";
				if (!MoveNoReplace(parent, leafName, temporaryName, out var nativeError, out var moveError))
				{
					if (nativeError == 2)
					{
						error = null;
						return StaleFileDeletionResult.Changed;
					}

					if (nativeError == 0 || IsAtomicRenameUnavailable(nativeError))
					{
						error = moveError;
						return StaleFileDeletionResult.Unsupported;
					}

					error = moveError;
					return StaleFileDeletionResult.Failed;
				}

				if (!IsOriginalLeaf(temporaryName, out error))
				{
					if (!Restore(temporaryName))
					{
						error = $"The leaf changed identity and its replacement could not be restored from '{temporaryName}'.";
						return StaleFileDeletionResult.Failed;
					}

					return StaleFileDeletionResult.Changed;
				}

				if (UnixUnlinkAt(parent, temporaryName, 0) == 0)
				{
					error = null;
					return StaleFileDeletionResult.Deleted;
				}

				var unlinkError = GetErrorMessage();
				Restore(temporaryName);
				error = unlinkError;
				return StaleFileDeletionResult.Failed;
			}

			public void Dispose()
			{
				leaf.Dispose();
				parent.Dispose();
			}

			bool Restore(string temporaryName) =>
				MoveNoReplace(parent, temporaryName, leafName, out _, out _);

			bool IsOriginalLeaf(string temporaryName, out string error)
			{
				if (darwinIdentity is DarwinFileIdentity expected)
				{
					using var moved = OpenAt(parent, temporaryName, LeafOpenFlags, 0);
					if (moved.IsInvalid)
					{
						error = GetErrorMessage();
						return false;
					}

					if (!TryGetDarwinIdentity(moved, out var actual, out error))
						return false;

					error = null;
					return actual.Equals(expected);
				}

				if (!TryGetLinuxPath(parent, out var parentPath, out error) ||
					!TryGetLinuxPath(leaf, out var leafPath, out error))
				{
					return false;
				}

				error = null;
				return string.Equals(Path.Combine(parentPath, temporaryName), leafPath, StringComparison.Ordinal);
			}
		}

		readonly struct DarwinFileIdentity : IEquatable<DarwinFileIdentity>
		{
			const ushort FileTypeMask = 0xF000;
			const ushort DirectoryType = 0x4000;

			public DarwinFileIdentity(int device, ulong inode, ushort mode)
			{
				Device = device;
				Inode = inode;
				Mode = mode;
			}

			public int Device { get; }

			public ulong Inode { get; }

			public ushort Mode { get; }

			public bool IsDirectory => (Mode & FileTypeMask) == DirectoryType;

			public bool Equals(DarwinFileIdentity other) =>
				Device == other.Device && Inode == other.Inode;

			public override bool Equals(object obj) =>
				obj is DarwinFileIdentity other && Equals(other);

			public override int GetHashCode() =>
				(Device, Inode).GetHashCode();
		}

		[StructLayout(LayoutKind.Explicit, Size = 144)]
		struct DarwinStat
		{
			[FieldOffset(0)]
			public int Device;

			[FieldOffset(4)]
			public ushort Mode;

			[FieldOffset(8)]
			public ulong Inode;
		}

		sealed class SafeUnixHandle : SafeHandleMinusOneIsInvalid
		{
			public SafeUnixHandle()
				: base(ownsHandle: true)
			{
			}

			protected override bool ReleaseHandle() =>
				UnixClose(handle) == 0;
		}

		[DllImport("libc", EntryPoint = "open", SetLastError = true)]
		static extern SafeUnixHandle Open(
			[MarshalAs(UnmanagedType.LPStr)] string path,
			int flags,
			int mode);

		[DllImport("libc", EntryPoint = "openat", SetLastError = true)]
		static extern SafeUnixHandle OpenAt(
			SafeUnixHandle directory,
			[MarshalAs(UnmanagedType.LPStr)] string path,
			int flags,
			int mode);

		[DllImport("libc", EntryPoint = "dup", SetLastError = true)]
		static extern SafeUnixHandle UnixDuplicate(SafeUnixHandle handle);

		[DllImport("libc", EntryPoint = "close", SetLastError = true)]
		static extern int UnixClose(IntPtr handle);

		[DllImport("libc", EntryPoint = "unlinkat", SetLastError = true)]
		static extern int UnixUnlinkAt(
			SafeUnixHandle directory,
			[MarshalAs(UnmanagedType.LPStr)] string path,
			int flags);

		static int MacFStat(SafeUnixHandle handle, out DarwinStat status) =>
			RuntimeInformation.ProcessArchitecture == Architecture.X64
				? MacFStatInode64(handle, out status)
				: MacFStatDefault(handle, out status);

		[DllImport("libSystem.B.dylib", EntryPoint = "fstat", SetLastError = true)]
		static extern int MacFStatDefault(
			SafeUnixHandle handle,
			out DarwinStat status);

		[DllImport("libSystem.B.dylib", EntryPoint = "fstat$INODE64", SetLastError = true)]
		static extern int MacFStatInode64(
			SafeUnixHandle handle,
			out DarwinStat status);

		[DllImport("libSystem.B.dylib", EntryPoint = "renameatx_np", SetLastError = true)]
		static extern int MacRenameAt(
			SafeUnixHandle sourceDirectory,
			[MarshalAs(UnmanagedType.LPStr)] string source,
			SafeUnixHandle destinationDirectory,
			[MarshalAs(UnmanagedType.LPStr)] string destination,
			uint flags);

		[DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
		static extern IntPtr LinuxReadLink(
			[MarshalAs(UnmanagedType.LPStr)] string path,
			[Out] byte[] buffer,
			UIntPtr bufferSize);

		[DllImport("libc", EntryPoint = "renameat2", SetLastError = true)]
		static extern int LinuxRenameAt(
			SafeUnixHandle sourceDirectory,
			[MarshalAs(UnmanagedType.LPStr)] string source,
			SafeUnixHandle destinationDirectory,
			[MarshalAs(UnmanagedType.LPStr)] string destination,
			uint flags);
	}
}
