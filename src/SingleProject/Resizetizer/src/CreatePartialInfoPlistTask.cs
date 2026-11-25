using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	[MSBuildMultiThreadableTask]
	public class CreatePartialInfoPlistTask : MauiTask
	{
		public ITaskItem[] CustomFonts { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public string PlistName { get; set; }

		public string Storyboard { get; set; }

		public string LaunchScreenImage { get; set; }

		public string LaunchScreenColor { get; set; }

		const string plistHeader =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>";
		const string plistFooter = @"
</dict>
</plist>";

		public override bool Execute()
		{
			try
			{
				Directory.CreateDirectory(TaskEnvironment.GetAbsolutePath(IntermediateOutputPath));

				var plistFilename = Path.Combine(IntermediateOutputPath, PlistName ?? "PartialInfo.plist");

				using (var f = File.CreateText(TaskEnvironment.GetAbsolutePath(plistFilename)))
				{
					f.WriteLine(plistHeader);

					if (CustomFonts != null && CustomFonts.Length > 0)
					{
						f.WriteLine("  <key>UIAppFonts</key>");
						f.WriteLine("  <array>");

						foreach (var font in CustomFonts)
						{
							f.WriteLine("	<string>" + Path.GetFileName(font.ItemSpec) + "</string>");
						}

						f.WriteLine("  </array>");
					}

					if (!string.IsNullOrEmpty(LaunchScreenImage) || !string.IsNullOrEmpty(LaunchScreenColor))
					{
						f.WriteLine("  <key>UILaunchScreen</key>");
						f.WriteLine("  <dict>");

						if (!string.IsNullOrEmpty(LaunchScreenImage))
						{
							f.WriteLine("    <key>UIImageName</key>");
							f.WriteLine($"    <string>{LaunchScreenImage}</string>");
						}

						if (!string.IsNullOrEmpty(LaunchScreenColor))
						{
							f.WriteLine("    <key>UIColorName</key>");
							f.WriteLine($"    <string>{LaunchScreenColor}</string>");
						}

						f.WriteLine("  </dict>");
					}
					else if (!string.IsNullOrEmpty(Storyboard))
					{
						f.WriteLine("  <key>UILaunchStoryboardName</key>");
						f.WriteLine($"  <string>{Path.GetFileNameWithoutExtension(Storyboard)}</string>");
					}

					f.WriteLine(plistFooter);
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}
	}
}