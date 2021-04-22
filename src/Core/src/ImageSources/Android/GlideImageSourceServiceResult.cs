using System;
using Android.Graphics.Drawables;

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
	}
}