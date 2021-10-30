#nullable enable
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Engine.BitmapRecycle;
using Bumptech.Glide.Load.Resource.Bitmap;

namespace Microsoft.Maui.BumptechGlide
{
	// TODO: make this public and do it better
	class FontImageSourceDecoder : Java.Lang.Object, IResourceDecoder
	{
		readonly IBitmapPool _bitmapPool = new BitmapPoolAdapter();

		public bool Handles(Java.Lang.Object model, Options options) =>
			model is FontImageSourceModel;

		public IResource Decode(Java.Lang.Object model, int width, int height, Options options)
		{
			var source = (FontImageSourceModel)model;

			// TODO: do not use the static here, make the service extensible so that it can be overridden
			var bitmap = FontImageSourceService.RenderBitmap(source, (w, h, c) => _bitmapPool.Get(w, h, c));

			return new BitmapResource(bitmap, _bitmapPool);
		}
	}
}