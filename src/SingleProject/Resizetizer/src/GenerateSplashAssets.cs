using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Generates the splash assets file for Windows
	/// </summary>
	[MSBuildMultiThreadableTask]
	public class GenerateSplashAssets : MauiTask, ILogger
	{
		[Required]
		public string IntermediateOutputPath { get; set; }

		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		public override bool Execute()
		{
			var splash = MauiSplashScreen[0];

			var img = ResizeImageInfo.Parse(splash, TaskEnvironment);

			var intermediateOutputPath = TaskEnvironment.GetAbsolutePath(IntermediateOutputPath);
			Directory.CreateDirectory(intermediateOutputPath);

			var appTool = new SkiaSharpAppIconTools(img, this);

			Log.LogMessage(MessageImportance.Low, $"Splash Screen: Intermediate Path " + IntermediateOutputPath);

			foreach (var dpi in DpiPath.Windows.SplashScreen)
			{
				Log.LogMessage(MessageImportance.Low, $"Splash Screen: " + dpi);

				var destination = Resizer.GetRasterFileDestination(img, dpi, intermediateOutputPath);

				Log.LogMessage(MessageImportance.Low, $"Splash Screen Destination: " + destination);

				appTool.Resize(dpi, destination);
			}

			return !Log.HasLoggedErrors;
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}
	}
}
