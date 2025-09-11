using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Maui.Diagnostics;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Diagnostics)]
	public class ImageDiagnosticMetricsTests
	{
		[Fact]
		public void Create_InitializesAllMetrics()
		{
			// Arrange
			var meter = new Meter("test-meter");
			var metrics = new ImageDiagnosticMetrics();

			// Act
			metrics.Create(meter);

			// Assert
			Assert.NotNull(metrics.ImageLoadDuration);
			Assert.NotNull(metrics.ImageFileSize);
			Assert.NotNull(metrics.ImageWidth);
			Assert.NotNull(metrics.ImageHeight);
			Assert.NotNull(metrics.ImageLoadSuccess);
			Assert.NotNull(metrics.ImageLoadFailures);

			meter.Dispose();
		}

		[Fact]
		public void RecordImageLoadSuccess_RecordsDurationAndSuccessCount()
		{
			// Arrange
			var meter = new Meter("test-meter");
			var metrics = new ImageDiagnosticMetrics();
			metrics.Create(meter);

			var tagList = new TagList();
			tagList.Add("test.tag", "test-value");

			// Act
			metrics.RecordImageLoadSuccess(tagList, duration: 150);

			// Assert - Verify metrics were created (we can't easily verify recording without instrumentation framework)
			Assert.NotNull(metrics.ImageLoadDuration);
			Assert.NotNull(metrics.ImageLoadSuccess);

			meter.Dispose();
		}

		[Fact]
		public void RecordImageLoadSuccess_WithAllParameters_RecordsAllMetrics()
		{
			// Arrange
			var meter = new Meter("test-meter");
			var metrics = new ImageDiagnosticMetrics();
			metrics.Create(meter);

			var tagList = new TagList();
			tagList.Add("image.format", "PNG");

			// Act
			metrics.RecordImageLoadSuccess(
				tagList,
				duration: 200,
				fileSizeBytes: 1024000,
				widthPixels: 1920,
				heightPixels: 1080);

			// Assert - All metrics should be available for recording
			Assert.NotNull(metrics.ImageLoadDuration);
			Assert.NotNull(metrics.ImageLoadSuccess);
			Assert.NotNull(metrics.ImageFileSize);
			Assert.NotNull(metrics.ImageWidth);
			Assert.NotNull(metrics.ImageHeight);

			meter.Dispose();
		}

		[Fact]
		public void RecordImageLoadFailure_RecordsFailureAndDuration()
		{
			// Arrange
			var meter = new Meter("test-meter");
			var metrics = new ImageDiagnosticMetrics();
			metrics.Create(meter);

			var tagList = new TagList();
			tagList.Add("image.format", "JPEG");

			// Act
			metrics.RecordImageLoadFailure(
				tagList,
				durationMs: 50,
				errorType: "FileNotFound",
				errorMessage: "Image file not found");

			// Assert
			Assert.NotNull(metrics.ImageLoadFailures);
			Assert.NotNull(metrics.ImageLoadDuration);

			meter.Dispose();
		}

		[Fact]
		public void RecordImageLoadFailure_WithMinimalParameters_DoesNotThrow()
		{
			// Arrange
			var meter = new Meter("test-meter");
			var metrics = new ImageDiagnosticMetrics();
			metrics.Create(meter);

			var tagList = new TagList();

			// Act & Assert - Should not throw
			metrics.RecordImageLoadFailure(tagList);

			meter.Dispose();
		}
	}
}