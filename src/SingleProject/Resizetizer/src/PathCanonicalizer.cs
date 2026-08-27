using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Produces a canonical spelling of a file system path so that two differently spelled paths which
	/// point at the same file compare as equal.
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
	/// Only the <em>directory</em> part of a path is link resolved. The file name is kept verbatim, so two
	/// different names in one directory never collapse into one even when one of them is a link to the
	/// other. Resolving the leaf as well would let a stale alias masquerade as a live output and survive
	/// cleanup forever.
	/// </para>
	/// <para>
	/// Directories that do not exist yet are appended unresolved, so a path for a file the build has not
	/// written can be canonicalized without the failure a plain <c>realpath</c> would produce.
	/// </para>
	/// <para>
	/// The canonical form is only ever used for comparison. Callers keep the original item spec so the
	/// paths surfaced to the rest of the build stay in the spelling the user provided.
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

		/// <summary>The <see cref="StringComparison"/> matching <see cref="Comparer"/>.</summary>
		public static StringComparison Comparison { get; } =
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

		static readonly MethodInfo ResolveDirectoryLinkTarget =
			typeof(Directory).GetMethod("ResolveLinkTarget", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(bool) }, null);

		// Cache keys are exact spellings. Two directories whose names differ only by case can be two
		// different directories on a case sensitive volume, so a case insensitive key could hand back
		// another directory's resolved target.
		readonly Dictionary<string, string> directoryCache = new Dictionary<string, string>(StringComparer.Ordinal);

		/// <summary>
		/// Returns the key to compare <paramref name="path"/> by: its link resolved directory plus its
		/// file name unchanged. Returns <see langword="null"/> when the path cannot be interpreted.
		/// </summary>
		public string GetComparisonKey(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			string full;
			try
			{
				full = TrimTrailingSeparators(Path.GetFullPath(path));
			}
			catch (Exception)
			{
				// An item spec can contain characters that are not valid in a path.
				return null;
			}

			var parent = Path.GetDirectoryName(full);
			var name = Path.GetFileName(full);

			// A bare root such as "/" or "C:\" has no file name to keep.
			if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
				return CanonicalizeDirectory(full);

			var directory = CanonicalizeDirectory(parent);

			return directory is null ? null : Path.Combine(directory, name);
		}

		/// <summary>
		/// Returns <paramref name="directory"/> with every link in it resolved, or <see langword="null"/>
		/// when it cannot be interpreted. Segments that do not exist are kept as they are.
		/// </summary>
		public string CanonicalizeDirectory(string directory)
		{
			if (string.IsNullOrWhiteSpace(directory))
				return null;

			string full;
			try
			{
				full = TrimTrailingSeparators(Path.GetFullPath(directory));
			}
			catch (Exception)
			{
				return null;
			}

			return Canonicalize(full, MaxLinkHops);
		}

		/// <summary>
		/// Returns whether <paramref name="key"/> names something inside <paramref name="root"/>. Both
		/// must already be comparison keys.
		/// </summary>
		public static bool IsUnder(string key, string root)
		{
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(root))
				return false;

			if (Comparer.Equals(key, root))
				return true;

			if (key.Length <= root.Length || !key.StartsWith(root, Comparison))
				return false;

			// A root that already ends in a separator, such as "/" or "C:\", has no separator to skip.
			var last = root[root.Length - 1];
			if (last == Path.DirectorySeparatorChar || last == Path.AltDirectorySeparatorChar)
				return true;

			// Guard against "…/r-backup" being treated as living inside "…/r".
			var next = key[root.Length];
			return next == Path.DirectorySeparatorChar || next == Path.AltDirectorySeparatorChar;
		}

		string Canonicalize(string full, int hops)
		{
			if (directoryCache.TryGetValue(full, out var cached))
				return cached;

			var parent = Path.GetDirectoryName(full);
			var name = Path.GetFileName(full);

			// A root resolves to itself, which also terminates the walk.
			var canonical = string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)
				? full
				: ResolveDirectoryLink(Path.Combine(Canonicalize(parent, hops), name), hops);

			directoryCache[full] = canonical;
			return canonical;
		}

		string ResolveDirectoryLink(string path, int hops)
		{
			if (hops <= 0)
				return path;

			var target = GetDirectoryLinkTarget(path);
			if (target is null)
				return path;

			var resolved = TrimTrailingSeparators(target.FullName);
			if (Comparer.Equals(resolved, path))
				return path;

			// The target itself may live under directories that are links.
			return Canonicalize(resolved, hops - 1);
		}

		static FileSystemInfo GetDirectoryLinkTarget(string path)
		{
			// Directory.ResolveLinkTarget only exists on .NET 6 and later. This assembly targets
			// netstandard2.0 so that it can also load into MSBuild.exe on .NET Framework, where link
			// resolution is unavailable and comparison falls back to the lexical full path.
			if (ResolveDirectoryLinkTarget is null || !Directory.Exists(path))
				return null;

			try
			{
				return ResolveDirectoryLinkTarget.Invoke(null, new object[] { path, /* returnFinalTarget: */ true }) as FileSystemInfo;
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
