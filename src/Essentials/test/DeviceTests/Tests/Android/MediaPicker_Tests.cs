using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Xunit;
using Path = System.IO.Path;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("MediaPicker")]
	public class Android_MediaPicker_Tests
	{
		// Creates a small, decodable JPEG in a directory OUTSIDE the app cache to represent a picked
		// file whose location MAUI does not own and therefore must never modify or delete.
		static string CreateSourceImage(string fileName, string makeTag = null)
		{
			var dir = Path.Combine(FileSystem.AppDataDirectory, "mediapicker_source_tests");
			Directory.CreateDirectory(dir);
			var path = Path.Combine(dir, fileName);

			using (var bitmap = Bitmap.CreateBitmap(64, 48, Bitmap.Config.Argb8888))
			{
				bitmap.EraseColor(unchecked((int)0xFF3366CC));
				using var stream = File.Create(path);
				bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
			}

			if (makeTag is not null)
			{
				var exif = new ExifInterface(path);
				exif.SetAttribute(ExifInterface.TagMake, makeTag);
				exif.SaveAttributes();
			}

			return path;
		}

		static string CanonicalCacheRoot()
			=> new Java.IO.File(FileSystem.CacheDirectory).CanonicalPath;

		// Resolves symlinks (e.g. /data/user/0 -> /data/data) so cache-root prefix checks are stable.
		static string Canonical(string path)
			=> new Java.IO.File(path).CanonicalPath;

		static void CleanUp(params string[] paths)
		{
			foreach (var path in paths)
			{
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
					File.Delete(path);
			}
		}

		[Fact]
		public async Task ProcessPhoto_Compress_DoesNotModifyOrDeleteSource()
		{
			var sourcePath = CreateSourceImage("compress_source.jpg");
			var originalBytes = File.ReadAllBytes(sourcePath);
			string resultPath = null;
			try
			{
				var options = new MediaPickerOptions { CompressionQuality = 50 };

				resultPath = await MediaPickerImplementation.ProcessPhotoAsync(sourcePath, options);

				// The picked source must be left completely untouched.
				Assert.True(File.Exists(sourcePath), "Source file was deleted.");
				Assert.Equal(originalBytes, File.ReadAllBytes(sourcePath));

				// The processed output must be a NEW file inside the app's own cache directory...
				Assert.NotEqual(sourcePath, resultPath);
				Assert.True(File.Exists(resultPath));
				Assert.StartsWith(CanonicalCacheRoot(), Canonical(resultPath), System.StringComparison.Ordinal);

				// ...while preserving the original file name (issue #33258).
				Assert.Equal(Path.GetFileName(sourcePath), Path.GetFileName(resultPath));
			}
			finally
			{
				CleanUp(sourcePath, resultPath);
			}
		}

		[Fact]
		public async Task ProcessPhoto_Rotate_DoesNotModifyOrDeleteSource()
		{
			var sourcePath = CreateSourceImage("rotate_source.jpg");
			var originalBytes = File.ReadAllBytes(sourcePath);
			string resultPath = null;
			try
			{
				var options = new MediaPickerOptions { RotateImage = true };

				resultPath = await MediaPickerImplementation.ProcessPhotoAsync(sourcePath, options);

				Assert.True(File.Exists(sourcePath), "Source file was deleted.");
				Assert.Equal(originalBytes, File.ReadAllBytes(sourcePath));
				Assert.NotEqual(sourcePath, resultPath);
				Assert.True(File.Exists(resultPath));
				Assert.StartsWith(CanonicalCacheRoot(), Canonical(resultPath), System.StringComparison.Ordinal);
			}
			finally
			{
				CleanUp(sourcePath, resultPath);
			}
		}

		[Fact]
		public async Task ProcessPhoto_PreservesExifMetadata()
		{
			var sourcePath = CreateSourceImage("meta_source.jpg", makeTag: "CopilotCam");
			string resultPath = null;
			try
			{
				var options = new MediaPickerOptions { CompressionQuality = 60, PreserveMetaData = true };

				resultPath = await MediaPickerImplementation.ProcessPhotoAsync(sourcePath, options);

				var exif = new ExifInterface(resultPath);
				Assert.Equal("CopilotCam", exif.GetAttribute(ExifInterface.TagMake));
			}
			finally
			{
				CleanUp(sourcePath, resultPath);
			}
		}
	}
}
