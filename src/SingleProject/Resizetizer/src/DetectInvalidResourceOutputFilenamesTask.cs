using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public ITaskItem[] InvalidItems { get; set; }

		public override bool Execute()
		{
			var tempInvalidItems = new Dictionary<ITaskItem, string>();
			try
			{
				if (Items != null)
				{
					foreach (var item in Items)
					{
						var image = ResizeImageInfo.Parse(item);
						var output = image.OutputName;
						if (!Utils.IsValidResourceFilename(output))
							tempInvalidItems.Add(item, output);
					}
				}
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}
			finally
			{
				if (tempInvalidItems.Count > 0)
				{
					InvalidItems = tempInvalidItems.Keys.ToArray();

					var builder = new StringBuilder();
					builder.Append(ErrorMessage);

					var idx = 0;
					foreach (var pair in tempInvalidItems)
					{
						if (idx > 0)
							builder.Append(", ");

						builder.Append($"{pair.Value} ({pair.Key.ItemSpec})");

						idx++;
					}

					if (ThrowsError)
						Log.LogError(builder.ToString());
					else
						Log.LogMessage(builder.ToString());
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
