using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Generates the MauiSplash.storyboard file for iOS splash screens
	/// </summary>
	public class GenerateSplashStoryboard : Task
	{
		[Required]
		public string OutputFile { get; set; }

		[Required]
		public ITaskItem[] MauiSplashScreen { get; set; }

		public override bool Execute()
		{
			var splash = MauiSplashScreen[0];
			var colorMetadata = splash.GetMetadata("Color");
			var color = Utils.ParseColorString(colorMetadata);
			if (color == null)
			{
				if (!string.IsNullOrEmpty(colorMetadata))
				{
					Log.LogWarning($"Unable to parse color value '{colorMetadata}' for '{splash.ItemSpec}'.");
				}
				color = SKColors.White;
			}

			Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
			using var resourceStream = GetType().Assembly.GetManifestResourceStream("MauiSplash.storyboard");
			using var reader = new StreamReader(resourceStream);
			using var writer = File.CreateText(OutputFile);

			string image = Path.GetFileNameWithoutExtension(splash.ItemSpec) + ".png";
			float r = color.Value.Red / (float)byte.MaxValue;
			float g = color.Value.Green / (float)byte.MaxValue;
			float b = color.Value.Blue / (float)byte.MaxValue;
			float a = color.Value.Alpha / (float)byte.MaxValue;

			while (!reader.EndOfStream)
			{
				var line = string.Format(CultureInfo.InvariantCulture, reader.ReadLine(), image, r, g, b, a);
				writer.WriteLine(line);
			}

			return !Log.HasLoggedErrors;
		}
	}
}
