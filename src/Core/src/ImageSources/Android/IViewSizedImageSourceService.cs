#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	/// <summary>
	/// An image source service that can decode an image for a view of a given size without loading it into the view,
	/// like <see cref="IImageSourceService.LoadDrawableAsync"/> would.
	/// </summary>
	interface IViewSizedImageSourceService
	{
		/// <summary>Glide's <c>Target.SIZE_ORIGINAL</c>: decode the image at its original size.</summary>
		const int SizeOriginal = int.MinValue;

		/// <param name="width">The width of the view's content, in pixels, or <see cref="SizeOriginal"/>.</param>
		/// <param name="height">The height of the view's content, in pixels, or <see cref="SizeOriginal"/>.</param>
		Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, int width, int height, CancellationToken cancellationToken);
	}
}
