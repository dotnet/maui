using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	internal class WPFIsolatedStorageFile : IIsolatedStorageFile
	{
		readonly IsolatedStorageFile _isolatedStorageFile;

		public WPFIsolatedStorageFile(IsolatedStorageFile isolatedStorageFile)
		{
			_isolatedStorageFile = isolatedStorageFile;
		}

		public Task CreateDirectoryAsync(string path)
		{
			_isolatedStorageFile.CreateDirectory(path);
			return Task.FromResult(true);
		}

		public Task<bool> GetDirectoryExistsAsync(string path)
		{
			return Task.FromResult(_isolatedStorageFile.DirectoryExists(path));
		}

		public Task<bool> GetFileExistsAsync(string path)
		{
			return Task.FromResult(_isolatedStorageFile.FileExists(path));
		}

		public Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
		{
			return Task.FromResult(_isolatedStorageFile.GetLastWriteTime(path));
		}

		public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access)
		{
			Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access);
			return Task.FromResult(stream);
		}

		public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
		{
			Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access, (System.IO.FileShare)share);
			return Task.FromResult(stream);
		}
	}
}