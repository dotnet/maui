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
		public Task ProcessImage_Rotation_DoesNotModifySource_And_WritesSingleCacheFile()
			=> AssertProcessedToCacheWithoutTouchingSource(
				new MediaPickerOptions { RotateImage = true });

		[Fact]
		public Task ProcessImage_Compression_DoesNotModifySource_And_WritesSingleCacheFile()
			=> AssertProcessedToCacheWithoutTouchingSource(
				new MediaPickerOptions { CompressionQuality = 50 });

		[Fact]
		public Task ProcessImage_RotationAndCompression_DoesNotModifySource_And_WritesSingleCacheFile()
			=> AssertProcessedToCacheWithoutTouchingSource(
				new MediaPickerOptions { RotateImage = true, CompressionQuality = 50 });

		[Fact]
		public async Task ProcessImage_NoProcessingNeeded_ReturnsOriginalPath_Unchanged()
		{
			var baseName = "mp_noop_" + Guid.NewGuid().ToString("N");
			var sourcePath = CreateJpegOutsideCache(baseName + ".jpg");
			var originalBytes = File.ReadAllBytes(sourcePath);

			try
			{
				// No rotation, full quality, no resize -> there is nothing to process.
				var options = new MediaPickerOptions { RotateImage = false, CompressionQuality = 100 };

				var outputPath = await MediaPickerImplementation.ProcessImage(sourcePath, options);

				// The original path is returned as-is and the file is untouched.
				Assert.Equal(sourcePath, outputPath);
				Assert.True(File.Exists(sourcePath));
				Assert.Equal(originalBytes, File.ReadAllBytes(sourcePath));

				// No cache copy is created when there is nothing to do.
				Assert.Empty(FindCacheFiles(baseName));
			}
			finally
			{
				SafeDelete(sourcePath);
				DeleteCacheFiles(baseName);
			}
		}

		static async Task AssertProcessedToCacheWithoutTouchingSource(MediaPickerOptions options)
		{
			var baseName = "mp_src_" + Guid.NewGuid().ToString("N");
			var sourcePath = CreateJpegOutsideCache(baseName + ".jpg");
			var originalBytes = File.ReadAllBytes(sourcePath);

			try
			{
				var outputPath = await MediaPickerImplementation.ProcessImage(sourcePath, options);

				// The picked source is left untouched - no in-place overwrite, no data loss.
				Assert.True(File.Exists(sourcePath));
				Assert.Equal(originalBytes, File.ReadAllBytes(sourcePath));

				// The processed image is a brand new file under the app cache dir.
				Assert.NotEqual(sourcePath, outputPath);
				Assert.True(File.Exists(outputPath));
				Assert.StartsWith(FileSystem.CacheDirectory, outputPath, StringComparison.Ordinal);

				// The original base filename is preserved (#33258); only the extension may change.
				Assert.Equal(baseName, Path.GetFileNameWithoutExtension(outputPath));

				// Exactly one terminal cache file is written - no orphaned intermediate,
				// even when both rotation and compression are applied.
				var cacheCopies = FindCacheFiles(baseName);
				Assert.Single(cacheCopies);
				Assert.Equal(Path.GetFullPath(outputPath), Path.GetFullPath(cacheCopies[0]));
			}
			finally
			{
				SafeDelete(sourcePath);
				DeleteCacheFiles(baseName);
			}
		}

		static string CreateJpegOutsideCache(string fileName)
		{
			// AppDataDirectory (/data/user/0/<pkg>/files) is separate from the cache dir and
			// stands in for a picked file that MAUI does not own.
			var dir = Path.Combine(FileSystem.AppDataDirectory, "mediapicker-source-tests");
			Directory.CreateDirectory(dir);

			var filePath = Path.Combine(dir, fileName);

			using (var bitmap = Bitmap.CreateBitmap(64, 64, Bitmap.Config.Argb8888))
			using (var stream = File.Create(filePath))
			{
				bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
			}

			return filePath;
		}

		static string[] FindCacheFiles(string baseName)
		{
			if (!Directory.Exists(FileSystem.CacheDirectory))
				return Array.Empty<string>();

			return Directory.GetFiles(FileSystem.CacheDirectory, baseName + ".*", SearchOption.AllDirectories);
		}

		static void DeleteCacheFiles(string baseName)
		{
			foreach (var file in FindCacheFiles(baseName))
				SafeDelete(file);
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
