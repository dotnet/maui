using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class CreatePartialInfoPlistTask : Task
	{
		public ITaskItem[] CustomFonts { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public string PlistName { get; set; }

		public string Storyboard { get; set; }

		[Output]
		public ITaskItem[] PlistFiles { get; set; }

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
				var plistFilename = Path.Combine(IntermediateOutputPath, PlistName ?? "PartialInfo.plist");

				using (var f = File.CreateText(plistFilename))
				{
					f.WriteLine(plistHeader);

					if (CustomFonts != null && CustomFonts.Length > 0)
					{
						f.WriteLine("  <key>UIAppFonts</key>");
						f.WriteLine("  <array>");

						foreach (var font in CustomFonts)
						{
							var fontFile = new FileInfo(font.ItemSpec);

							f.WriteLine("	<string>" + fontFile.Name + "</string>");
						}

						f.WriteLine("  </array>");
					}

					if (!string.IsNullOrEmpty(Storyboard))
					{
						f.WriteLine("  <key>UILaunchStoryboardName</key>");
						f.WriteLine($"  <string>{Path.GetFileNameWithoutExtension(Storyboard)}</string>");
					}

					f.WriteLine(plistFooter);
				}

				PlistFiles = new[] { new TaskItem(plistFilename) };
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}
	}
}