#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials;

/// <summary>
/// Cross-platform image processing for the <see cref="IMediaPicker"/>, built entirely on the
/// MAUI Graphics image pipeline (<see cref="IImageLoadingService"/> / <see cref="IImage"/>).
///
/// The processing itself is shared across every platform; only the small amount of I/O plumbing
/// (obtaining the source stream and the platform <see cref="IImageLoadingService"/>) lives in the
/// platform-specific MediaPicker code.
/// </summary>
internal static class ImageProcessor
{
	/// <summary>
	/// Whether the caller asked for EXIF rotation normalization.
	/// </summary>
	public static bool IsRotationNeeded(MediaPickerOptions? options) =>
		options?.RotateImage ?? false;

	/// <summary>
	/// Whether any processing (resize or recompress) is required.
	/// </summary>
	public static bool IsProcessingNeeded(int? maxWidth, int? maxHeight, int qualityPercent) =>
		maxWidth.HasValue || maxHeight.HasValue || qualityPercent < 100;

	/// <summary>
	/// Whether any processing (rotation, resize, or recompress) is required.
	/// </summary>
	public static bool IsProcessingNeeded(MediaPickerOptions? options) =>
		IsRotationNeeded(options) ||
		IsProcessingNeeded(options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100);

	/// <summary>
	/// Determines the output container. Deterministic: PNG stays PNG, everything else becomes JPEG.
	/// </summary>
	public static ImageFormat GetOutputFormat(string? originalFileName)
	{
		var extension = string.IsNullOrEmpty(originalFileName) ? null : Path.GetExtension(originalFileName);
		return string.Equals(extension, FileExtensions.Png, StringComparison.OrdinalIgnoreCase)
			? ImageFormat.Png
			: ImageFormat.Jpeg;
	}

	/// <summary>
	/// The file extension for a given <see cref="ImageFormat"/>.
	/// </summary>
	public static string GetOutputExtension(ImageFormat format) =>
		format == ImageFormat.Png ? FileExtensions.Png : FileExtensions.Jpg;

	/// <summary>
	/// Loads the <paramref name="input"/> through MAUI Graphics (normalizing EXIF orientation and/or
	/// capturing metadata per the options), applies any resize, and writes the encoded result directly
	/// to <paramref name="output"/>. No intermediate in-memory buffering of the encoded image is done.
	/// </summary>
	public static async Task ProcessImageAsync(
		IImageLoadingService loadingService,
		Stream input,
		Stream output,
		ImageFormat format,
		int? maxWidth,
		int? maxHeight,
		int qualityPercent,
		bool rotateImage,
		bool preserveMetaData)
	{
		var loadOptions = new ImageLoadOptions
		{
			// RotateImage == true means "normalize the EXIF orientation into the pixels".
			DisableRotationNormalization = !rotateImage,
			PreserveMetadata = preserveMetaData,
		};

		using var image = loadingService.FromStream(input, loadOptions)
			?? throw new InvalidOperationException("Failed to load image from stream.");

		var current = image;
		if (maxWidth.HasValue || maxHeight.HasValue)
		{
			// Downsize preserves aspect ratio and only ever scales down.
			current = image.Downsize(maxWidth ?? int.MaxValue, maxHeight ?? int.MaxValue, disposeOriginal: false);
		}

		try
		{
			var saveOptions = new ImageSaveOptions
			{
				Quality = Math.Max(0, Math.Min(100, qualityPercent)) / 100f,
				PreserveMetadata = preserveMetaData,
			};

			await current.SaveAsync(output, format, saveOptions).ConfigureAwait(false);
		}
		finally
		{
			if (current != image)
			{
				current.Dispose();
			}
		}
	}

	/// <summary>
	/// Processes the <paramref name="input"/> and writes the result to a new file in the app cache
	/// directory, preserving the original file name (with a corrected extension). The source is only
	/// ever read, never modified. Returns the path to the new file.
	/// </summary>
	public static async Task<string> ProcessImageToCacheFileAsync(
		IImageLoadingService loadingService,
		Stream input,
		string? originalFileName,
		int? maxWidth,
		int? maxHeight,
		int qualityPercent,
		bool rotateImage,
		bool preserveMetaData)
	{
		var format = GetOutputFormat(originalFileName);
		var extension = GetOutputExtension(format);

		var baseName = string.IsNullOrEmpty(originalFileName)
			? Guid.NewGuid().ToString("N")
			: Path.GetFileNameWithoutExtension(originalFileName);

		// Write into a unique sub-directory of the app cache so the original file name can be preserved
		// exactly (issue #33258) without ever colliding with another picked file of the same name.
		var outputDirectory = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(outputDirectory);
		var outputPath = Path.Combine(outputDirectory, baseName + extension);

		using (var output = File.Create(outputPath))
		{
			await ProcessImageAsync(
				loadingService, input, output, format,
				maxWidth, maxHeight, qualityPercent, rotateImage, preserveMetaData).ConfigureAwait(false);
		}

		return outputPath;
	}
}
