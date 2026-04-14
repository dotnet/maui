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
			// A path in a protected system directory that the app cannot read
			var filePath = "/data/system/test_unreachable_file.txt";
			Assert.False(FileSystemUtils.IsFileReadable(filePath));
		}
	}
}
