using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.Maui.Resizetizer
{
	internal class AppleIconAssetsGenerator
	{
		public AppleIconAssetsGenerator(ResizeImageInfo info, string appIconName, string intermediateOutputPath, DpiPath[] dpis, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
			AppIconName = appIconName;
			Dpis = dpis;
		}

		public string AppIconName { get; }

		public DpiPath[] Dpis { get; }

		public ResizeImageInfo Info { get; private set; }
		public string IntermediateOutputPath { get; private set; }
		public ILogger Logger { get; private set; }

		public IEnumerable<ResizedImageInfo> Generate()
		{
			var outputAppIconSetDir = Path.Combine(IntermediateOutputPath, DpiPath.Ios.AppIconPath.Replace("{name}", AppIconName));
			var outputAssetsDir = Path.Combine(outputAppIconSetDir, "..");

			Logger.Log("iOS App Icon Set Directory: " + outputAppIconSetDir);

			Directory.CreateDirectory(outputAppIconSetDir);

			var assetContentsFile = Path.Combine(outputAssetsDir, "Contents.json");
			var appIconSetContentsFile = Path.Combine(outputAppIconSetDir, "Contents.json");

			var (sourceExists, sourceModified) = Utils.FileExists(Info.Filename);
			var (destinationExists, destinationModified) = Utils.FileExists(appIconSetContentsFile);

			if (destinationModified > sourceModified)
			{
				Logger.Log($"Skipping `{Info.Filename}` => `{appIconSetContentsFile}` file is up to date.");
				return new List<ResizedImageInfo> {
					new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = appIconSetContentsFile }
				};
			}

			var appIconImagesJson = new List<string>();

			foreach (var dpi in Dpis)
			{
				foreach (var idiom in dpi.Idioms)
				{
					var w = dpi.Size.Value.Width.ToString("0.#", CultureInfo.InvariantCulture);
					var h = dpi.Size.Value.Height.ToString("0.#", CultureInfo.InvariantCulture);
					var s = dpi.Scale.ToString("0", CultureInfo.InvariantCulture);

					var imageIcon =
					$$"""
							{
								"idiom": "{{idiom}}",
								"size": "{{w}}x{{h}}",
								"scale": "{{s}}x",
								"filename": "{{AppIconName + dpi.FileSuffix + Resizer.RasterFileExtension}}"
							}
					""";

					appIconImagesJson.Add(imageIcon);
				}
			}

			var appIconContentsJson =
			$$"""
			{
				"images": [
			{{string.Join("," + Environment.NewLine, appIconImagesJson)}}
				],
				"properties": {},
				"info": {
					"version": 1,
					"author": "xcode"
				}
			}
			""";

			File.WriteAllText(appIconSetContentsFile, appIconContentsJson.Replace("\t", "  "));

			return new List<ResizedImageInfo> {
				//new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = assetContentsFile },
				new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = appIconSetContentsFile }
			};
		}
	}
}
