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
		readonly List<ITaskItem> _generatedCodeFiles = new List<ITaskItem>();

		[Required]
		public ITaskItem[] XamlFiles { get; set; }

		[Output]
		public ITaskItem[] GeneratedCodeFiles => _generatedCodeFiles.ToArray();

		public string Language { get; set; }
		public string AssemblyName { get; set; }
		public string OutputPath { get; set; }

		public override bool Execute()
		{
			bool success = true;
			Log.LogMessage(MessageImportance.Normal, "Generating assembly attributes for CSS files");
			if (XamlFiles == null) {
				Log.LogMessage(MessageImportance.Low, "Skipping CssG");
				return true;
			}

			foreach (var xamlFile in XamlFiles) {
				var outputFile = Path.Combine(OutputPath, $"{xamlFile.GetMetadata("TargetPath")}.g.cs");
				var generator = new CssGenerator(xamlFile, Language, AssemblyName, outputFile, Log);
				try {
					if (generator.Execute())
						_generatedCodeFiles.Add(new TaskItem(Microsoft.Build.Evaluation.ProjectCollection.Escape(outputFile)));
				}
				catch (XmlException xe) {
					Log.LogError(null, null, null, xamlFile.ItemSpec, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message, xe.HelpLink, xe.Source);

					success = false;
				}
				catch (Exception e) {
					Log.LogError(null, null, null, xamlFile.ItemSpec, 0, 0, 0, 0, e.Message, e.HelpLink, e.Source);
					success = false;
				}
			}

			return success;
		}
	}
}