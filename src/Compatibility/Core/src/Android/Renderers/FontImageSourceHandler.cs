using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Color = Microsoft.Maui.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public sealed class FontImageSourceHandler : IImageSourceHandler
	{
		public Task<Bitmap> LoadImageAsync(
			ImageSource imagesource,
			Context context,
			CancellationToken cancelationToken = default(CancellationToken))
		{
			Bitmap image = null;
			var fontsource = imagesource as FontImageSource;
			if (fontsource != null)
			{
				using var paint = new Paint
				{
					TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)fontsource.Size, context.Resources.DisplayMetrics),
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
					Color = (fontsource.Color ?? Colors.White).ToAndroid(),
#pragma warning restore CA1416
					TextAlign = Paint.Align.Left,
					AntiAlias = true,
				};

				paint.SetTypeface(fontsource.FontFamily.ToTypeface(imagesource.RequireFontManager()));

				var width = (int)(paint.MeasureText(fontsource.Glyph) + .5f);
				var baseline = (int)(-paint.Ascent() + .5f);
				var height = (int)(baseline + paint.Descent() + .5f);
				image = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
				using var canvas = new Canvas(image);
				canvas.DrawText(fontsource.Glyph, 0, baseline, paint);
			}

			return Task.FromResult(image);
		}
	}
}