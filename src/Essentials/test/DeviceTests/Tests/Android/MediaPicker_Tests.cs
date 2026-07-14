using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Xunit;
using Path = System.IO.Path;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("MediaPicker")]
	public class Android_MediaPicker_Tests
	{
		[Fact]
		public async Task RotateImage_DoesNotModifySource_And_WritesToCache()
		{
			var sourcePath = CreateJpegOutsideCache("rotate_source.jpg");
			var originalBytes = File.ReadAllBytes(sourcePath);
			string outputPath = null;

			try
			{
				var options = new MediaPickerOptions { RotateImage = true };

				outputPath = await MediaPickerImplementation.RotateImage(sourcePath, options);

				// The picked source is left untouched - no in-place overwrite, no data loss.
				Assert.True(File.Exists(sourcePath));
				Assert.Equal(originalBytes, File.ReadAllBytes(sourcePath));

				// The processed image is a brand new file under the app cache dir.
				Assert.NotEqual(sourcePath, outputPath);
				Assert.True(File.Exists(outputPath));
				Assert.StartsWith(FileSystem.CacheDirectory, outputPath, StringComparison.Ordinal);

				// The original filename is preserved (no _processed/_rotated suffix - #33258).
				Assert.Equal(Path.GetFileName(sourcePath), Path.GetFileName(outputPath));
			}
			finally
			{
				SafeDelete(sourcePath);
				SafeDelete(outputPath);
			}
		}

		[Fact]
		public async Task CompressImageIfNeeded_DoesNotModifySource_And_WritesToCache()
		{
			var sourcePath = CreateJpegOutsideCache("compress_source.jpg");
			var originalBytes = File.ReadAllBytes(sourcePath);
			string outputPath = null;

			try
			{
				var options = new MediaPickerOptions { CompressionQuality = 50 };

				outputPath = await MediaPickerImplementation.CompressImageIfNeeded(sourcePath, options);

				// The picked source is left untouched - no in-place overwrite, no data loss.
				Assert.True(File.Exists(sourcePath));
				Assert.Equal(originalBytes, File.ReadAllBytes(sourcePath));

				// The processed image is a brand new file under the app cache dir.
				Assert.NotEqual(sourcePath, outputPath);
				Assert.True(File.Exists(outputPath));
				Assert.StartsWith(FileSystem.CacheDirectory, outputPath, StringComparison.Ordinal);

				// The original filename is preserved (#33258); only the extension may change.
				Assert.Equal(
					Path.GetFileNameWithoutExtension(sourcePath),
					Path.GetFileNameWithoutExtension(outputPath));
			}
			finally
			{
				SafeDelete(sourcePath);
				SafeDelete(outputPath);
			}
		}

		static string CreateJpegOutsideCache(string fileName)
		{
			// AppDataDirectory (/data/user/0/<pkg>/files) is separate from the cache dir and
			// stands in for a picked file that MAUI does not own.
			var dir = Path.Combine(FileSystem.AppDataDirectory, "mediapicker-source-tests");
			Directory.CreateDirectory(dir);

			var filePath = Path.Combine(dir, fileName);

			using (var bitmap = Bitmap.CreateBitmap(8, 8, Bitmap.Config.Argb8888))
			using (var stream = File.Create(filePath))
			{
				bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
			}

			return filePath;
		}

		static void SafeDelete(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
					File.Delete(path);
			}
			catch
			{
				// best-effort cleanup
			}
		}
	}
}
