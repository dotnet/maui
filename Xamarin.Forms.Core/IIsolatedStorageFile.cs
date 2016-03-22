using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	internal interface IIsolatedStorageFile
	{
		Task CreateDirectoryAsync(string path);
		Task<bool> GetDirectoryExistsAsync(string path);
		Task<bool> GetFileExistsAsync(string path);

		Task<DateTimeOffset> GetLastWriteTimeAsync(string path);

		Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access);
		Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share);
	}
}