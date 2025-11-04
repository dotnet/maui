#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
		{
			var fontImageSource = (IFontImageSource)imageSource;
			if (!fontImageSource.IsEmpty)
			{
				var size = FontManager.GetFontSize(fontImageSource.Font);
				var unit = fontImageSource.Font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip;
				var textSize = TypedValue.ApplyDimension(unit, size.Value, imageView.Context?.Resources?.DisplayMetrics);
				var typeface = FontManager.GetTypeface(fontImageSource.Font);
				var color = (fontImageSource.Color ?? Graphics.Colors.White).ToPlatform();

				var callback = new ImageLoaderCallback();

				try
				{
					PlatformInterop.LoadImageFromFont(
						imageView,
						color,
						fontImageSource.Glyph,
						typeface,
						textSize,
						callback);

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", fontImageSource.Glyph);
					throw;
				}
			}
			return Task.FromResult<IImageSourceServiceResult?>(null);
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var fontImageSource = (IFontImageSource)imageSource;
			if (!fontImageSource.IsEmpty)
			{
				var size = FontManager.GetFontSize(fontImageSource.Font);
				var unit = fontImageSource.Font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip;
				var textSize = TypedValue.ApplyDimension(unit, size.Value, context?.Resources?.DisplayMetrics);
				var typeface = FontManager.GetTypeface(fontImageSource.Font);
				var color = (fontImageSource.Color ?? Graphics.Colors.White).ToPlatform();

				try
				{
					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromFont(
						context,
						color,
						fontImageSource.Glyph,
						typeface,
						textSize,
						drawableCallback);

					return drawableCallback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", fontImageSource.Glyph);
					throw;
				}
			}
			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}
	}
}