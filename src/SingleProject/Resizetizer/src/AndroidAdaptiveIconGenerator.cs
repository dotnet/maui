using System.Collections.Generic;
using System.IO;

namespace Microsoft.Maui.Resizetizer
{
	internal class AndroidAdaptiveIconGenerator
	{
		public AndroidAdaptiveIconGenerator(ResizeImageInfo info, string appIconName, string intermediateOutputPath, ILogger logger, bool useVectors)
		{
			Info = info;
			Logger = logger;
			UseVectors = useVectors;
			IntermediateOutputPath = intermediateOutputPath;
			AppIconName = appIconName;
		}

		public ResizeImageInfo Info { get; }
		public string IntermediateOutputPath { get; }
		public ILogger Logger { get; private set; }
		public bool UseVectors { get; }
		public string AppIconName { get; }

		const string AdaptiveIconDrawableXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<adaptive-icon xmlns:android=""http://schemas.android.com/apk/res/android"">
	<background android:drawable=""@{backgroundType}/{name}_background""/>
	<foreground android:drawable=""@{foregroundType}/{name}_foreground""/>
</adaptive-icon>";

		const string EmptyVectorDrawable =
@"<vector xmlns:android=""http://schemas.android.com/apk/res/android"" xmlns:aapt=""http://schemas.android.com/aapt""
	android:viewportWidth=""1024""
	android:viewportHeight=""1024""
	android:width=""1024dp""
	android:height=""1024dp"" />
";

		public IEnumerable<ResizedImageInfo> Generate()
		{
			var results = new List<ResizedImageInfo>();

			var fullIntermediateOutputPath = new DirectoryInfo(IntermediateOutputPath);

			var backgroundFile = Info.Filename;
			var backgroundIsVector = UseVectors && Info.IsVector;
			var backgroundExt = backgroundIsVector ? ".xml" : ".png";
			var backgroundDestFilename = AppIconName + "_background" + backgroundExt;

			var foregroundFile = Info.ForegroundFilename;
			var foregroundExists = File.Exists(foregroundFile);
			var foregroundIsVector = !foregroundExists || (UseVectors && Info.ForegroundIsVector);
			var foregroundExt = foregroundIsVector ? ".xml" : ".png";
			var foregroundDestFilename = AppIconName + "_foreground" + foregroundExt;

			if (backgroundIsVector)
			{
				Logger.Log("Converting Background SVG to Android Drawable Vector: " + backgroundFile);

				var dir = Path.Combine(fullIntermediateOutputPath.FullName, "drawable-v24");
				var destination = Path.Combine(dir, backgroundDestFilename);
				Directory.CreateDirectory(dir);

				Svg2VectorDrawable.Svg2Vector.Convert(backgroundFile, destination);

				results.Add(new ResizedImageInfo { Dpi = new DpiPath("drawable-v24", 1, "_background"), Filename = destination });
			}
			else
			{
				Logger.Log("Converting Background SVG to PNG: " + backgroundFile);

				foreach (var dpi in DpiPath.Android.AppIconParts)
				{
					var dir = Path.Combine(fullIntermediateOutputPath.FullName, dpi.Path);
					var destination = Path.Combine(dir, backgroundDestFilename);
					Directory.CreateDirectory(dir);

					Logger.Log($"App Icon Background Part: " + destination);

					var tools = SkiaSharpTools.Create(Info.IsVector, Info.Filename, dpi.Size, null, Logger);
					tools.Resize(dpi, destination);

					results.Add(new ResizedImageInfo { Dpi = dpi, Filename = destination });
				}
			}

			Logger.Log("Looking for Foreground File: " + foregroundFile);

			var foregroundDestinationDir = Path.Combine(fullIntermediateOutputPath.FullName, "drawable");
			var foregroundDestination = Path.Combine(foregroundDestinationDir, foregroundDestFilename);
			Directory.CreateDirectory(foregroundDestinationDir);

			if (foregroundExists)
			{
				if (foregroundIsVector)
				{
					Logger.Log("Converting Foreground SVG to Android Drawable Vector: " + foregroundFile);
					Svg2VectorDrawable.Svg2Vector.Convert(foregroundFile, foregroundDestination);

					results.Add(new ResizedImageInfo { Dpi = new DpiPath("drawable", 1, "_foreground"), Filename = foregroundDestination });
				}
				else
				{
					Logger.Log("Converting Foreground SVG to PNG: " + foregroundFile);

					foreach (var dpi in DpiPath.Android.AppIconParts)
					{
						var dir = Path.Combine(fullIntermediateOutputPath.FullName, dpi.Path);
						var destination = Path.Combine(dir, foregroundDestFilename);
						Directory.CreateDirectory(dir);

						Logger.Log($"App Icon Foreground Part: " + destination);

						var tools = SkiaSharpTools.Create(Info.ForegroundIsVector, Info.ForegroundFilename, dpi.Size, null, Logger);
						tools.Resize(dpi, destination);

						results.Add(new ResizedImageInfo { Dpi = dpi, Filename = destination });
					}
				}
			}
			else
			{
				Logger.Log("Foreground was not found: " + foregroundFile);

				File.WriteAllText(foregroundDestination, EmptyVectorDrawable);
			}

			// process adaptive icon xml
			{
				var adaptiveIconXmlStr = AdaptiveIconDrawableXml
					.Replace("{name}", AppIconName)
					.Replace("{backgroundType}", backgroundIsVector ? "drawable" : "mipmap")
					.Replace("{foregroundType}", foregroundIsVector ? "drawable" : "mipmap");

				var dir = Path.Combine(fullIntermediateOutputPath.FullName, "mipmap-anydpi-v26");
				var adaptiveIconDestination = Path.Combine(dir, AppIconName + ".xml");
				var adaptiveIconRoundDestination = Path.Combine(dir, AppIconName + "_round.xml");
				Directory.CreateDirectory(dir);

				// Write out the adaptive icon xml drawables
				File.WriteAllText(adaptiveIconDestination, adaptiveIconXmlStr);
				File.WriteAllText(adaptiveIconRoundDestination, adaptiveIconXmlStr);

				results.Add(new ResizedImageInfo { Dpi = new DpiPath("mipmap-anydpi-v26", 1), Filename = adaptiveIconDestination });
				results.Add(new ResizedImageInfo { Dpi = new DpiPath("mipmap-anydpi-v26", 1, "_round"), Filename = adaptiveIconRoundDestination });
			}

			return results;
		}
	}
}
