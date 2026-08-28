using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public sealed class DirectoryAliasMutationTask : Task
	{
		[Required]
		public string AliasPath { get; set; }

		public string TargetPath { get; set; }

		public string MarkerFile { get; set; }

		public bool UseJunction { get; set; }

		public override bool Execute()
		{
			try
			{
				try
				{
					Directory.Delete(AliasPath);
				}
				catch (DirectoryNotFoundException)
				{
				}

				if (!string.IsNullOrWhiteSpace(TargetPath))
				{
					string error;
					bool created;
					if (UseJunction)
					{
						if (!OperatingSystem.IsWindows())
						{
							Log.LogError("Windows junctions were requested on a non-Windows host.");
							return false;
						}

						created = Junction.TryCreate(AliasPath, TargetPath, out error);
					}
					else
					{
						created = SymbolicLink.TryCreateDirectoryLink(AliasPath, TargetPath, out error);
					}

					if (!created)
						Log.LogError($"Could not create directory alias '{AliasPath}' to '{TargetPath}': {error}");
				}

				if (!string.IsNullOrWhiteSpace(MarkerFile))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(MarkerFile));
					File.WriteAllText(MarkerFile, AliasPath);
				}

				return !Log.HasLoggedErrors;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, showStackTrace: true);
				return false;
			}
		}
	}
}
