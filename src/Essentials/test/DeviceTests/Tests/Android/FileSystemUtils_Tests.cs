using System.IO;
using Microsoft.Maui.Storage;
using Xunit;

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
				javaFile.SetReadable(false);

				Assert.False(FileSystemUtils.IsFileReadable(filePath));
			}
			finally
			{
				if (File.Exists(filePath))
				{
					// Restore permission so cleanup can delete it
					using var javaFile = new Java.IO.File(filePath);
					javaFile.SetReadable(true);
					File.Delete(filePath);
				}
			}
		}
	}
}
