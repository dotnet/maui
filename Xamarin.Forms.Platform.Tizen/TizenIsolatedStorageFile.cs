using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using TApplication = Tizen.Applications.Application;

namespace Xamarin.Forms.Platform.Tizen
{
	internal class TizenIsolatedStorageFile : IIsolatedStorageFile
	{
		string _rootPath;

		internal TizenIsolatedStorageFile()
		{
			_rootPath = TApplication.Current.DirectoryInfo.Data;
		}

		public void CreateDirectory(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			Directory.CreateDirectory(Path.Combine(_rootPath, path));
		}

		public Task CreateDirectoryAsync(string path)
		{
			CreateDirectory(path);
			return Task.FromResult(true);
		}

		public void MoveFile(string source, string dest)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (dest == null)
				throw new ArgumentNullException("dest");

			File.Move(Path.Combine(_rootPath, source), Path.Combine(_rootPath, dest));
		}

		public void DeleteFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			if (path.Trim().Length == 0)
				throw new ArgumentException("An empty path is not valid.");

			File.Delete(Path.Combine(_rootPath, path));
		}

		public bool DirectoryExists(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			return Directory.Exists(Path.Combine(_rootPath, path));
		}

		public Task<bool> GetDirectoryExistsAsync(string path)
		{
			return Task.FromResult(DirectoryExists(path));
		}

		public bool FileExists(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			return File.Exists(Path.Combine(_rootPath, path));
		}

		public Task<bool> GetFileExistsAsync(string path)
		{
			return Task.FromResult(FileExists(path));
		}

		public DateTimeOffset GetLastWriteTime(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			if (path.Trim().Length == 0)
				throw new ArgumentException("An empty path is not valid.");

			string fullPath = Path.Combine(_rootPath, path);
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
				throw new ArgumentNullException("path");
			if (path.Trim().Length == 0)
				throw new ArgumentException("An empty path is not valid.");

			string fullPath = Path.Combine(_rootPath, path);
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
