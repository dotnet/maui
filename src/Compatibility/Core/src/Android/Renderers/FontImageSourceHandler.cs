using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Util;
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
					Color = (fontsource.Color != null ? fontsource.Color : Colors.White).ToAndroid(),
					TextAlign = Paint.Align.Left,
					AntiAlias = true,
				};

				paint.SetTypeface(fontsource.FontFamily.ToTypeface());

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