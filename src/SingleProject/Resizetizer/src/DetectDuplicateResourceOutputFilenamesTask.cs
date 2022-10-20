using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class DetectDuplicateResourceOutputFilenamesTask : Task
	{
		public ITaskItem[] Items { get; set; }

		[Required]
		public string Message { get; set; }

		[Output]
		public string[] InvalidItems { get; set; }

		public override bool Execute()
		{
			try
			{
				if (Items != null)
				{
					var infos = ResizeImageInfo.Parse(Items);

					// group the items
					var filenames = new Dictionary<string, List<ResizeImageInfo>>();
					foreach (var info in infos)
					{
						var filename = info.OutputName;
						if (!filenames.TryGetValue(filename, out var list))
						{
							list = new List<ResizeImageInfo>();
							filenames[filename] = list;
						}
						list.Add(info);
					}

					// build up the warning
					var builder = new StringBuilder();
					builder.Append(Message);

					// select duplicates
					var duplicateItems = new List<string>();
					var idupe = 0;
					foreach (var pair in filenames)
					{
						if (pair.Value.Count > 1)
						{
							if (idupe > 0)
								builder.Append(", ");
							idupe++;

							builder.Append(pair.Key);

							foreach (var item in pair.Value)
							{
								duplicateItems.Add(item.Filename);
							}
						}
					}

					if (duplicateItems.Count > 0)
					{
						InvalidItems = duplicateItems.ToArray();

						Log.LogWarning(builder.ToString());
					}
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
