using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	public class DetectInvalidResourceOutputFilenamesTask : AsyncTask
	{
		public ITaskItem[] Items { get; set; }

		public bool ThrowsError { get; set; } = true;

		[Required]
		public string ErrorMessage { get; set; }

		[Output]
		public ITaskItem[] InvalidItems { get; set; }

		public override bool Execute()
		{
			System.Threading.Tasks.Task.Run(() =>
			{
				var invalidFilenames = new ConcurrentBag<string>();

				try
				{
					if (Items != null)
					{
						System.Threading.Tasks.Parallel.ForEach(Items, item =>
						{
							var filename = item.ItemSpec;

							if (!Utils.IsValidResourceFilename(filename))
								invalidFilenames.Add(filename);
						});
					}
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
				finally
				{
					InvalidItems = invalidFilenames.Select(f => new TaskItem(f)).ToArray();

					if (ThrowsError && invalidFilenames.Any())
					{
						Log.LogError($"{ErrorMessage}{Environment.NewLine}\t"
							+ string.Join(Environment.NewLine + "\t", invalidFilenames.Select(f => Path.GetFileNameWithoutExtension(f))));
					}
					Complete();
				}
			});

			return base.Execute();
		}
	}
}