using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class ResizetizeImages : MauiAsyncTask, ILogger
	{
		[Required]
		public string PlatformType { get; set; } = "android";

		[Required]
		public string IntermediateOutputPath { get; set; }

		public bool ThrowsErrorOnDuplicateOutput { get; set; } = true;

		public string DuplicateOutputErrorMessage { get; set; }

		public string InputsFile { get; set; }

		public ITaskItem[] Images { get; set; }

		[Output]
		public ITaskItem[] CopiedResources { get; set; }

		public string IsMacEnabled { get; set; }

		public ILogger Logger => this;

		public override System.Threading.Tasks.Task ExecuteAsync()
		{
			var inputImages = ResizeImageInfo.Parse(Images);
			var images = RemoveDuplicates(inputImages);

			var dpis = DpiPath.GetDpis(PlatformType);

			if (dpis == null || dpis.Length <= 0)
				return System.Threading.Tasks.Task.CompletedTask;

			var originalScaleDpi = DpiPath.GetOriginal(PlatformType);

			var resizedImages = new ConcurrentBag<ResizedImageInfo>();

			this.ParallelForEach(images, img =>
			{
				try
				{
					var opStopwatch = new Stopwatch();
					opStopwatch.Start();

					string op;

					if (img.IsAppIcon)
					{
						// App icons are special
						ProcessAppIcon(img, resizedImages);

						op = "App Icon";
					}
					else
					{
						// By default we resize, but let's make sure
						if (img.Resize)
						{
							ProcessImageResize(img, dpis, resizedImages);

							op = "Resize";
						}
						else
						{
							// Otherwise just copy the thing over to the 1.0 scale
							ProcessImageCopy(img, originalScaleDpi, resizedImages);

							op = "Copy";
						}
					}

					opStopwatch.Stop();

					LogDebugMessage($"{op} took {opStopwatch.ElapsedMilliseconds}ms");
				}
				catch (Exception ex)
				{
					LogCodedError("MAUI0000", $"There was an exception processing the image '{img.Filename}': {ex}");
				}
			});

			if (PlatformType == "tizen")
			{
				var tizenResourceXmlGenerator = new TizenResourceXmlGenerator(IntermediateOutputPath, Logger);
				var r = tizenResourceXmlGenerator.Generate();
				if (r is not null)
					resizedImages.Add(r);
			}

			var copiedResources = new List<TaskItem>();

			foreach (var img in resizedImages)
			{
				var attr = new Dictionary<string, string>(StringComparer.Ordinal);
				string itemSpec = Path.GetFullPath(img.Filename);

				// Fix the item spec to be relative for mac
				if (bool.TryParse(IsMacEnabled, out bool isMac) && isMac)
					itemSpec = img.Filename;

				// Add DPI info to the itemspec so we can use it in the targets
				attr.Add("_ResizetizerDpiPath", img.Dpi.Path);
				attr.Add("_ResizetizerDpiScale", img.Dpi.Scale.ToString("0.0", CultureInfo.InvariantCulture));

				copiedResources.Add(new TaskItem(itemSpec, attr));
			}

			CopiedResources = copiedResources.ToArray();

			return System.Threading.Tasks.Task.CompletedTask;
		}

		IEnumerable<ResizeImageInfo> RemoveDuplicates(IEnumerable<ResizeImageInfo> inputImages)
		{
			var imagesPairs = new Dictionary<string, ResizeImageInfo>();

			var builder = new StringBuilder();
			builder.Append(DuplicateOutputErrorMessage);

			var hasDuplicates = false;
			foreach (var image in inputImages)
			{
				if (imagesPairs.ContainsKey(image.OutputName))
				{
					if (hasDuplicates)
						builder.Append(", ");

					builder.Append($"{image.OutputName} ({image.ItemSpec})");

					hasDuplicates = true;
				}

				imagesPairs[image.OutputName] = image;
			}

			if (hasDuplicates)
			{
				if (ThrowsErrorOnDuplicateOutput)
					Log.LogError(builder.ToString());
				else
					Log.LogMessage(builder.ToString());
			}

			return imagesPairs.Values;
		}

		void ProcessAppIcon(ResizeImageInfo img, ConcurrentBag<ResizedImageInfo> resizedImages)
		{
			var appIconName = img.OutputName;

			// Generate the actual bitmap app icons themselves
			var appIconDpis = DpiPath.GetAppIconDpis(PlatformType, appIconName);

			LogDebugMessage($"App Icon");

			// Apple and Android have special additional files to generate for app icons
			if (PlatformType == "android")
			{
				LogDebugMessage($"Android Adaptive Icon Generator");

				appIconName = appIconName.ToLowerInvariant();

				var adaptiveIconGen = new AndroidAdaptiveIconGenerator(img, appIconName, IntermediateOutputPath, this);
				var iconsGenerated = adaptiveIconGen.Generate();

				foreach (var iconGenerated in iconsGenerated)
					resizedImages.Add(iconGenerated);
			}
			else if (PlatformType == "ios")
			{
				LogDebugMessage($"iOS Icon Assets Generator");

				var appleAssetGen = new AppleIconAssetsGenerator(img, appIconName, IntermediateOutputPath, appIconDpis, this);

				var assetsGenerated = appleAssetGen.Generate();

				foreach (var assetGenerated in assetsGenerated)
					resizedImages.Add(assetGenerated);
			}
			else if (PlatformType == "uwp")
			{
				LogDebugMessage($"Windows Icon Generator");

				var windowsIconGen = new WindowsIconGenerator(img, IntermediateOutputPath, this);

				resizedImages.Add(windowsIconGen.Generate());
			}

			LogDebugMessage($"Generating App Icon Bitmaps for DPIs");

			var appTool = new SkiaSharpAppIconTools(img, this);

			LogDebugMessage($"App Icon: Intermediate Path " + IntermediateOutputPath);

			foreach (var dpi in appIconDpis)
			{
				LogDebugMessage($"App Icon: " + dpi);

				var destination = Resizer.GetRasterFileDestination(img, dpi, IntermediateOutputPath)
					.Replace("{name}", appIconName);
				var (sourceExists, sourceModified) = Utils.FileExists(img.Filename);
				var (destinationExists, destinationModified) = Utils.FileExists(destination);

				LogDebugMessage($"App Icon Destination: " + destination);

				if (destinationModified > sourceModified)
				{
					Logger.Log($"Skipping `{img.Filename}` => `{destination}` file is up to date.");
					resizedImages.Add(new ResizedImageInfo() { Dpi = dpi, Filename = destination });
					continue;
				}

				appTool.Resize(dpi, destination);
				var r = appTool.Resize(dpi, destination);
				resizedImages.Add(r);
			}
		}

		void ProcessImageResize(ResizeImageInfo img, DpiPath[] dpis, ConcurrentBag<ResizedImageInfo> resizedImages)
		{
			var resizer = new Resizer(img, IntermediateOutputPath, this);

			foreach (var dpi in dpis)
			{
				LogDebugMessage($"Resizing {img.Filename}");

				var r = resizer.Resize(dpi, InputsFile);
				resizedImages.Add(r);

				LogDebugMessage($"Resized {img.Filename}");
			}
		}

		void ProcessImageCopy(ResizeImageInfo img, DpiPath originalScaleDpi, ConcurrentBag<ResizedImageInfo> resizedImages)
		{
			var resizer = new Resizer(img, IntermediateOutputPath, this);

			LogDebugMessage($"Copying {img.Filename}");

			var r = resizer.CopyFile(originalScaleDpi, InputsFile);
			resizedImages.Add(r);

			LogDebugMessage($"Copied {img.Filename}");
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}
	}
}
