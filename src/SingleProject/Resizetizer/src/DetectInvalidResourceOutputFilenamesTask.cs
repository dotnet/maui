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

		[Required]
		public string Message { get; set; }

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

					var builder = new StringBuilder();
					builder.Append(Message);

					for (var i = 0; i < invalidFilenames.Count; i++)
					{
						if (i > 0)
							builder.Append(", ");
						var file = invalidFilenames[i];
						builder.Append(Path.GetFileNameWithoutExtension(file));
					}

					Log.LogError(builder.ToString());
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
