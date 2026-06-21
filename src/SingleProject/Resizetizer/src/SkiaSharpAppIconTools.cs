#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpAppIconTools
	{
		public SkiaSharpAppIconTools(ResizeImageInfo info, ILogger? logger)
		{
			Info = info;
			Logger = logger;

			AppIconName = info.OutputName;

			var hasBackground = !string.IsNullOrWhiteSpace(info.Filename) && File.Exists(info.Filename);
			var hasForeground = !string.IsNullOrWhiteSpace(info.ForegroundFilename) && File.Exists(info.ForegroundFilename);
			if (!hasBackground && !hasForeground)
				throw new InvalidOperationException("An app icon needs at least one image.");

			if (hasBackground)
				backgroundTools = SkiaSharpTools.Create(info.IsVector, info.Filename, null, null, null, logger);
			if (hasForeground)
				foregroundTools = SkiaSharpTools.Create(info.ForegroundIsVector, info.ForegroundFilename, null, null, info.TintColor, logger);
		}

		SkiaSharpTools? backgroundTools;
		SkiaSharpTools? foregroundTools;

		public ResizeImageInfo Info { get; }

		public ILogger? Logger { get; }

		public string AppIconName { get; }

		public ResizedImageInfo Resize(DpiPath dpi, string destination, Stream? stream = null)
		{
			var sw = new Stopwatch();
			sw.Start();

			// 1. if an explicit size was given by the type of image, use that
			// 2. if an explicit size was given in the csproj, use that
			// 3. try determine the best size based on the background then foreground
			var (canvasSize, unscaledCanvasSize) = SkiaSharpTools.GetCanvasSize(
				dpi,
				Info.BaseSize,
				backgroundTools ?? foregroundTools);

			using (var tempBitmap = new SKBitmap(canvasSize.Width, canvasSize.Height))
			{
				Draw(tempBitmap, dpi, unscaledCanvasSize);
				Save(tempBitmap, destination, stream);
			}

			sw.Stop();
			Logger?.Log($"Save app icon took {sw.ElapsedMilliseconds}ms ({destination})");

			return new ResizedImageInfo { Dpi = dpi, Filename = destination };
		}

		void Save(SKBitmap tempBitmap, string destination, Stream? stream)
		{
			if (stream is not null)
			{
				tempBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
			}
			else
			{
				var dir = Path.GetDirectoryName(destination);
				Directory.CreateDirectory(dir);

				using var wrapper = File.Create(destination);
				tempBitmap.Encode(wrapper, SKEncodedImageFormat.Png, 100);
			}
		}

		void Draw(SKBitmap tempBitmap, DpiPath dpi, SKSize unscaledCanvasSize)
		{
			var canvasSize = tempBitmap.Info.Size;

			using var canvas = new SKCanvas(tempBitmap);

			if (GetClipPath(dpi, canvasSize, unscaledCanvasSize) is { } clipPath)
			{
				canvas.Clear(SKColors.Transparent);

				canvas.ClipPath(clipPath, antialias: true);
			}

			canvas.Clear(Info.Color ?? SKColors.Transparent);

			// draw background
			if (backgroundTools is not null)
			{
				canvas.Save();

				var backgroundOriginalSize = backgroundTools.GetOriginalSize();
				var (bgScaledSize, bgScale) = backgroundTools.GetScaledSize(backgroundOriginalSize, dpi, unscaledCanvasSize);

				// center the background
				canvas.Translate(
					(canvasSize.Width - bgScaledSize.Width) / 2,
					(canvasSize.Height - bgScaledSize.Height) / 2);

				// scale the background to the desired size
				canvas.Scale(bgScale, bgScale);

				// draw
				backgroundTools.DrawUnscaled(canvas, bgScale);

				canvas.Restore();
			}

			// draw foreground
			if (foregroundTools is not null)
			{
				var foregroundOriginalSize = foregroundTools.GetOriginalSize();
				var (fgScaledSize, fgScale) = foregroundTools.GetScaledSize(foregroundOriginalSize, dpi, unscaledCanvasSize);

				// center the foreground
				canvas.Translate(
					(canvasSize.Width - fgScaledSize.Width) / 2,
					(canvasSize.Height - fgScaledSize.Height) / 2);

				// scale the background to the desired size
				canvas.Scale(fgScale, fgScale);

				// add any foreground scale on top
				if (Info.ForegroundScale != 1.0)
				{
					var userFgScale = (float)Info.ForegroundScale;

					// add the user scale to the main scale
					fgScale *= userFgScale;

					// work out the center as if the canvas was exactly the same size as the foreground
					var fgCenterX = foregroundOriginalSize.Width / 2;
					var fgCenterY = foregroundOriginalSize.Height / 2;

					// scale to the user scale, centering
					canvas.Scale(userFgScale, userFgScale, fgCenterX, fgCenterY);
				}

				// draw
				foregroundTools.DrawUnscaled(canvas, fgScale);
			}
		}

		static SKPath? GetClipPath(DpiPath dpi, SKSize canvasSize, SKSize unscaledCanvasSize)
		{
			if (dpi.ClipShape == ClipShape.Circle)
			{
				var radius = Math.Min(canvasSize.Width, canvasSize.Height) / 2;

				var clip = new SKPath();
				clip.AddCircle(
					canvasSize.Width / 2,
					canvasSize.Height / 2,
					radius);
				return clip;
			}

			return null;
		}
	}
}
