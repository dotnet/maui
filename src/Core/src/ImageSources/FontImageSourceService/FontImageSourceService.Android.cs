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
		public override Task<bool> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFontImageSource fontImageSource)
			{
				if (fontImageSource.IsEmpty)
					return Task.FromResult(false);

				var glyph = fontImageSource.Glyph;

				var size = FontManager.GetFontSize(fontImageSource.Font);
				var textSize = TypedValue.ApplyDimension(size.Unit, size.Value, imageView.Context?.Resources?.DisplayMetrics);
				var typeface = FontManager.GetTypeface(fontImageSource.Font);
				var color = (fontImageSource.Color ?? Graphics.Colors.White).ToPlatform();

				try
				{
					Glide
						.With(imageView.Context)
						.Load(new FontImageSourceModel(glyph, textSize, typeface, color))
						.Into(imageView);

					return Task.FromResult(true);
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", glyph);
					throw;
				}
			}

			return Task.FromResult(false);
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((IFontImageSource)imageSource, context, cancellationToken);

		public async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IFontImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var glyph = imageSource.Glyph;

			var size = FontManager.GetFontSize(imageSource.Font);
			var textSize = TypedValue.ApplyDimension(size.Unit, size.Value, context.Resources?.DisplayMetrics);
			var typeface = FontManager.GetTypeface(imageSource.Font);
			var color = (imageSource.Color ?? Graphics.Colors.White).ToPlatform();

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