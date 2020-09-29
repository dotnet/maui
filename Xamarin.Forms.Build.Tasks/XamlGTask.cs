using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.Build.Tasks
{
	public class XamlGTask : Task
	{
		[Required]
		public ITaskItem[] XamlFiles { get; set; }

		[Required]
		public ITaskItem[] OutputFiles { get; set; }

		public string Language { get; set; }
		public string AssemblyName { get; set; }
		public string References { get; set; }

		public override bool Execute()
		{
			bool success = true;
			Log.LogMessage(MessageImportance.Normal, "Generating code behind for XAML files");

			//NOTE: should not happen due to [Required], but there appears to be a place this is class is called directly
			if (XamlFiles == null || OutputFiles == null)
			{
				Log.LogMessage("Skipping XamlG");
				return true;
			}

			if (XamlFiles.Length != OutputFiles.Length)
			{
				Log.LogError("\"{2}\" refers to {0} item(s), and \"{3}\" refers to {1} item(s). They must have the same number of items.", XamlFiles.Length, OutputFiles.Length, "XamlFiles", "OutputFiles");
				return false;
			}

			for (int i = 0; i < XamlFiles.Length; i++)
			{
				var xamlFile = XamlFiles[i];
				var outputFile = OutputFiles[i].ItemSpec;
				if (IOPath.DirectorySeparatorChar == '/' && outputFile.Contains(@"\"))
					outputFile = outputFile.Replace('\\', '/');
				else if (IOPath.DirectorySeparatorChar == '\\' && outputFile.Contains(@"/"))
					outputFile = outputFile.Replace('/', '\\');

				var generator = new XamlGenerator(xamlFile, Language, AssemblyName, outputFile, References, Log);
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
					Log.LogError(null, null, null, xamlFile.ItemSpec, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message, xe.HelpLink, xe.Source);
					Log.LogMessage(MessageImportance.Low, xe.StackTrace);
					success = false;
				}
				catch (Exception e)
				{
					Log.LogError(null, null, null, xamlFile.ItemSpec, 0, 0, 0, 0, e.Message, e.HelpLink, e.Source);
					Log.LogMessage(MessageImportance.Low, e.StackTrace);
					success = false;
				}
			}

			return success;
		}
	}
}