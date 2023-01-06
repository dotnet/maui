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
	/// Generates the MauiSplash.storyboard file for iOS splash screens
	/// </summary>
	public class GenerateSplashStoryboard : Task, ILogger
	{
		[Required]
		public string IntermediateOutputPath { get; set; }

		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		public string InputsFile { get; set; }

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, $"Splash Screen: Intermediate Path " + IntermediateOutputPath);

			var splash = MauiSplashScreen?.FirstOrDefault();
			if (splash is null)
			{
				Log.LogMessage(MessageImportance.Low, $"Splash Screen: No images found.");
				return true;
			}

			try
			{
				var info = ResizeImageInfo.Parse(splash);

				var resizer = new Resizer(info, IntermediateOutputPath, this);

				WriteImages(resizer);
				WriteStoryboard(resizer);

				return !Log.HasLoggedErrors;
			}
			catch (Exception ex)
			{
				Log.LogError(null, "MAUI0000", null, null, 0, 0, 0, 0, ex.ToString());
				return false;
			}
		}

		private void WriteImages(Resizer resizer)
		{
			if (resizer.Info.Resize)
			{
				foreach (var dpi in DpiPath.Ios.Image)
				{
					Log.LogMessage(MessageImportance.Low, $"Splash Screen Resize: " + dpi);
					resizer.Resize(dpi, InputsFile);
				}
			}
			else
			{
				var dpi = DpiPath.Ios.Original;

				Log.LogMessage(MessageImportance.Low, $"Splash Screen Copy: " + dpi);
				resizer.CopyFile(dpi, InputsFile);
			}
		}

		private void WriteStoryboard(Resizer resizer)
		{
			Directory.CreateDirectory(IntermediateOutputPath);
			var storyboardFile = Path.Combine(IntermediateOutputPath, "MauiSplash.storyboard");

			Log.LogMessage(MessageImportance.Low, $"Splash Screen Storyboard: " + storyboardFile);

			DpiPath dpi;
			if (resizer.Info.Resize)
				dpi = DpiPath.Ios.Image[0];
			else
				dpi = DpiPath.Ios.Original;

			var image = resizer.GetRasterFileDestination(dpi, includeIntermediate: false, includeScale: false);

			var color = resizer.Info.Color ?? SKColors.White;
			float r = color.Red / (float)byte.MaxValue;
			float g = color.Green / (float)byte.MaxValue;
			float b = color.Blue / (float)byte.MaxValue;
			float a = color.Alpha / (float)byte.MaxValue;

			var rStr = r.ToString(CultureInfo.InvariantCulture);
			var gStr = g.ToString(CultureInfo.InvariantCulture);
			var bStr = b.ToString(CultureInfo.InvariantCulture);
			var aStr = a.ToString(CultureInfo.InvariantCulture);

			using var writer = File.CreateText(storyboardFile);
			SubstituteStoryboard(writer, image, rStr, gStr, bStr, aStr);
		}

		internal static void SubstituteStoryboard(TextWriter writer, string image, string r, string g, string b, string a)
		{
			using var resourceStream = typeof(GenerateSplashStoryboard).Assembly.GetManifestResourceStream("MauiSplash.storyboard");
			using var reader = new StreamReader(resourceStream);

			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine()
					.Replace("{imageView.image}", image)
					.Replace("{color.red}", r)
					.Replace("{color.green}", g)
					.Replace("{color.blue}", b)
					.Replace("{color.alpha}", a);

				writer.WriteLine(line);
			}
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
		}
	}
}
