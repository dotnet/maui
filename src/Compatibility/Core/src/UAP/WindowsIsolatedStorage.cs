using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

#if !UWP_16299
using FileMode = Microsoft.Maui.Controls.Compatibility.Internals.FileMode;
using FileAccess = Microsoft.Maui.Controls.Compatibility.Internals.FileAccess;
using FileShare = Microsoft.Maui.Controls.Compatibility.Internals.FileShare;
#endif

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class WindowsIsolatedStorage : Internals.IIsolatedStorageFile
	{
		StorageFolder _folder;

		public WindowsIsolatedStorage(StorageFolder folder)
		{
			if (folder == null)
				throw new ArgumentNullException("folder");

			_folder = folder;
		}

		public Task CreateDirectoryAsync(string path)
		{
			return _folder.CreateFolderAsync(path).AsTask();
		}

		public async Task<bool> GetDirectoryExistsAsync(string path)
		{
			try
			{
				await _folder.GetFolderAsync(path).AsTask().ConfigureAwait(false);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		public async Task<bool> GetFileExistsAsync(string path)
		{
			try
			{
				await _folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		public async Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
		{
			StorageFile file = await _folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			BasicProperties properties = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
			return properties.DateModified;
		}

		public async Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access)
		{
			StorageFile file;

			switch (mode)
			{
				case FileMode.CreateNew:
					file = await _folder.CreateFileAsync(path, CreationCollisionOption.FailIfExists).AsTask().ConfigureAwait(false);
					break;

				case FileMode.Create:
				case FileMode.Truncate: // TODO See if ReplaceExisting already truncates
					file = await _folder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
					break;

				case FileMode.OpenOrCreate:
				case FileMode.Append:
					file = await _folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
					break;

				case FileMode.Open:
					file = await _folder.GetFileAsync(path);
					break;

				default:
					throw new ArgumentException("mode was an invalid FileMode", "mode");
			}

			switch (access)
			{
				case FileAccess.Read:
					return await file.OpenStreamForReadAsync().ConfigureAwait(false);
				case FileAccess.Write:
					Stream stream = await file.OpenStreamForWriteAsync().ConfigureAwait(false);
					if (mode == FileMode.Append)
						stream.Position = stream.Length;

					return stream;

				case FileAccess.ReadWrite:
					IRandomAccessStream randStream = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false);
					return randStream.AsStream();

				default:
					throw new ArgumentException("access was an invalid FileAccess", "access");
			}			
		}

		public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return OpenFileAsync(path, mode, access);
		}
	}
}