using Android.Graphics;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Engine.BitmapRecycle;
using Bumptech.Glide.Load.Resource.Bitmap;

namespace Microsoft.Maui.BumptechGlide
{
	public class FontImageSourceModel : Java.Lang.Object, FontImageSourceService.IModel
	{
		public FontImageSourceModel(string glyph, float textSize, Typeface? typeface, Color color)
		{
			Glyph = glyph;
			TextSize = textSize;
			Typeface = typeface;
			Color = color;
		}

		public string Glyph { get; }

		public float TextSize { get; }

		public Typeface? Typeface { get; }

		public Color Color { get; }

		public override string ToString() =>
			$"FontImageSourceModel{{Glyph={Glyph}, TextSize={TextSize}, Typeface={Typeface}, Color={Color}}}";
	}

	public class FontImageSourceDecoder : Java.Lang.Object, IResourceDecoder
	{
		readonly IBitmapPool _bitmapPool = new BitmapPoolAdapter();

		public bool Handles(Java.Lang.Object model, Options options) =>
			model is FontImageSourceModel;

		public IResource Decode(Java.Lang.Object model, int width, int height, Options options)
		{
			var source = (FontImageSourceModel)model;

			var bitmap = FontImageSourceService.RenderBitmap(source, (w, h, c) => _bitmapPool.Get(w, h, c));

			return new BitmapResource(bitmap, _bitmapPool);
		}
	}
}