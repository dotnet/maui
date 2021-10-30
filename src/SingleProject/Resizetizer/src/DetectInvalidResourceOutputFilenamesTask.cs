using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class DetectInvalidResourceOutputFilenamesTask : Task
	{
		public ITaskItem[] Items { get; set; }

		public bool ThrowsError { get; set; } = true;

		[Required]
		public string ErrorMessage { get; set; }

		[Output]
		public string[] InvalidItems { get; set; }

		public override bool Execute()
		{
			var invalidFilenames = new List<string>();
			try
			{
				if (Items != null)
				{
					foreach (var item in Items)
					{
						if (!Utils.IsValidResourceFilename(item.ItemSpec))
							invalidFilenames.Add(item.ItemSpec);
					}
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}
			finally
			{
				if (invalidFilenames.Count > 0)
				{
					InvalidItems = invalidFilenames.ToArray();

					if (ThrowsError)
					{
						var builder = new StringBuilder();
						builder.AppendLine(ErrorMessage);
						foreach (var file in invalidFilenames)
						{
							builder.AppendLine();
							builder.Append('\t');
							builder.Append(Path.GetFileNameWithoutExtension(file));
						}

						Log.LogError(builder.ToString());
					}
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}