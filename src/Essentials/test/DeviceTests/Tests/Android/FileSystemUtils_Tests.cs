using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Microsoft.Maui.Storage;
using Xunit;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("FileSystem")]
	public class Android_FileSystemUtils_Tests
	{
		[Fact]
		public void IsFileReadable_Returns_True_For_Readable_File()
		{
			var filePath = Path.Combine(FileSystem.CacheDirectory, "readable_test.txt");
			try
			{
				File.WriteAllText(filePath, "test content");
				Assert.True(FileSystemUtils.IsFileReadable(filePath));
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}

		[Fact]
		public Task EnsurePhysicalPath_Rejects_FileUri_Inside_AppDataDirectory()
			=> AssertFileUriRejected(FileSystem.AppDataDirectory);

		[Fact]
		public Task EnsurePhysicalPath_Rejects_FileUri_Inside_DeviceProtectedStorage()
		{
			var deviceProtectedContext = Application.Context.CreateDeviceProtectedStorageContext();
			var filesDirectory = deviceProtectedContext.FilesDir ??
				throw new InvalidOperationException("Device-protected files directory is not available.");

			return AssertFileUriRejected(filesDirectory.CanonicalPath);
		}

		static async Task AssertFileUriRejected(string directory)
		{
			var filePath = Path.Combine(directory, $"picker_private_{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "private app data");

			try
			{
				using var file = new Java.IO.File(filePath);
				using var uri = AndroidUri.FromFile(file);

				Assert.Throws<FileNotFoundException>(() => FileSystemUtils.EnsurePhysicalPath(uri));
				await Assert.ThrowsAsync<FileNotFoundException>(() => FileSystemUtils.EnsurePhysicalPathAsync(uri));
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}

		[Fact]
		public async Task EnsurePhysicalPath_Allows_FileUri_Outside_AppDataDirectory()
		{
			var externalCacheDirectory = Application.Context.ExternalCacheDir;
			Assert.NotNull(externalCacheDirectory);

			var filePath = Path.Combine(externalCacheDirectory.AbsolutePath, $"picker_external_{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "external app data");

			try
			{
				using var file = new Java.IO.File(filePath);
				using var uri = AndroidUri.FromFile(file);

				Assert.Equal(file.CanonicalPath, FileSystemUtils.EnsurePhysicalPath(uri));
				Assert.Equal(file.CanonicalPath, await FileSystemUtils.EnsurePhysicalPathAsync(uri));
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}

		[Fact]
		public void IsFileReadable_Returns_False_For_NonExistent_File()
		{
			var filePath = Path.Combine(FileSystem.CacheDirectory, "nonexistent_file_12345.txt");
			Assert.False(FileSystemUtils.IsFileReadable(filePath));
		}

		[Fact]
		public void IsFileReadable_Returns_False_For_Inaccessible_Path()
		{
			// Create a real file then revoke read permission for a deterministic test
			var filePath = Path.Combine(FileSystem.CacheDirectory, "unreadable_test.txt");
			try
			{
				File.WriteAllText(filePath, "test content");
				using var javaFile = new Java.IO.File(filePath);
				if (!javaFile.SetReadable(false))
				{
					return; // Permission change not supported on this device/config — skip test
				}

				Assert.False(FileSystemUtils.IsFileReadable(filePath));
			}
			finally
			{
				if (File.Exists(filePath))
				{
					// Restore readability before cleanup to return the file to a normal state
					using var javaFile = new Java.IO.File(filePath);
					javaFile.SetReadable(true);
					File.Delete(filePath);
				}
			}
		}
	}
}
