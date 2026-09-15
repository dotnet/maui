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
	/// <para>
	/// The two item lists reach this task through different code paths: <see cref="Files"/> comes from an
	/// MSBuild wildcard rooted at the project directory, while <see cref="KnownOutputs"/> comes back from
	/// a task that resolved the same directory itself. A plain <c>Remove</c> compares those item specs
	/// textually, so a symbolic link or junction anywhere in the project path is enough to make every
	/// generated file look stale. Comparing canonical paths makes the difference independent of spelling
	/// while the returned items keep their original item spec and metadata.
	/// </para>
	/// <para>
	/// A recursive MSBuild wildcard descends through directory links, so it can name a file that lives
	/// outside <see cref="Root"/>, and <c>&lt;Delete&gt;</c> would then remove that outside file rather
	/// than the link. Anything whose resolved directory escapes <see cref="Root"/> is therefore never
	/// reported as stale.
	/// </para>
	/// </remarks>
	public class DetectStaleOutputFilesTask : Task
	{
		/// <summary>The files currently present in the output directory.</summary>
		public ITaskItem[] Files { get; set; }

		/// <summary>The files the build expects to be there.</summary>
		public ITaskItem[] KnownOutputs { get; set; }

		/// <summary>The only directory whose contents this task is allowed to report as stale.</summary>
		[Required]
		public string Root { get; set; }

		/// <summary>The members of <see cref="Files"/> that are safe to delete.</summary>
		[Output]
		public ITaskItem[] StaleFiles { get; set; }

		/// <summary>
		/// Lets tests exercise the behaviour of hosts without <c>Directory.ResolveLinkTarget</c>, which is
		/// every .NET Framework host, including MSBuild.exe.
		/// </summary>
		internal bool AllowLinkResolution { get; set; } = true;

		public override bool Execute()
		{
			StaleFiles = Array.Empty<ITaskItem>();

			if (Files is null || Files.Length == 0)
				return true;

			var canonicalizer = new PathCanonicalizer(AllowLinkResolution);

			var root = canonicalizer.CanonicalizeDirectory(Root);
			if (string.IsNullOrEmpty(root))
			{
				Log.LogMessage(MessageImportance.Low, $"Skipping stale file detection because the root '{Root}' could not be resolved.");
				return true;
			}

			var keep = new HashSet<string>(PathCanonicalizer.Comparer);
			foreach (var known in KnownOutputs ?? Enumerable.Empty<ITaskItem>())
			{
				var key = canonicalizer.GetComparisonKey(known?.ItemSpec);
				if (key is not null)
					keep.Add(key);
			}

			var stale = new List<ITaskItem>();

			foreach (var file in Files)
			{
				if (file is null || string.IsNullOrWhiteSpace(file.ItemSpec))
					continue;

				var key = canonicalizer.GetComparisonKey(file.ItemSpec);
				if (key is null)
				{
					Log.LogMessage(MessageImportance.Low, $"Leaving '{file.ItemSpec}' alone because its real location could not be determined.");
					continue;
				}

				if (!PathCanonicalizer.IsUnder(key, root))
				{
					Log.LogMessage(MessageImportance.Low, $"Leaving '{file.ItemSpec}' alone because it resolves to '{key}', which is outside '{root}'.");
					continue;
				}

				if (keep.Contains(key))
					continue;

				Log.LogMessage(MessageImportance.Low, $"Detected stale output file '{file.ItemSpec}'.");
				stale.Add(file);
			}

			StaleFiles = stale.ToArray();

			return true;
		}
	}
}
