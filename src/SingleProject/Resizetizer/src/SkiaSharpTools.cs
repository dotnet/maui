using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal abstract class SkiaSharpTools
	{
		const int ERROR_ACCESS_DENIED = -2147024891;
		const int ERROR_SHARING_VIOLATION = -2147024864;
		const int DEFAULT_FILE_WRITE_RETRY_ATTEMPTS = 10;
		const int DEFAULT_FILE_WRITE_RETRY_DELAY_MS = 1000;

		static int fileWriteRetry = -1;
		static int fileWriteRetryDelay = -1;

		/// <summary>
		/// Checks for the environment variable DOTNET_ANDROID_FILE_WRITE_RETRY_ATTEMPTS to
		/// see if a custom value for the number of times to retry writing a file has been 
		/// set.
		/// </summary>
		/// <returns>The value of DOTNET_ANDROID_FILE_WRITE_RETRY_ATTEMPTS or the default of DEFAULT_FILE_WRITE_RETRY_ATTEMPTS</returns>
		public static int GetFileWriteRetryAttempts()
		{
			if (fileWriteRetry == -1)
			{
				var retryVariable = Environment.GetEnvironmentVariable("DOTNET_ANDROID_FILE_WRITE_RETRY_ATTEMPTS");
				if (string.IsNullOrEmpty(retryVariable) || !int.TryParse(retryVariable, out fileWriteRetry))
					fileWriteRetry = DEFAULT_FILE_WRITE_RETRY_ATTEMPTS;
			}
			return fileWriteRetry;
		}

		/// <summary>
		/// Checks for the environment variable DOTNET_ANDROID_FILE_WRITE_RETRY_DELAY_MS to
		/// see if a custom value for the delay between trying to write a file has been 
		/// set.
		/// </summary>
		/// <returns>The value of DOTNET_ANDROID_FILE_WRITE_RETRY_DELAY_MS or the default of DEFAULT_FILE_WRITE_RETRY_DELAY_MS</returns>
		public static int GetFileWriteRetryDelay()
		{
			if (fileWriteRetryDelay == -1)
			{
				var delayVariable = Environment.GetEnvironmentVariable("DOTNET_ANDROID_FILE_WRITE_RETRY_DELAY_MS");
				if (string.IsNullOrEmpty(delayVariable) || !int.TryParse(delayVariable, out fileWriteRetryDelay))
					fileWriteRetryDelay = DEFAULT_FILE_WRITE_RETRY_DELAY_MS;
			}
			return fileWriteRetryDelay;
		}

		static SkiaSharpTools()
		{
			// DO NOT DELETE!
			// Because we are doing dangerous things - like using a net462 assembly in a netstandard2.0 assembly
			// we need to make sure the correct dependencies are loaded. We use net462 because this has special
			// native library loading logic for the .NET Framework (Visual Studio). 
			var span = (Span<SKPoint>)new SKPoint[1];
			span[0] = new SKPoint();
		}

		public static SkiaSharpTools Create(bool isVector, string filename, SKSize? baseSize, SKColor? backgroundColor, SKColor? tintColor, ILogger logger)
			=> isVector
				? new SkiaSharpSvgTools(filename, baseSize, backgroundColor, tintColor, logger) as SkiaSharpTools
				: new SkiaSharpBitmapTools(filename, baseSize, backgroundColor, tintColor, logger);

		public static SkiaSharpTools CreateImaginary(SKColor? backgroundColor, ILogger logger)
			=> new SkiaSharpImaginaryTools(backgroundColor, logger);

		public SkiaSharpTools(ResizeImageInfo info, ILogger logger)
			: this(info.Filename, info.BaseSize, info.Color, info.TintColor, logger)
		{
		}

		public SkiaSharpTools(string filename, SKSize? baseSize, SKColor? backgroundColor, SKColor? tintColor, ILogger logger)
		{
			Logger = logger;
			Filename = filename;
			BaseSize = baseSize;
			BackgroundColor = backgroundColor;
			Paint = new SKPaint
			{
				FilterQuality = SKFilterQuality.High
			};

			if (tintColor is SKColor tint)
			{
				Logger?.Log($"Detected a tint color of {tint}");
				Paint.ColorFilter = SKColorFilter.CreateBlendMode(tint, SKBlendMode.SrcIn);
			}
		}

		public string Filename { get; }

		public SKSize? BaseSize { get; }

		public SKColor? BackgroundColor { get; }

		public ILogger Logger { get; }

		public SKPaint Paint { get; }

		public void Resize(DpiPath dpi, string destination, double additionalScale = 1.0, bool dpiSizeIsAbsolute = false)
		{
			var sw = new Stopwatch();
			sw.Start();

			var originalSize = GetOriginalSize();
			var absoluteSize = dpiSizeIsAbsolute ? dpi.Size : null;
			var (scaledSize, scale) = GetScaledSize(originalSize, dpi, absoluteSize);
			var (canvasSize, _) = GetCanvasSize(dpi, null, this);

			using (var tempBitmap = new SKBitmap(canvasSize.Width, canvasSize.Height))
			{
				Draw(tempBitmap, additionalScale, originalSize, scale, scaledSize);
				Save(destination, tempBitmap);
			}

			sw.Stop();
			Logger?.Log($"Save Image took {sw.ElapsedMilliseconds}ms ({destination})");
		}

		public static (SKSizeI Scaled, SKSize Unscaled) GetCanvasSize(DpiPath dpi, SKSize? baseSize = null, SkiaSharpTools baseTools = null)
		{
			// if an explicit size was given by the type of image, use that
			if (dpi.Size is SKSize size)
			{
				var scale = (float)dpi.Scale;
				var scaled = new SKSizeI(
					(int)(size.Width * scale),
					(int)(size.Height * scale));
				return (scaled, size);
			}

			// if an explicit size was given in the csproj, use that
			if (baseSize is SKSize bs)
			{
				var scale = (float)dpi.Scale;
				var scaled = new SKSizeI(
					(int)(bs.Width * scale),
					(int)(bs.Height * scale));
				return (scaled, bs);
			}

			// try determine the best size based on the loaded image
			if (baseTools is not null)
			{
				var baseOriginalSize = baseTools.GetOriginalSize();
				var (baseScaledSize, _) = baseTools.GetScaledSize(baseOriginalSize, dpi.Scale);
				return (baseScaledSize, baseOriginalSize);
			}

			throw new InvalidOperationException("The canvas size cannot be calculated if there is no size to start from (DPI size, BaseSize or image size).");
		}

		void Draw(SKBitmap tempBitmap, double additionalScale, SKSize originalSize, float scale, SKSizeI scaledSize)
		{
			using var canvas = new SKCanvas(tempBitmap);

			var canvasSize = tempBitmap.Info.Size;

			// clear
			canvas.Clear(BackgroundColor ?? SKColors.Transparent);

			// center the drawing
			canvas.Translate(
				(canvasSize.Width - scaledSize.Width) / 2,
				(canvasSize.Height - scaledSize.Height) / 2);

			// apply initial scale to size the image to fit the canvas
			canvas.Scale(scale, scale);

			// apply additional user scaling
			if (additionalScale != 1.0)
			{
				var userFgScale = (float)additionalScale;

				// add the user scale to the main scale
				scale *= userFgScale;

				// work out the center as if the canvas was exactly the same size as the foreground
				var fgCenterX = originalSize.Width / 2;
				var fgCenterY = originalSize.Height / 2;

				// scale to the user scale, centering
				canvas.Scale(userFgScale, userFgScale, fgCenterX, fgCenterY);
			}

			// draw
			DrawUnscaled(canvas, scale);
		}

		void Save(string destination, SKBitmap tempBitmap)
		{
			int attempt = 0;
			int attempts = GetFileWriteRetryAttempts();
			int delay = GetFileWriteRetryDelay();
			while (attempt <= attempts)
			{
				try
				{
					using var stream = File.Create(destination);
					tempBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
					return;
				}
				catch (Exception ex)
				{
					switch (ex)
					{
						case UnauthorizedAccessException:
						case IOException:
							var code = Marshal.GetHRForException(ex);
							if ((code != ERROR_ACCESS_DENIED && code != ERROR_SHARING_VIOLATION) || attempt >= attempts)
							{
								throw;
							}
							break;
						default:
							throw;
					}
					attempt++;
					Thread.Sleep(delay);
				}
			}
		}

		public abstract SKSize GetOriginalSize();

		public abstract void DrawUnscaled(SKCanvas canvas, float scale);

		public (SKSizeI, float) GetScaledSize(SKSize originalSize, DpiPath dpi, SKSize? absoluteSize = null) =>
			GetScaledSize(originalSize, dpi.Scale, absoluteSize ?? dpi.Size);

		public (SKSizeI, float) GetScaledSize(SKSize originalSize, decimal resizeRatio, SKSize? absoluteSize = null)
		{
			var sourceNominalWidth = (int)(absoluteSize?.Width ?? BaseSize?.Width ?? originalSize.Width);
			var sourceNominalHeight = (int)(absoluteSize?.Height ?? BaseSize?.Height ?? originalSize.Height);

			// Find the actual size of the image
			var sourceActualWidth = (double)originalSize.Width;
			var sourceActualHeight = (double)originalSize.Height;

			// Figure out what the ratio to convert the actual image size to the nominal size is
			var nominalRatio = Math.Min(
				sourceNominalWidth / sourceActualWidth,
				sourceNominalHeight / sourceActualHeight);

			// Multiply nominal ratio by the resize ratio to get our final ratio we actually adjust by
			var adjustRatio = nominalRatio * (double)resizeRatio;

			// Figure out our scaled width and height to make a new canvas for
			var scaledWidth = sourceActualWidth * adjustRatio;
			var scaledHeight = sourceActualHeight * adjustRatio;
			var scaledSize = new SKSizeI(
				(int)Math.Round(scaledWidth),
				(int)Math.Round(scaledHeight));

			return (scaledSize, (float)adjustRatio);
		}
	}
}
