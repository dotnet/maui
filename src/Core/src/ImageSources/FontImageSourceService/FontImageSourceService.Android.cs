using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Bumptech.Glide;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public Task<Drawable?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFontImageSource fontImageSource)
				return GetDrawableAsync(fontImageSource, context, cancellationToken);

			return Task.FromResult<Drawable?>(null);
		}

		public async Task<Drawable?> GetDrawableAsync(IFontImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var glyph = imageSource.Glyph;
			var textSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, FontManager.GetScaledPixel(imageSource.Font), context.Resources?.DisplayMetrics);
			var typeface = FontManager.GetTypeface(imageSource.Font);
			var color = (imageSource.Color ?? Graphics.Colors.White).ToNative();

			var target = Glide
				.With(context)
				.Load(new FontImageSourceModel(glyph, textSize, typeface, color))
				.Submit();

			var drawable = await target.AsTask<Drawable>(cancellationToken);

			return drawable;
		}

		internal static Bitmap RenderBitmap(IModel model, Func<int, int, Bitmap.Config, Bitmap> newBitmap)
		{
			using var paint = new Paint
			{
				TextSize = model.TextSize,
				Color = model.Color,
				TextAlign = Paint.Align.Left,
				AntiAlias = true,
			};

			if (model.Typeface != null)
				paint.SetTypeface(model.Typeface);

			var width = (int)(paint.MeasureText(model.Glyph) + .5f);
			var baseline = (int)(-paint.Ascent() + .5f);
			var height = (int)(baseline + paint.Descent() + .5f);

			var bitmap = newBitmap(width, height, Bitmap.Config.Argb8888!);

			using var canvas = new Canvas(bitmap);
			canvas.DrawText(model.Glyph, 0, baseline, paint);

			return bitmap;
		}

		internal interface IModel
		{
			Color Color { get; }

			string Glyph { get; }

			float TextSize { get; }

			Typeface? Typeface { get; }
		}
	}
}