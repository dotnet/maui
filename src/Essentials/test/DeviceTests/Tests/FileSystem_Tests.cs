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
		[Fact]
		public async Task CheckFileResultWithFilePath()
		{
			string filePath = Path.Combine(FileSystem.CacheDirectory, "sample.txt");
			await File.WriteAllTextAsync(filePath, "Sample content for testing");

			var fileResult = new FileResult(filePath);

			using var stream = await fileResult.OpenReadAsync();

			Assert.NotNull(stream);

			File.Delete(filePath);
		}

		[Fact]
		public async Task CheckFileResultOpenReadAsyncMultipleTimes()
		{
			string filePath = Path.Combine(FileSystem.CacheDirectory, "sample_multiple.txt");
			string expectedContent = "Sample content for multiple stream testing";
			await File.WriteAllTextAsync(filePath, expectedContent);

			var fileResult = new FileResult(filePath);

			// First call to OpenReadAsync
			using (var firstStream = await fileResult.OpenReadAsync())
			{
				Assert.NotNull(firstStream);
				using var firstReader = new StreamReader(firstStream);
				var firstContent = await firstReader.ReadToEndAsync();
				Assert.Equal(expectedContent, firstContent);
			}

			// Second call to OpenReadAsync - should still work
			using (var secondStream = await fileResult.OpenReadAsync())
			{
				Assert.NotNull(secondStream);
				using var secondReader = new StreamReader(secondStream);
				var secondContent = await secondReader.ReadToEndAsync();
				Assert.Equal(expectedContent, secondContent);
			}

			File.Delete(filePath);
		}
	}
}
