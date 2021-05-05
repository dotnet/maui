#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Bumptech.Glide;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.BumptechGlide;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((IFontImageSource)imageSource, context, cancellationToken);

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IFontImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var glyph = imageSource.Glyph;

			var sp = FontManager.GetFontSize(imageSource.Font);
			var textSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, sp, context.Resources?.DisplayMetrics);
			var typeface = FontManager.GetTypeface(imageSource.Font);
			var color = (imageSource.Color ?? Graphics.Colors.White).ToNative();

			try
			{
				var result = await Glide
					.With(context)
					.Load(new FontImageSourceModel(glyph, textSize, typeface, color))
					.SubmitAsync(context, cancellationToken)
					.ConfigureAwait(false);

				if (result == null)
					throw new InvalidOperationException("Unable to generate font image.");

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", glyph);
				throw;
			}
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