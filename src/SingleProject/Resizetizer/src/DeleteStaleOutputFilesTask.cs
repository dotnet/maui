using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Removes files left over from earlier Resizetizer runs without separating filesystem validation
	/// from the operation that consumes it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The root is opened before enumeration and retained until cleanup finishes. Each stale leaf is then
	/// opened and validated before it is removed through retained filesystem identities. Windows deletes
	/// the opened leaf while preventing ancestry changes. Unix atomically moves the opened leaf to a
	/// quarantine directory outside the generated resource tree; <c>Clean</c> removes that quarantine.
	/// This prevents replacing the root, a parent directory, or the leaf name after validation from
	/// redirecting cleanup.
	/// </para>
	/// <para>
	/// Known outputs are still canonicalized permissively because they describe files a future build step
	/// may not have created yet. Existing deletion candidates never use that permissive path.
	/// </para>
	/// </remarks>
	public class DeleteStaleOutputFilesTask : Task
	{
		/// <summary>The directory whose stale files may be removed.</summary>
		[Required]
		public string Root { get; set; }

		/// <summary>The files the current build expects beneath <see cref="Root"/>.</summary>
		public ITaskItem[] KnownOutputs { get; set; }

		public override bool Execute() => Execute(null, null);

		internal bool Execute(Action beforeDeletion) =>
			Execute(beforeDeletion, null);

		internal bool Execute(Action beforeDeletion, Action afterIdentityValidation)
		{
			var lexicalRoot = NormalizePath(Root);
			if (lexicalRoot is null)
			{
				Log.LogMessage(MessageImportance.Low, $"Skipping stale file cleanup because the root '{Root}' is not a valid path.");
				return true;
			}

			using var session = StaleOutputDeletionSession.TryOpen(Root, out var openError);
			if (session is null)
			{
				Log.LogMessage(MessageImportance.Low, $"Skipping stale file cleanup because the root '{Root}' could not be opened safely: {openError}");
				return true;
			}

			var keep = GetKnownRelativePaths(lexicalRoot, session.RootPath);
			var existing = EnumerateExistingFiles(lexicalRoot);
			var invokedBeforeDeletion = false;
			var invokedAfterIdentityValidation = false;

			foreach (var path in existing)
			{
				if (!TryGetRelativePath(path, lexicalRoot, StringComparison.Ordinal, out var relative))
					continue;

				if (keep.Contains(relative))
					continue;

				using var candidate = session.TryValidate(relative, out var validationError);
				if (candidate is null)
				{
					Log.LogMessage(MessageImportance.Low, $"Leaving '{path}' alone because it could not be bound to the cleanup root: {validationError}");
					continue;
				}

				if (!invokedBeforeDeletion && beforeDeletion is not null)
				{
					try
					{
						beforeDeletion();
					}
					catch (Exception ex)
					{
						Log.LogErrorFromException(ex, showStackTrace: true);
						return false;
					}

					invokedBeforeDeletion = true;
				}

				void AfterIdentityValidation()
				{
					if (!invokedAfterIdentityValidation && afterIdentityValidation is not null)
					{
						afterIdentityValidation();
						invokedAfterIdentityValidation = true;
					}
				}

				switch (candidate.Delete(AfterIdentityValidation, out var deleteError))
				{
					case StaleFileDeletionResult.Deleted:
						Log.LogMessage(MessageImportance.Low, $"Deleted stale output file '{path}'.");
						break;

					case StaleFileDeletionResult.Quarantined:
						Log.LogMessage(MessageImportance.Low, $"Moved stale output file '{path}' out of the Resizetizer output tree.");
						break;

					case StaleFileDeletionResult.Changed:
						Log.LogMessage(MessageImportance.Low, $"Leaving '{path}' alone because its filesystem identity changed before deletion.");
						break;

					case StaleFileDeletionResult.Unsupported:
						Log.LogWarning($"Leaving stale output file '{path}' in place because this filesystem cannot perform the required atomic deletion: {deleteError}");
						break;

					default:
						Log.LogError($"Could not safely delete stale output file '{path}': {deleteError}");
						break;
				}
			}

			return !Log.HasLoggedErrors;
		}

		HashSet<string> GetKnownRelativePaths(string lexicalRoot, string physicalRoot)
		{
			var keep = new HashSet<string>(PathCanonicalizer.Comparer);
			var canonicalizer = new PathCanonicalizer();

			foreach (var item in KnownOutputs ?? Enumerable.Empty<ITaskItem>())
			{
				var path = NormalizePath(item?.ItemSpec);
				if (path is not null &&
					TryGetRelativePath(path, lexicalRoot, PathComparison, out var lexicalRelative))
				{
					keep.Add(lexicalRelative);
					continue;
				}

				var key = canonicalizer.GetComparisonKey(item?.ItemSpec);
				if (key is not null &&
					TryGetRelativePath(key, physicalRoot, PathComparison, out var physicalRelative))
				{
					keep.Add(physicalRelative);
				}
			}

			return keep;
		}

		List<string> EnumerateExistingFiles(string root)
		{
			var files = new List<string>();
			var pending = new Stack<string>();
			pending.Push(root);

			while (pending.Count != 0)
			{
				var directory = pending.Pop();
				string[] entries;

				try
				{
					entries = Directory.GetFileSystemEntries(directory);
				}
				catch (Exception ex)
				{
					Log.LogMessage(MessageImportance.Low, $"Leaving '{directory}' alone because it could not be enumerated: {ex.Message}");
					continue;
				}

				foreach (var entry in entries)
				{
					FileAttributes attributes;
					try
					{
						attributes = File.GetAttributes(entry);
					}
					catch (Exception ex)
					{
						Log.LogMessage(MessageImportance.Low, $"Leaving '{entry}' alone because it could not be inspected: {ex.Message}");
						continue;
					}

					if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
					{
						if ((attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
							pending.Push(entry);

						continue;
					}

					files.Add(entry);
				}
			}

			files.Sort(PathCanonicalizer.Comparer);
			return files;
		}

		static StringComparison PathComparison =>
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

		static string NormalizePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			try
			{
				return TrimTrailingSeparators(Path.GetFullPath(path));
			}
			catch (Exception)
			{
				return null;
			}
		}

		static bool TryGetRelativePath(string path, string root, StringComparison comparison, out string relative)
		{
			relative = null;

			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root) ||
				path.Length <= root.Length ||
				!path.StartsWith(root, comparison))
			{
				return false;
			}

			var rootEndsInSeparator = IsDirectorySeparator(root[root.Length - 1]);
			if (!rootEndsInSeparator && !IsDirectorySeparator(path[root.Length]))
				return false;

			var start = root.Length;
			while (start < path.Length && IsDirectorySeparator(path[start]))
				start++;

			if (start == path.Length)
				return false;

			var candidate = path.Substring(start).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			foreach (var segment in candidate.Split(Path.DirectorySeparatorChar))
			{
				if (string.IsNullOrEmpty(segment) || segment == "." || segment == "..")
					return false;
			}

			relative = candidate;
			return true;
		}

		static bool IsDirectorySeparator(char value) =>
			value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar;

		static string TrimTrailingSeparators(string path)
		{
			var root = Path.GetPathRoot(path) ?? string.Empty;
			var end = path.Length;

			while (end > root.Length && IsDirectorySeparator(path[end - 1]))
				end--;

			return end == path.Length ? path : path.Substring(0, end);
		}
	}

	internal enum StaleFileDeletionResult
	{
		Deleted,
		Quarantined,
		Changed,
		Unsupported,
		Failed,
	}

	internal interface IStaleOutputDeletionSession : IDisposable
	{
		string RootPath { get; }

		IValidatedStaleFile TryValidate(string relativePath, out string error);
	}

	internal interface IValidatedStaleFile : IDisposable
	{
		StaleFileDeletionResult Delete(Action afterIdentityValidation, out string error);
	}

	internal static class StaleOutputDeletionSession
	{
		public static IStaleOutputDeletionSession TryOpen(string root, out string error)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return WindowsStaleOutputDeletionSession.TryOpen(root, out error);

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
				RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return UnixStaleOutputDeletionSession.TryOpen(root, out error);
			}

			error = $"The current operating system '{RuntimeInformation.OSDescription}' is not supported.";
			return null;
		}
	}
}
