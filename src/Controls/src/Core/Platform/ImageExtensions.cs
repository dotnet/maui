namespace Microsoft.Maui.Controls.Platform;

static partial class ImageExtensions
{
	public static bool IsNullOrEmpty(this ImageSource? imageSource) =>
		imageSource is null || imageSource.IsEmpty;
}
