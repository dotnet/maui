using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Generates the splash assets file for Windows
	/// </summary>
	public class GenerateSplashAssets : Task, ILogger
	{
		[Required]
		public string IntermediateOutputPath { get; set; }

		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		public override bool Execute()
		{
			var splash = MauiSplashScreen[0];

			var colorMetadata = splash.GetMetadata("Color");
			var color = Utils.ParseColorString(colorMetadata);
			if (color == null && !string.IsNullOrEmpty(colorMetadata))
				Log.LogWarning($"Unable to parse color value '{colorMetadata}' for '{splash.ItemSpec}'.");

			var fileInfo = new FileInfo(splash.GetMetadata("FullPath"));
			if (!fileInfo.Exists)
				throw new FileNotFoundException("Unable to find background file: " + fileInfo.FullName, fileInfo.FullName);

			var img = new ResizeImageInfo
			{
				Filename = fileInfo.FullName,
				Color = color ?? SKColors.Transparent,
			};

			Directory.CreateDirectory(IntermediateOutputPath);

			var appTool = new SkiaSharpAppIconTools(img, this);

			Log.LogMessage(MessageImportance.Low, $"Splash Screen: Intermediate Path " + IntermediateOutputPath);

			foreach (var dpi in DpiPath.UwpSplashScreen)
			{
				Log.LogMessage(MessageImportance.Low, $"Splash Screen: " + dpi);

				var destination = Resizer.GetFileDestination(img, dpi, IntermediateOutputPath);

				Log.LogMessage(MessageImportance.Low, $"Splash Screen Destination: " + destination);

				appTool.Resize(dpi, Path.ChangeExtension(destination, ".png"));
			}

			return !Log.HasLoggedErrors;
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}
	}
}
