#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
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

					var result = await callback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to generate font image '{fontImageSource.Glyph}'.");

					return result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", fontImageSource.Glyph);
					throw;
				}
			}
			return new ImageSourceServiceResult(false);
		}

		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
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
					var drawableCallback = new ImageLoaderCallback(callback);

					PlatformInterop.LoadImageFromFont(
						context,
						color,
						fontImageSource.Glyph,
						typeface,
						textSize,
						drawableCallback);

					var result = await drawableCallback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to generate font image '{fontImageSource.Glyph}'.");

					return result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", fontImageSource.Glyph);
					throw;
				}
			}
			return new ImageSourceServiceResult(false);
		}
	}
}