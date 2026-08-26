using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Produces a canonical spelling of a file system path so that two differently spelled paths
	/// which point at the same file compare as equal.
	/// </summary>
	/// <remarks>
	/// <para>
	/// MSBuild resolves relative item specs against <c>$(MSBuildProjectDirectory)</c>, which keeps the
	/// spelling the build was started with, while <see cref="Path.GetFullPath(string)"/> inside a task
	/// resolves them against the process working directory, which Unix reports with every symbolic link
	/// already resolved. When any segment of the project path is a link (macOS <c>/tmp</c> and
	/// <c>/var/folders</c>, or a Windows junction) the two spellings differ, so a set difference between
	/// MSBuild items and task outputs wrongly reports freshly written files as stale.
	/// </para>
	/// <para>
	/// The canonical form is only ever used for comparison. Callers keep the original item spec so the
	/// paths surfaced to the rest of the build stay in the spelling the user provided.
	/// </para>
	/// <para>
	/// Canonicalization resolves the links of each path segment that already exists and appends the
	/// segments that do not, so paths for files which have not been created yet can be canonicalized
	/// without the failure a plain <c>realpath</c> would produce.
	/// </para>
	/// </remarks>
	internal sealed class PathCanonicalizer
	{
		/// <summary>Bounds link resolution so that a cycle of links cannot hang the build.</summary>
		const int MaxLinkHops = 40;

		/// <summary>
		/// Paths are compared case insensitively everywhere except Linux. macOS volumes can be case
		/// sensitive, but treating two spellings as the same file there only ever means a stale file is
		/// kept, which is far safer than deleting a file that is still needed.
		/// </summary>
		public static StringComparer Comparer { get; } =
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
				? StringComparer.Ordinal
				: StringComparer.OrdinalIgnoreCase;

		static readonly MethodInfo ResolveDirectoryLinkTarget =
			typeof(Directory).GetMethod("ResolveLinkTarget", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(bool) }, null);

		static readonly MethodInfo ResolveFileLinkTarget =
			typeof(File).GetMethod("ResolveLinkTarget", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(bool) }, null);

		readonly Dictionary<string, string> directoryCache = new Dictionary<string, string>(Comparer);

		/// <summary>
		/// Returns a canonical spelling of <paramref name="path"/>, or the input unchanged when it
		/// cannot be canonicalized. Never throws.
		/// </summary>
		public string Canonicalize(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return path;

			string full;
			try
			{
				full = Path.GetFullPath(path);
			}
			catch (Exception)
			{
				// An item spec can contain characters that are not valid in a path; leave it alone.
				return path;
			}

			return CanonicalizeFullPath(TrimTrailingSeparators(full), MaxLinkHops);
		}

		/// <summary>
		/// Builds a set of canonical paths that can be probed with <see cref="Canonicalize"/> results.
		/// </summary>
		public HashSet<string> CreateSet(IEnumerable<string> paths)
		{
			var set = new HashSet<string>(Comparer);

			if (paths is not null)
			{
				foreach (var path in paths)
				{
					if (!string.IsNullOrWhiteSpace(path))
						set.Add(Canonicalize(path));
				}
			}

			return set;
		}

		string CanonicalizeFullPath(string full, int hops)
		{
			var parent = Path.GetDirectoryName(full);
			var name = Path.GetFileName(full);

			// A root such as "/" or "C:\" has nothing left to resolve.
			if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
				return full;

			return ResolveLink(Path.Combine(CanonicalizeDirectory(parent, hops), name), hops);
		}

		string CanonicalizeDirectory(string directory, int hops)
		{
			directory = TrimTrailingSeparators(directory);

			if (directoryCache.TryGetValue(directory, out var cached))
				return cached;

			var canonical = CanonicalizeFullPath(directory, hops);
			directoryCache[directory] = canonical;
			return canonical;
		}

		string ResolveLink(string path, int hops)
		{
			if (hops <= 0)
				return path;

			var target = GetLinkTarget(path);
			if (target is null)
				return path;

			var resolved = TrimTrailingSeparators(target.FullName);
			if (Comparer.Equals(resolved, path))
				return path;

			// The target itself may live under directories that are links, so canonicalize it too.
			return CanonicalizeFullPath(resolved, hops - 1);
		}

		static FileSystemInfo GetLinkTarget(string path)
		{
			// Directory/File.ResolveLinkTarget only exist on .NET 6 and later. This assembly targets
			// netstandard2.0 so that it can also load into MSBuild.exe on .NET Framework, where link
			// resolution is simply unavailable and comparison falls back to the lexical full path.
			var resolve = Directory.Exists(path)
				? ResolveDirectoryLinkTarget
				: File.Exists(path)
					? ResolveFileLinkTarget
					: null;

			if (resolve is null)
				return null;

			try
			{
				return resolve.Invoke(null, new object[] { path, /* returnFinalTarget: */ true }) as FileSystemInfo;
			}
			catch (Exception)
			{
				// Cyclic links, missing permissions or a racing delete: keep the unresolved spelling.
				return null;
			}
		}

		static string TrimTrailingSeparators(string path)
		{
			var root = Path.GetPathRoot(path) ?? string.Empty;

			var end = path.Length;
			while (end > root.Length && (path[end - 1] == Path.DirectorySeparatorChar || path[end - 1] == Path.AltDirectorySeparatorChar))
				end--;

			return end == path.Length ? path : path.Substring(0, end);
		}
	}
}
