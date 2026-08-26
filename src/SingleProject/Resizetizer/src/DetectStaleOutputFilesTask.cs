using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Returns the subset of <see cref="Files"/> that is not part of <see cref="KnownOutputs"/>, so the
	/// build can delete files left over from a previous build without ever deleting a file it just wrote.
	/// </summary>
	/// <remarks>
	/// The two item lists reach this task through different code paths: <see cref="Files"/> comes from an
	/// MSBuild wildcard rooted at the project directory, while <see cref="KnownOutputs"/> comes back from
	/// a task that resolved the same directory itself. A plain <c>Remove</c> compares those item specs
	/// textually, so a symbolic link or junction anywhere in the project path is enough to make every
	/// generated file look stale. Comparing canonical paths makes the difference independent of spelling
	/// while the returned items keep their original item spec and metadata.
	/// </remarks>
	public class DetectStaleOutputFilesTask : Task
	{
		/// <summary>The files currently present in the output directory.</summary>
		public ITaskItem[] Files { get; set; }

		/// <summary>The files the build expects to be there.</summary>
		public ITaskItem[] KnownOutputs { get; set; }

		/// <summary>The members of <see cref="Files"/> that are safe to delete.</summary>
		[Output]
		public ITaskItem[] StaleFiles { get; set; }

		public override bool Execute()
		{
			StaleFiles = Array.Empty<ITaskItem>();

			if (Files is null || Files.Length == 0)
				return true;

			var canonicalizer = new PathCanonicalizer();
			var keep = canonicalizer.CreateSet(KnownOutputs?.Select(i => i.ItemSpec) ?? Enumerable.Empty<string>());

			var stale = new List<ITaskItem>();

			foreach (var file in Files)
			{
				if (file is null || string.IsNullOrWhiteSpace(file.ItemSpec))
					continue;

				if (keep.Contains(canonicalizer.Canonicalize(file.ItemSpec)))
					continue;

				stale.Add(file);
			}

			foreach (var file in stale)
				Log.LogMessage(MessageImportance.Low, $"Detected stale output file '{file.ItemSpec}'.");

			StaleFiles = stale.ToArray();

			return true;
		}
	}
}
