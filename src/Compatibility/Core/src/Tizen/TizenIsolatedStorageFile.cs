using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Internals;
using IOPath = System.IO.Path;
using TApplication = Tizen.Applications.Application;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	internal class TizenIsolatedStorageFile : IIsolatedStorageFile
	{
		readonly string _rootPath;

		internal TizenIsolatedStorageFile()
		{
			_rootPath = TApplication.Current.DirectoryInfo.Data;
		}

		public void CreateDirectory(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			Directory.CreateDirectory(IOPath.Combine(_rootPath, path));
		}

		public Task CreateDirectoryAsync(string path)
		{
			CreateDirectory(path);
			return Task.FromResult(true);
		}

		public void MoveFile(string source, string dest)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (dest == null)
				throw new ArgumentNullException(nameof(dest));

			File.Move(IOPath.Combine(_rootPath, source), IOPath.Combine(_rootPath, dest));
		}

		public void DeleteFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Trim().Length == 0)
				throw new ArgumentException("An empty path is not valid.");

			File.Delete(IOPath.Combine(_rootPath, path));
		}

		public bool DirectoryExists(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			return Directory.Exists(IOPath.Combine(_rootPath, path));
		}

		public Task<bool> GetDirectoryExistsAsync(string path)
		{
			return Task.FromResult(DirectoryExists(path));
		}

		public bool FileExists(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			return File.Exists(IOPath.Combine(_rootPath, path));
		}

		public Task<bool> GetFileExistsAsync(string path)
		{
			return Task.FromResult(FileExists(path));
		}

		public DateTimeOffset GetLastWriteTime(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Trim().Length == 0)
				throw new ArgumentException("An empty path is not valid.");

			string fullPath = IOPath.Combine(_rootPath, path);
			if (File.Exists(fullPath))
				return File.GetLastWriteTime(fullPath);

			return Directory.GetLastWriteTime(fullPath);
		}

		public Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
		{
			return Task.FromResult(GetLastWriteTime(path));
		}

		public Stream OpenFile(string path, FileMode mode)
		{
			return OpenFile(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite));
		}

		public Stream OpenFile(string path, FileMode mode, FileAccess access)
		{
			return OpenFile(path, mode, access, FileShare.Read);
		}

		public Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (path.Trim().Length == 0)
				throw new ArgumentException("An empty path is not valid.");

			string fullPath = IOPath.Combine(_rootPath, path);
			return new FileStream(fullPath, mode, access, share);
		}

		public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access)
		{
			return Task.FromResult(OpenFile(path, mode, access));
		}

		public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return Task.FromResult(OpenFile(path, mode, access, share));
		}

	}
}
