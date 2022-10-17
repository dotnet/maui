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
		public string WarningMessage { get; set; }

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
					builder.Append(WarningMessage);

					// select duplicates
					var invalidItems = new List<string>();
					var idupe = 0;
					foreach (var pair in filenames)
					{
						if (pair.Value.Count > 1)
						{
							if (idupe > 0)
								builder.Append("; ");
							idupe++;

							builder.Append(pair.Key + " -> ");

							var ifname = 0;
							foreach (var item in pair.Value)
							{
								if (ifname > 0)
									builder.Append(", ");
								ifname++;

								builder.Append(item.Filename);

								invalidItems.Add(item.Filename);
							}
						}
					}

					if (invalidItems.Count > 0)
					{
						InvalidItems = invalidItems.ToArray();

						Log.LogError(builder.ToString());
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
