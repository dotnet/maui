using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.Forms.Build.Tasks
{
	public class CssGTask : Task
	{
		[Required]
		public ITaskItem[] CSSFiles { get; set; }

		[Required]
		public ITaskItem[] OutputFiles { get; set; }

		public string Language { get; set; }
		public string AssemblyName { get; set; }

		public override bool Execute()
		{
			bool success = true;
			Log.LogMessage(MessageImportance.Normal, "Generating assembly attributes for CSS files");
			if (CSSFiles == null || OutputFiles == null)
			{
				Log.LogMessage(MessageImportance.Low, "Skipping CssG");
				return true;
			}

			if (CSSFiles.Length != OutputFiles.Length)
			{
				Log.LogError("\"{2}\" refers to {0} item(s), and \"{3}\" refers to {1} item(s). They must have the same number of items.", CSSFiles.Length, OutputFiles.Length, "CSSFiles", "OutputFiles");
				return false;
			}

			for (var i = 0; i < CSSFiles.Length; i++)
			{
				var cssFile = CSSFiles[i];
				var outputFile = OutputFiles[i].ItemSpec;

				var generator = new CssGenerator(cssFile, Language, AssemblyName, outputFile, Log);
				try
				{
					if (!generator.Execute())
					{
						//If Execute() fails, the file still needs to exist because it is added to the <Compile/> ItemGroup
						File.WriteAllText(outputFile, string.Empty);
					}
				}
				catch (XmlException xe)
				{
					Log.LogError(null, null, null, cssFile.ItemSpec, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message, xe.HelpLink, xe.Source);

					success = false;
				}
				catch (Exception e)
				{
					Log.LogError(null, null, null, cssFile.ItemSpec, 0, 0, 0, 0, e.Message, e.HelpLink, e.Source);
					success = false;
				}
			}

			return success;
		}
	}
}