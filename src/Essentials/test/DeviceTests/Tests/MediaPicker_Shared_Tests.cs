using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// Cross-platform validation of the shared MediaPicker image-processing pipeline
	// (ImageProcessor built on MAUI Graphics). Runs on Android, iOS, MacCatalyst and Windows and
	// exercises the exact code path the platform MediaPickers use to process a picked photo.
	[Category("MediaPicker")]
	public class MediaPicker_Shared_Tests
	{
		// A small (120x90) valid JPEG used as a stand-in for a picked photo.
		const string SampleJpegBase64 =
			"/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAMCAgICAgMCAgIDAwMDBAYEBAQEBAgGBgUGCQgKCgkICQkKDA8MCgsOCwkJDRENDg8QEBEQCgwSExIQEw8QEBD/2wBDAQMDAwQDBAgEBAgQCwkLEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBD/wAARCABaAHgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDy+iiiv6gPyQKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//Z";

		static byte[] SampleJpeg => Convert.FromBase64String(SampleJpegBase64);

		[Fact]
		public async Task ProcessImage_Downsizes_ToNewCacheFile()
		{
			var loadingService = new PlatformImageLoadingService();
			string outputPath = null;
			try
			{
				using var input = new MemoryStream(SampleJpeg);

				outputPath = await ImageProcessor.ProcessImageToCacheFileAsync(
					input,
					"picked.jpg",
					new ImageProcessingOptions(
						maximumWidth: 40,
						maximumHeight: 40,
						compressionQuality: 80,
						rotateImage: false,
						preserveMetadata: true));

				// A brand new file is written to the app cache, preserving the original file name.
				Assert.True(File.Exists(outputPath), "Processed output file was not created.");
				Assert.StartsWith(FileSystem.CacheDirectory, outputPath, StringComparison.Ordinal);
				Assert.Equal("picked.jpg", Path.GetFileName(outputPath));

				// The output must decode back to a valid image that respects the resize constraint
				// (source is 120x90, so it downsizes to 40x30 preserving aspect ratio).
				using var verifyStream = File.OpenRead(outputPath);
				using var processed = loadingService.FromStream(verifyStream);
				Assert.NotNull(processed);
				Assert.True(
					processed.Width <= 40 && processed.Height <= 40,
					$"Expected a downsized image <= 40px, got {processed.Width}x{processed.Height}.");
			}
			finally
			{
				if (outputPath is not null && File.Exists(outputPath))
					File.Delete(outputPath);
			}
		}

		[Fact]
		public async Task ProcessImage_PreservesJpegContainer()
		{
			string outputPath = null;
			try
			{
				using var input = new MemoryStream(SampleJpeg);

				outputPath = await ImageProcessor.ProcessImageToCacheFileAsync(
					input,
					"picked.jpg",
					new ImageProcessingOptions(
						maximumWidth: null,
						maximumHeight: null,
						compressionQuality: 60,
						rotateImage: false,
						preserveMetadata: true));

				// Deterministic container preservation: a JPEG source stays a JPEG.
				Assert.Equal(".jpg", Path.GetExtension(outputPath));

				var bytes = File.ReadAllBytes(outputPath);
				Assert.True(
					bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
					"Output is not a valid JPEG.");
			}
			finally
			{
				if (outputPath is not null && File.Exists(outputPath))
					File.Delete(outputPath);
			}
		}
	}
}
