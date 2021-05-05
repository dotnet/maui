using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;

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
			var outputAppIconSetDir = Path.Combine(IntermediateOutputPath, DpiPath.IosAppIconPath.Replace("{name}", AppIconName));
			var outputAssetsDir = Path.Combine(outputAppIconSetDir, "..");

			Logger.Log("iOS App Icon Set Directory: " + outputAppIconSetDir);

			Directory.CreateDirectory(outputAppIconSetDir);

			var assetContentsFile = Path.Combine(outputAssetsDir, "Contents.json");
			var appIconSetContentsFile = Path.Combine(outputAppIconSetDir, "Contents.json");

			var infoJsonProp = new JObject(
				new JProperty("info", new JObject(
					new JProperty("version", 1),
					new JProperty("author", "xcode"))));

			var appIconImagesJson = new List<JObject>();

			foreach (var dpi in Dpis)
			{
				foreach (var idiom in dpi.Idioms)
				{
					var w = dpi.Size.Value.Width.ToString("0.#", CultureInfo.InvariantCulture);
					var h = dpi.Size.Value.Height.ToString("0.#", CultureInfo.InvariantCulture);
					var s = dpi.Scale.ToString("0", CultureInfo.InvariantCulture);

					appIconImagesJson.Add(new JObject(
						new JProperty("idiom", idiom),
						new JProperty("size", $"{w}x{h}"),
						new JProperty("scale", $"{s}x"),
						new JProperty("filename", AppIconName + dpi.FileSuffix + ".png")));
				}
			}

			var appIconContentsJson = new JObject(
				new JProperty("images", appIconImagesJson.ToArray()),
				new JProperty("properties", new JObject()),
				new JProperty("info", new JObject(
					new JProperty("version", 1),
					new JProperty("author", "xcode"))));

			//File.WriteAllText(assetContentsFile, infoJsonProp.ToString());
			File.WriteAllText(appIconSetContentsFile, appIconContentsJson.ToString());

			return new List<ResizedImageInfo> {
				//new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = assetContentsFile },
				new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = appIconSetContentsFile }
			};
		}
	}
}
