using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

		public string InputsFile { get; set; }

		public ITaskItem[] Images { get; set; }

		[Output]
		public ITaskItem[] CopiedResources { get; set; }

		public string IsMacEnabled { get; set; }

		public ILogger Logger => this;

		public override System.Threading.Tasks.Task ExecuteAsync()
		{
			Svg.SvgDocument.SkipGdiPlusCapabilityCheck = true;

			var images = ParseImageTaskItems(Images);

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
					LogWarning("MAUI0000", ex.ToString());

					throw;
				}
			});

			var copiedResources = new List<TaskItem>();

			foreach (var img in resizedImages)
			{
				var attr = new Dictionary<string, string>();
				string itemSpec = Path.GetFullPath(img.Filename);

				// Fix the item spec to be relative for mac
				if (bool.TryParse(IsMacEnabled, out bool isMac) && isMac)
					itemSpec = img.Filename;

				// Add DPI info to the itemspec so we can use it in the targets
				attr.Add("_ResizetizerDpiPath", img.Dpi.Path);
				attr.Add("_ResizetizerDpiScale", img.Dpi.Scale.ToString());

				copiedResources.Add(new TaskItem(itemSpec, attr));
			}

			CopiedResources = copiedResources.ToArray();

			return System.Threading.Tasks.Task.CompletedTask;
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

			LogDebugMessage($"Generating App Icon Bitmaps for DPIs");

			var appTool = new SkiaSharpAppIconTools(img, this);

			LogDebugMessage($"App Icon: Intermediate Path " + IntermediateOutputPath);

			foreach (var dpi in appIconDpis)
			{
				LogDebugMessage($"App Icon: " + dpi);

				var destination = Resizer.GetFileDestination(img, dpi, IntermediateOutputPath)
					.Replace("{name}", appIconName);

				LogDebugMessage($"App Icon Destination: " + destination);

				appTool.Resize(dpi, Path.ChangeExtension(destination, ".png"));
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

			var r = resizer.CopyFile(originalScaleDpi, InputsFile, PlatformType.ToLower().Equals("android"));
			resizedImages.Add(r);

			LogDebugMessage($"Copied {img.Filename}");
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}

		List<ResizeImageInfo> ParseImageTaskItems(ITaskItem[] images)
		{
			var r = new List<ResizeImageInfo>();

			if (images == null)
				return r;

			foreach (var image in images)
			{
				var info = new ResizeImageInfo();

				var fileInfo = new FileInfo(image.GetMetadata("FullPath"));
				if (!fileInfo.Exists)
					throw new FileNotFoundException("Unable to find background file: " + fileInfo.FullName, fileInfo.FullName);

				info.Filename = fileInfo.FullName;

				info.Alias = image.GetMetadata("Link");

				info.BaseSize = Utils.ParseSizeString(image.GetMetadata("BaseSize"));

				if (bool.TryParse(image.GetMetadata("Resize"), out var rz))
					info.Resize = rz;

				info.TintColor = Utils.ParseColorString(image.GetMetadata("TintColor"));

				if (bool.TryParse(image.GetMetadata("IsAppIcon"), out var iai))
					info.IsAppIcon = iai;

				if (float.TryParse(image.GetMetadata("ForegroundScale"), out var fsc))
					info.ForegroundScale = fsc;

				var fgFile = image.GetMetadata("ForegroundFile");
				if (!string.IsNullOrEmpty(fgFile))
				{
					var fgFileInfo = new FileInfo(fgFile);
					if (!fgFileInfo.Exists)
						throw new FileNotFoundException("Unable to find foreground file: " + fgFileInfo.FullName, fgFileInfo.FullName);

					info.ForegroundFilename = fgFileInfo.FullName;
				}

				// TODO:
				// - Parse out custom DPI's

				r.Add(info);
			}

			return r;
		}
	}
}
