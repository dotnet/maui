#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Bumptech.Glide;

namespace Microsoft.Maui.BumptechGlide
{
	public static class RequestBuilderExtensions
	{
		public static async Task<Drawable?> SubmitAsync(this RequestBuilder requestBuilder, CancellationToken cancellationToken = default)
		{
			var target = requestBuilder.Submit();

			return await target.AsTask<Drawable>(cancellationToken);
		}

		public static Task<ImageSourceServiceResult?> SubmitAsync(this RequestBuilder requestBuilder, Context context, CancellationToken cancellationToken = default) =>
			requestBuilder.SubmitAsync(Glide.With(context), cancellationToken);

		public static async Task<ImageSourceServiceResult?> SubmitAsync(this RequestBuilder requestBuilder, RequestManager requestManager, CancellationToken cancellationToken = default)
		{
			var target = requestBuilder.Submit();

			var drawable = await target.AsTask<Drawable>(cancellationToken);
			if (drawable == null)
				return null;

			return new ImageSourceServiceResult(drawable, () => requestManager.Clear(target));
		}
	}
}