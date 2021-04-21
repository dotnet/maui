using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Bumptech.Glide.Request;

namespace Microsoft.Maui
{
	public class GlideImageSourceServiceResult : IImageSourceServiceResult<Drawable>
	{
		Action? _dispose;

		public GlideImageSourceServiceResult(Drawable drawable, Action? dispose = null)
		{
			Value = drawable;
			_dispose = dispose;
		}

		public Drawable Value { get; }

		public void Dispose()
		{
			_dispose?.Invoke();
			_dispose = null;
		}

		public static async Task<GlideImageSourceServiceResult?> CreateAsync(IFutureTarget target, RequestManager manager, CancellationToken cancellationToken = default)
		{
			var drawable = await target.AsTask<Drawable>(cancellationToken);
			if (drawable == null)
				return null;

			return new GlideImageSourceServiceResult(drawable, () => manager.Clear(target));
		}
	}
}