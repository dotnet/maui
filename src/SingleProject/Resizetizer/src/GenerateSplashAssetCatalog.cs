#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Generates iOS asset catalog resources for themed launch screens.
	/// </summary>
	public class GenerateSplashAssetCatalog : Task, ILogger
	{
		[Required]
		public string IntermediateOutputPath { get; set; } = null!;

		public ITaskItem[]? MauiSplashScreen { get; set; }

		public string? InputsFile { get; set; }

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, $"Splash Screen Asset Catalog: Intermediate Path " + IntermediateOutputPath);

			try
			{
				var splash = MauiSplashScreen?.FirstOrDefault();
				if (splash is null)
					return true;

				CleanStoryboard();

				var info = ResizeImageInfo.Parse(splash);
				var lightInfo = CloneForAsset(info, DpiPath.Ios.SplashImageName);
				var lightResizer = new Resizer(lightInfo, IntermediateOutputPath, this);
				var darkInfo = CloneForAsset(info.CreateDarkVariant(), DpiPath.Ios.SplashImageDarkName);
				if (darkInfo.BaseSize is null && !string.IsNullOrWhiteSpace(info.DarkFilename))
					darkInfo.BaseSize = lightResizer.BaseSize ?? lightResizer.GetOriginalSize();

				WriteImages(lightResizer);
				WriteImages(new Resizer(darkInfo, IntermediateOutputPath, this));
				WriteImageSet();
				if (info.Color is not null || info.DarkColor is not null)
				{
					if (info.Color is null && info.DarkColor is not null)
						Log.LogWarning("MauiSplashScreen DarkColor was specified without Color; white will be used as the light-mode launch screen background.");

					WriteColorSet(info.Color ?? SKColors.White, info.DarkColor ?? info.Color ?? SKColors.White);
				}
				else
				{
					DeleteColorSet();
				}

				return !Log.HasLoggedErrors;
			}
			catch (Exception ex)
			{
				Log.LogError(Resources.ErrorMessages.AppleResourceProcessing, ErrorCodes.AppleResourceProcessingCode, null, null, 0, 0, 0, 0, string.Format(Resources.ErrorMessages.AppleResourceProcessingError, ex.ToString()));
				return false;
			}
		}

		private static ResizeImageInfo CloneForAsset(ResizeImageInfo info, string alias) =>
			new ResizeImageInfo
			{
				ItemSpec = info.ItemSpec,
				Alias = alias + Resizer.RasterFileExtension,
				Filename = info.Filename,
				BaseSize = info.BaseSize,
				Resize = info.Resize,
				TintColor = info.TintColor,
				Color = info.Color,
			};

		private void WriteImages(Resizer resizer)
		{
			foreach (var dpi in DpiPath.Ios.SplashImageAsset)
			{
				Log.LogMessage(MessageImportance.Low, $"Splash Screen Asset Catalog Resize: " + dpi);
				resizer.Resize(dpi, InputsFile);
			}
		}

		private void WriteImageSet()
		{
			var imageSetPath = Path.Combine(IntermediateOutputPath, DpiPath.Ios.SplashImageSetPath);
			Directory.CreateDirectory(imageSetPath);

			var contentsFile = Path.Combine(imageSetPath, "Contents.json");
			File.WriteAllText(contentsFile,
				$$"""
				{
				  "images": [
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImage.png",
				      "scale": "1x"
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImage@2x.png",
				      "scale": "2x"
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImage@3x.png",
				      "scale": "3x"
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImage.png",
				      "scale": "1x",
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "light"
				        }
				      ]
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImage@2x.png",
				      "scale": "2x",
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "light"
				        }
				      ]
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImage@3x.png",
				      "scale": "3x",
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "light"
				        }
				      ]
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImageDark.png",
				      "scale": "1x",
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "dark"
				        }
				      ]
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImageDark@2x.png",
				      "scale": "2x",
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "dark"
				        }
				      ]
				    },
				    {
				      "idiom": "universal",
				      "filename": "MauiSplashImageDark@3x.png",
				      "scale": "3x",
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "dark"
				        }
				      ]
				    }
				  ],
				  "info": {
				    "version": 1,
				    "author": "xcode"
				  }
				}
				""");
		}

		private void WriteColorSet(SKColor lightColor, SKColor darkColor)
		{
			var colorSetPath = Path.Combine(IntermediateOutputPath, DpiPath.Ios.SplashColorSetPath);
			Directory.CreateDirectory(colorSetPath);

			var contentsFile = Path.Combine(colorSetPath, "Contents.json");
			File.WriteAllText(contentsFile,
				$$"""
				{
				  "colors": [
				    {
				      "idiom": "universal",
				      "color": {
				        "color-space": "srgb",
				        "components": {
				          "red": "{{ToComponent(lightColor.Red)}}",
				          "green": "{{ToComponent(lightColor.Green)}}",
				          "blue": "{{ToComponent(lightColor.Blue)}}",
				          "alpha": "{{ToComponent(lightColor.Alpha)}}"
				        }
				      }
				    },
				    {
				      "idiom": "universal",
				      "color": {
				        "color-space": "srgb",
				        "components": {
				          "red": "{{ToComponent(lightColor.Red)}}",
				          "green": "{{ToComponent(lightColor.Green)}}",
				          "blue": "{{ToComponent(lightColor.Blue)}}",
				          "alpha": "{{ToComponent(lightColor.Alpha)}}"
				        }
				      },
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "light"
				        }
				      ]
				    },
				    {
				      "idiom": "universal",
				      "color": {
				        "color-space": "srgb",
				        "components": {
				          "red": "{{ToComponent(darkColor.Red)}}",
				          "green": "{{ToComponent(darkColor.Green)}}",
				          "blue": "{{ToComponent(darkColor.Blue)}}",
				          "alpha": "{{ToComponent(darkColor.Alpha)}}"
				        }
				      },
				      "appearances": [
				        {
				          "appearance": "luminosity",
				          "value": "dark"
				        }
				      ]
				    }
				  ],
				  "info": {
				    "version": 1,
				    "author": "xcode"
				  }
				}
				""");
		}

		private static string ToComponent(byte value) =>
			(value / (float)byte.MaxValue).ToString("0.#######", CultureInfo.InvariantCulture);

		private void CleanStoryboard()
		{
			var storyboard = Path.Combine(IntermediateOutputPath, "MauiSplash.storyboard");
			if (File.Exists(storyboard))
				File.Delete(storyboard);
		}

		private void DeleteColorSet()
		{
			var colorSetPath = Path.Combine(IntermediateOutputPath, DpiPath.Ios.SplashColorSetPath);
			if (Directory.Exists(colorSetPath))
				Directory.Delete(colorSetPath, recursive: true);
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}
	}
}
