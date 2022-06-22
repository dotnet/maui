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

			var info = ResizeImageInfo.Parse(splash);

			var outputFileName = info.OutputName;
			var image = outputFileName + ".png";

			var color = info.Color ?? SKColors.White;
			float r = color.Red / (float)byte.MaxValue;
			float g = color.Green / (float)byte.MaxValue;
			float b = color.Blue / (float)byte.MaxValue;
			float a = color.Alpha / (float)byte.MaxValue;

			var rStr = r.ToString(CultureInfo.InvariantCulture);
			var gStr = g.ToString(CultureInfo.InvariantCulture);
			var bStr = b.ToString(CultureInfo.InvariantCulture);
			var aStr = a.ToString(CultureInfo.InvariantCulture);

			var dir = Path.GetDirectoryName(OutputFile);
			Directory.CreateDirectory(dir);

			using (var writer = File.CreateText(OutputFile))
			{
				SubstituteStoryboard(writer, image, rStr, gStr, bStr, aStr);
			}

			return !Log.HasLoggedErrors;
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
	}
}
