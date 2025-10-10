using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("FileSystem")]
	public class FileSystem_Tests
	{
		const string BundleFileContents = "This file was in the app bundle.";

		[Fact]
		public void CacheDirectory_Is_Valid()
		{
			Assert.False(string.IsNullOrWhiteSpace(FileSystem.CacheDirectory));
		}

		[Fact]
		public void AppDataDirectory_Is_Valid()
		{
			Assert.False(string.IsNullOrWhiteSpace(FileSystem.AppDataDirectory));
		}

		[Theory]
		[InlineData("AppBundleFile.txt", BundleFileContents)]
		[InlineData("AppBundleFile_NoExtension", BundleFileContents)]
		[InlineData("Folder/AppBundleFile_Nested.txt", BundleFileContents)]
		[InlineData("Folder\\AppBundleFile_Nested.txt", BundleFileContents)]
		public async Task OpenAppPackageFileAsync_Can_Load_File(string filename, string contents)
		{
			using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
			Assert.NotNull(stream);

			using var reader = new StreamReader(stream);
			var text = await reader.ReadToEndAsync().ConfigureAwait(false);

			Assert.Equal(contents, text);
		}

		[Fact]
		public async Task OpenAppPackageFileAsync_Throws_If_File_Is_Not_Found()
		{
			await Assert.ThrowsAsync<FileNotFoundException>(() => FileSystem.OpenAppPackageFileAsync("MissingFile.txt")).ConfigureAwait(false);
		}

#if MACCATALYST
		[Fact]
		public async Task ValidateMIMEFormat()
		{
			string filePath = Path.Combine(FileSystem.CacheDirectory, "sample.txt");
			await File.WriteAllTextAsync(filePath, "File Content type is text/plain");

			FileResult fileResult = new FileResult(filePath);
			Assert.Equal("text/plain", fileResult.ContentType);

			File.Delete(filePath);
		}
#endif

#if WINDOWS
		[Theory]
		[InlineData(".jpg", "image/jpeg")]
		[InlineData(".JPG", "image/jpeg")]
		[InlineData(".Jpg", "image/jpeg")]
		[InlineData(".jPg", "image/jpeg")]
		[InlineData(".jpg", "image/jpeg")]
		[InlineData(".jpg ", "image/jpeg")]  // Trailing space
		[InlineData(" .jpg", "image/jpeg")]  // Leading space
		[InlineData(" .jpg ", "image/jpeg")] // Leading and trailing spaces
		[InlineData(".png", "image/png")]
		[InlineData(".PNG", "image/png")]
		[InlineData(".tar.gz", "application/gzip")]
		[InlineData(".TAR.GZ", "application/gzip")]
		public async Task EnsureFileResultContentType(string extension, string expectedMimeType)
		{
			string filePath = Path.Combine(FileSystem.CacheDirectory, $"test{extension}");
			await File.WriteAllTextAsync(filePath, $"File Content type is {expectedMimeType}");
			FileResult fileResult = new FileResult(filePath);
			Assert.Equal(expectedMimeType, fileResult.ContentType);
			File.Delete(filePath);
		}
#endif
	}
}
