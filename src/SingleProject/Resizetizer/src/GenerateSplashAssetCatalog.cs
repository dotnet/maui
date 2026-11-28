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
	[MSBuildMultiThreadableTask]
	public class GenerateSplashAssetCatalog : MauiTask, ILogger
	{
		[Required]
		public string IntermediateOutputPath { get; set; } = null!;

		public ITaskItem[]? MauiSplashScreen { get; set; }

		public string? InputsFile { get; set; }

		string intermediateOutputPath = null!;
		string? inputsFile;
		FileRetryPolicy retryPolicy = FileRetryPolicy.Default;

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, $"Splash Screen Asset Catalog: Intermediate Path " + IntermediateOutputPath);

			try
			{
				intermediateOutputPath = TaskEnvironment.GetAbsolutePath(IntermediateOutputPath);
				inputsFile = InputsFile is null or "" ? InputsFile : TaskEnvironment.GetAbsolutePath(InputsFile);
				retryPolicy = FileRetryPolicy.FromEnvironment(TaskEnvironment);

				var splash = MauiSplashScreen?.FirstOrDefault();
				if (splash is null)
					return true;

				CleanStoryboard();
				CleanImageSet();

				var info = ResizeImageInfo.Parse(splash, TaskEnvironment);
				var lightInfo = CloneForAsset(info, DpiPath.Ios.SplashImageName);
				var lightResizer = new Resizer(lightInfo, intermediateOutputPath, this) { RetryPolicy = retryPolicy };
				var darkInfo = CloneForAsset(info.CreateDarkVariant(), DpiPath.Ios.SplashImageDarkName);
				if (darkInfo.BaseSize is null && !string.IsNullOrWhiteSpace(info.DarkFilename))
					darkInfo.BaseSize = lightResizer.BaseSize ?? lightResizer.GetOriginalSize();

				WriteImages(lightResizer);
				WriteImages(new Resizer(darkInfo, intermediateOutputPath, this) { RetryPolicy = retryPolicy });
				WriteImageSet(lightInfo, darkInfo);
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
				Alias = alias + GetAssetExtension(info),
				Filename = info.Filename,
				BaseSize = info.BaseSize,
				Resize = info.Resize,
				TintColor = info.TintColor,
				Color = info.Color,
				Quality = info.Quality,
			};

		private static string GetAssetExtension(ResizeImageInfo info) =>
			!info.Resize && !info.IsVector && !string.IsNullOrEmpty(info.OutputExtension)
				? info.OutputExtension
				: Resizer.RasterFileExtension;

		private void WriteImages(Resizer resizer)
		{
			foreach (var dpi in DpiPath.Ios.SplashImageAsset)
			{
				if (resizer.Info.Resize)
				{
					Log.LogMessage(MessageImportance.Low, $"Splash Screen Asset Catalog Resize: " + dpi);
					resizer.Resize(dpi, inputsFile);
				}
				else
				{
					Log.LogMessage(MessageImportance.Low, $"Splash Screen Asset Catalog Copy: " + dpi);
					resizer.CopyFile(dpi, inputsFile);
				}
			}
		}

		private void WriteImageSet(ResizeImageInfo lightInfo, ResizeImageInfo darkInfo)
		{
			var imageSetPath = Path.Combine(intermediateOutputPath, DpiPath.Ios.SplashImageSetPath);
			Directory.CreateDirectory(new AbsolutePath(imageSetPath));

			var contentsFile = Path.Combine(imageSetPath, "Contents.json");
			File.WriteAllText(new AbsolutePath(contentsFile),
				$$"""
				{
				  "images": [
				    {
				      "idiom": "universal",
				      "filename": "{{GetAssetFilename(lightInfo, DpiPath.Ios.SplashImageAsset[0])}}",
				      "scale": "1x"
				    },
				    {
				      "idiom": "universal",
				      "filename": "{{GetAssetFilename(lightInfo, DpiPath.Ios.SplashImageAsset[1])}}",
				      "scale": "2x"
				    },
				    {
				      "idiom": "universal",
				      "filename": "{{GetAssetFilename(lightInfo, DpiPath.Ios.SplashImageAsset[2])}}",
				      "scale": "3x"
				    },
				    {
				      "idiom": "universal",
				      "filename": "{{GetAssetFilename(lightInfo, DpiPath.Ios.SplashImageAsset[0])}}",
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
				      "filename": "{{GetAssetFilename(lightInfo, DpiPath.Ios.SplashImageAsset[1])}}",
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
				      "filename": "{{GetAssetFilename(lightInfo, DpiPath.Ios.SplashImageAsset[2])}}",
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
				      "filename": "{{GetAssetFilename(darkInfo, DpiPath.Ios.SplashImageAsset[0])}}",
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
				      "filename": "{{GetAssetFilename(darkInfo, DpiPath.Ios.SplashImageAsset[1])}}",
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
				      "filename": "{{GetAssetFilename(darkInfo, DpiPath.Ios.SplashImageAsset[2])}}",
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

		private static string GetAssetFilename(ResizeImageInfo info, DpiPath dpi) =>
			info.OutputName + dpi.FileSuffix + info.OutputExtension;

		private void WriteColorSet(SKColor lightColor, SKColor darkColor)
		{
			var colorSetPath = Path.Combine(intermediateOutputPath, DpiPath.Ios.SplashColorSetPath);
			Directory.CreateDirectory(new AbsolutePath(colorSetPath));

			var contentsFile = Path.Combine(colorSetPath, "Contents.json");
			File.WriteAllText(new AbsolutePath(contentsFile),
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
			value == byte.MaxValue
				? "1.0"
				: (value / (float)byte.MaxValue).ToString("0.#######", CultureInfo.InvariantCulture);

		private void CleanStoryboard()
		{
			var storyboard = Path.Combine(intermediateOutputPath, "MauiSplash.storyboard");
			if (File.Exists(new AbsolutePath(storyboard)))
				Utils.DeleteFileWithRetries(storyboard, retryPolicy);
		}

		private void CleanImageSet()
		{
			var imageSetPath = Path.Combine(intermediateOutputPath, DpiPath.Ios.SplashImageSetPath);
			if (Directory.Exists(new AbsolutePath(imageSetPath)))
				Utils.DeleteDirectoryWithRetries(imageSetPath, retryPolicy);
		}

		private void DeleteColorSet()
		{
			var colorSetPath = Path.Combine(intermediateOutputPath, DpiPath.Ios.SplashColorSetPath);
			if (Directory.Exists(new AbsolutePath(colorSetPath)))
				Utils.DeleteDirectoryWithRetries(colorSetPath, retryPolicy);
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}
	}
}
