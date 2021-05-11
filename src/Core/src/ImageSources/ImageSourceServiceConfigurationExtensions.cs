#nullable enable
namespace Microsoft.Maui
{
	public static class ImageSourceServiceConfigurationExtensions
	{
		public const string ImageDirectoryKey = "ImageDirectory";

		public static void SetImageDirectory(this IImageSourceServiceConfiguration config, string directory) =>
			config[ImageDirectoryKey] = directory;

		public static string? GetImageDirectory(this IImageSourceServiceConfiguration config) =>
			config.TryGetValue(ImageDirectoryKey, out var directory) ? directory : null;
	}
}