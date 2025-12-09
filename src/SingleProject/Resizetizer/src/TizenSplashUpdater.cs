using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	public class TizenSplashUpdater : Task
	{
		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public ILogger Logger { get; private set; }

		static public Dictionary<(string Resolution, string Orientation), string> splashDpiMap = new Dictionary<(string, string), string>();

		const string splashDirectoryName = "splash";
		SKSizeI hdSize = new SKSizeI(720, 1080);
		SKSizeI fhdSize = new SKSizeI(1080, 1920);

		public override bool Execute()
		{
			var orientations = new List<string>() { "portrait", "landscape" };
			var splashInfo = ResizeImageInfo.Parse(MauiSplashScreen[0]);
			var image = splashInfo.OutputName + ".png";
			var splashFullPath = Path.Combine(IntermediateOutputPath, splashDirectoryName);

			if (Directory.Exists(splashFullPath))
				Directory.Delete(splashFullPath, true);
			Directory.CreateDirectory(splashFullPath);

			var appTool = new SkiaSharpAppIconTools(splashInfo, Logger);

			splashDpiMap.Clear();
			foreach (var dpi in DpiPath.Tizen.SplashScreen)
			{
				var destination = Resizer.GetRasterFileDestination(splashInfo, dpi, IntermediateOutputPath);
				appTool.Resize(dpi, destination);

				if (File.Exists(destination))
				{
					var resolution = dpi.Path.Split('-')[1].ToLowerInvariant();
					foreach (var orientation in orientations)
					{
						var newImage = splashInfo.OutputName + "." + resolution + "." + orientation + ".png";
						if (splashDpiMap.ContainsKey((resolution, orientation)))
						{
							splashDpiMap.Remove((resolution, orientation));
						}
						splashDpiMap.Add((resolution, orientation), $"{splashDirectoryName}/{newImage}");
						UpdateColorAndMoveFile(splashInfo, GetScreenSize(resolution, orientation), destination, Path.Combine(splashFullPath, newImage));
					}
				}
			}

			return true;
		}

		SKSizeI GetScreenSize(string resolution, string orientation) =>
			resolution switch
			{
				"mdpi" => orientation == "portrait" ? hdSize : new SKSizeI(hdSize.Height, hdSize.Width),
				_ => orientation == "portrait" ? fhdSize : new SKSizeI(fhdSize.Height, fhdSize.Width)
			};

		void UpdateColorAndMoveFile(ResizeImageInfo splashInfo, SKSizeI screenSize, string sourceFilePath, string destFilePath)
		{
			var color = splashInfo.Color;
			if (color == null)
			{
				Log.LogWarning($"Unable to parse color for '{splashInfo.Filename}'.");
				color = SKColors.White;
			}

			using var img = SKImage.FromEncodedData(sourceFilePath);

			var info = new SKImageInfo(screenSize.Width, screenSize.Height);
			using var surface = SKSurface.Create(info);

			var canvas = surface.Canvas;
			canvas.Clear(color.Value);

			using SKPaint paint = new SKPaint
			{
				IsAntialias = true,
#pragma warning disable CS0618 // Type or member is obsolete
				FilterQuality = SKFilterQuality.High
#pragma warning restore CS0618 // Type or member is obsolete
			};
			var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);

			var left = screenSize.Width <= img.Width ? 0 : (screenSize.Width - img.Width) / 2;
			var top = screenSize.Height <= img.Height ? 0 : (screenSize.Height - img.Height) / 2;
			var right = screenSize.Width <= img.Width ? left + screenSize.Width : left + img.Width;
			var bottom = screenSize.Height <= img.Height ? top + screenSize.Height : top + img.Height;
			var dest = new SKRect(left, top, right, bottom);

			canvas.DrawImage(img, dest, sampling, paint);
			canvas.Flush();

			using var updatedsplash = surface.Snapshot();

			using var data = updatedsplash.Encode(SKEncodedImageFormat.Png, 100);
			using var stream = File.Create(destFilePath);
			data.SaveTo(stream);
		}
	}
}
