using System;
using System.IO;
using System.Linq;
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

		public bool MutateAfterIdentityValidation { get; set; }

		public bool ExpectMutationBlocked { get; set; }

		public string QuarantineReplacementPath { get; set; }

		public string QuarantinedOriginalPath { get; set; }

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

				Action mutation =
					string.IsNullOrWhiteSpace(MutationPath) &&
					string.IsNullOrWhiteSpace(QuarantineReplacementPath)
						? null
						: Mutate;

				return MutateAfterIdentityValidation
					? task.Execute(null, mutation)
					: task.Execute(mutation, null);
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, showStackTrace: true);
				return false;
			}
		}

		void Mutate()
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(QuarantineReplacementPath))
				{
					var root = Root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					var quarantine = Path.Combine(Path.GetDirectoryName(root), ".maui-resizetizer-stale");
					var quarantined = AssertSingleQuarantinedFile(quarantine);
					File.Move(quarantined, QuarantinedOriginalPath);
					File.Move(QuarantineReplacementPath, quarantined);
				}
				else if (!string.IsNullOrEmpty(RecreatedFileContents))
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
			}
			catch (IOException) when (ExpectMutationBlocked)
			{
			}

			if (!string.IsNullOrWhiteSpace(MarkerFile))
			{
				var markerDirectory = Path.GetDirectoryName(MarkerFile);
				if (!string.IsNullOrEmpty(markerDirectory))
					Directory.CreateDirectory(markerDirectory);

				File.WriteAllText(MarkerFile, MutationPath);
			}
		}

		static string AssertSingleQuarantinedFile(string quarantine)
		{
			var files = Directory.GetFiles(quarantine, ".maui-resizetizer-delete-*");
			if (files.Length != 1)
				throw new InvalidOperationException($"Expected one quarantined file in '{quarantine}', found {files.Length}: {string.Join(", ", files.Select(Path.GetFileName))}");

			return files[0];
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
