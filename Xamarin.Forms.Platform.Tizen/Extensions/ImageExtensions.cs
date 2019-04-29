namespace Xamarin.Forms.Platform.Tizen
{
	internal static class ImageExtensions
	{
		internal static bool IsNullOrEmpty(this ImageSource imageSource) =>
			imageSource == null || imageSource.IsEmpty;
	}
}
