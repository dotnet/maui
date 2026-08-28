using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public sealed class InterposedDeleteStaleOutputFilesTask : Task
	{
		[Required]
		public string Root { get; set; }

		public ITaskItem[] KnownOutputs { get; set; }

		public string MutationPath { get; set; }

		public string MovedPath { get; set; }

		public string AliasTargetPath { get; set; }

		public string RecreatedFileContents { get; set; }

		public bool UseJunction { get; set; }

		public string MarkerFile { get; set; }

		public override bool Execute()
		{
			try
			{
				var task = new DeleteStaleOutputFilesTask
				{
					Root = Root,
					KnownOutputs = KnownOutputs,
					BuildEngine = BuildEngine,
					HostObject = HostObject,
				};

				return task.Execute(string.IsNullOrWhiteSpace(MutationPath) ? null : Mutate);
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, showStackTrace: true);
				return false;
			}
		}

		void Mutate()
		{
			if (!string.IsNullOrEmpty(RecreatedFileContents))
			{
				File.Move(MutationPath, MovedPath);
				File.WriteAllText(MutationPath, RecreatedFileContents);
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(MovedPath))
				{
					Directory.Move(MutationPath, MovedPath);
				}
				else
				{
					try
					{
						Directory.Delete(MutationPath);
					}
					catch (DirectoryNotFoundException)
					{
					}
				}

				if (!string.IsNullOrWhiteSpace(AliasTargetPath) &&
					!TryCreateDirectoryAlias(MutationPath, AliasTargetPath, out var error))
				{
					throw new IOException($"Could not create directory alias '{MutationPath}' to '{AliasTargetPath}': {error}");
				}
			}

			if (!string.IsNullOrWhiteSpace(MarkerFile))
			{
				var markerDirectory = Path.GetDirectoryName(MarkerFile);
				if (!string.IsNullOrEmpty(markerDirectory))
					Directory.CreateDirectory(markerDirectory);

				File.WriteAllText(MarkerFile, MutationPath);
			}
		}

		bool TryCreateDirectoryAlias(string alias, string target, out string error)
		{
			if (UseJunction)
			{
				if (!OperatingSystem.IsWindows())
					throw new PlatformNotSupportedException("Windows junctions were requested on a non-Windows host.");

				return Junction.TryCreate(alias, target, out error);
			}

			return SymbolicLink.TryCreateDirectoryLink(alias, target, out error);
		}
	}
}
