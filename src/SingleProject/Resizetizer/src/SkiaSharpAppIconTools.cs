using System.Diagnostics;
using System.IO;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpAppIconTools
	{
		public SkiaSharpAppIconTools(ResizeImageInfo info, ILogger logger)
		{
			Info = info;
			Logger = logger;

			AppIconName = info.OutputName;

			hasForeground = File.Exists(info.ForegroundFilename);

			if (hasForeground)
				foregroundTools = SkiaSharpTools.Create(info.ForegroundIsVector, info.ForegroundFilename, null, info.TintColor, logger);

			backgroundTools = SkiaSharpTools.Create(info.IsVector, info.Filename, null, null, logger);

			backgroundOriginalSize = backgroundTools.GetOriginalSize();

			if (hasForeground)
				foregroundOriginalSize = foregroundTools.GetOriginalSize();
		}

		bool hasForeground = false;

		SkiaSharpTools backgroundTools;
		SkiaSharpTools foregroundTools;

		SKSize foregroundOriginalSize;
		SKSize backgroundOriginalSize;

		public ResizeImageInfo Info { get; }

		public ILogger Logger { get; }

		public string AppIconName { get; }

		public ResizedImageInfo Resize(DpiPath dpi, string destination)
		{
			var sw = new Stopwatch();
			sw.Start();

			var (bgScaledSize, bgScale) = backgroundTools.GetScaledSize(backgroundOriginalSize, dpi);

			// Make the canvas size match the desired size
			var canvasSize = bgScaledSize;
			if (dpi.Size is SKSize size && size.Width != size.Height)
			{
				var scale = (float)dpi.Scale;
				canvasSize = new SKSizeI((int)(size.Width * scale), (int)(size.Height * scale));
			}

			// Allocate
			using (var tempBitmap = new SKBitmap(canvasSize.Width, canvasSize.Height))
			{
				// Draw (copy)
				using (var canvas = new SKCanvas(tempBitmap))
				{
					canvas.Clear(Info.Color ?? SKColors.Transparent);
					canvas.Save();
					canvas.Translate(
						(canvasSize.Width - bgScaledSize.Width) / 2,
						(canvasSize.Height - bgScaledSize.Height) / 2);
					canvas.Scale(bgScale, bgScale);

					backgroundTools.DrawUnscaled(canvas, bgScale);
					canvas.Restore();

					if (hasForeground)
					{
						var userFgScale = (float)Info.ForegroundScale;

						// get the ratio to make the foreground fill the background
						var fitRatio = bgScaledSize.Width / foregroundOriginalSize.Width;

						// calculate the scale for the foreground to fit the background exactly
						var (fgScaledSize, fgScale) = foregroundTools.GetScaledSize(foregroundOriginalSize, (decimal)fitRatio);

						//Logger.Log("\tdpi.Size: " + dpi.Size);
						//Logger.Log("\tdpi.Scale: " + dpi.Scale);
						//Logger.Log("\tbgScaledSize: " + bgScaledSize);
						//Logger.Log("\tbgScale: " + bgScale);
						//Logger.Log("\tforegroundOriginalSize: " + foregroundOriginalSize);
						//Logger.Log("\tfgScaledSize: " + fgScaledSize);
						//Logger.Log("\tfgScale: " + fgScale);
						//Logger.Log("\tuserFgScale: " + userFgScale);

						// now work out the center as if the canvas was exactly the same size as the foreground
						var fgScaledSizeCenterX = foregroundOriginalSize.Width / 2;
						var fgScaledSizeCenterY = foregroundOriginalSize.Height / 2;

						//Logger.Log("\tfgScaledSizeCenterX: " + fgScaledSizeCenterX);
						//Logger.Log("\tfgScaledSizeCenterY: " + fgScaledSizeCenterY);

						// center the foreground
						canvas.Translate(
							(canvasSize.Width - fgScaledSize.Width) / 2,
							(canvasSize.Height - fgScaledSize.Height) / 2);

						// scale so the forground is the same size as the background
						canvas.Scale(fgScale, fgScale);

						// scale to the user scale, centering
						canvas.Scale(userFgScale, userFgScale, fgScaledSizeCenterX, fgScaledSizeCenterY);

						foregroundTools.DrawUnscaled(canvas, fgScale * userFgScale);
					}
				}

				// Save (encode)
				using var wrapper = File.Create(destination);
				tempBitmap.Encode(wrapper, SKEncodedImageFormat.Png, 100);
			}

			sw.Stop();
			Logger?.Log($"Save app icon took {sw.ElapsedMilliseconds}ms ({destination})");

			return new ResizedImageInfo { Dpi = dpi, Filename = destination };
		}
	}
}
