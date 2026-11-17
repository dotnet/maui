using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Produces a canonical spelling of a file system path so that two differently spelled paths which
	/// point at the same file compare as equal, and so that a caller can tell whether a path really lives
	/// inside a directory it appears to live inside.
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
	/// cleanup forever, and it is unnecessary: deleting a path whose leaf is a link removes the link
	/// rather than whatever it points at.
	/// </para>
	/// <para>
	/// Every decision here is made so that its failure mode is "keep the file". Comparing two paths for
	/// equality is case insensitive off Linux, so an unexpected spelling difference makes a stale file
	/// look live and it is kept. Containment is case sensitive everywhere, so an unexpected spelling
	/// difference puts a file outside the root and it is kept. A directory whose link target cannot be
	/// resolved makes the whole path unresolvable and it is kept.
	/// </para>
	/// </remarks>
	internal sealed class PathCanonicalizer
	{
		/// <summary>Bounds link resolution so that a cycle of links cannot hang the build.</summary>
		const int MaxLinkHops = 40;

		/// <summary>
		/// Compares two canonical paths for <em>equality</em>. Case insensitive everywhere except Linux.
		/// </summary>
		/// <remarks>
		/// This is only ever used to ask "is this file one the build just wrote?". Answering yes when the
		/// two spellings are actually different files on a case sensitive volume keeps a stale file, which
		/// is harmless. It must never be used to decide whether a path is inside a directory; see
		/// <see cref="IsUnder"/>.
		/// </remarks>
		public static StringComparer Comparer { get; } =
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
				? StringComparer.Ordinal
				: StringComparer.OrdinalIgnoreCase;

		static readonly MethodInfo ResolveDirectoryLinkTargetMethod =
			typeof(Directory).GetMethod("ResolveLinkTarget", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(bool) }, null);

		// Cache keys are exact spellings. Two directories whose names differ only by case can be two
		// different directories on a case sensitive volume, so a case insensitive key could hand back
		// another directory's resolved target.
		readonly Dictionary<string, string> directoryCache = new Dictionary<string, string>(StringComparer.Ordinal);

		readonly MethodInfo resolveDirectoryLinkTarget;

		public PathCanonicalizer()
			: this(ResolveDirectoryLinkTargetMethod is not null)
		{
		}

		/// <summary>
		/// Lets tests exercise the behaviour of hosts without <c>Directory.ResolveLinkTarget</c>, which is
		/// every .NET Framework host, including MSBuild.exe.
		/// </summary>
		internal PathCanonicalizer(bool allowLinkResolution)
		{
			resolveDirectoryLinkTarget = allowLinkResolution ? ResolveDirectoryLinkTargetMethod : null;
		}

		/// <summary>
		/// Whether this host can resolve directory links at all. When it cannot, any path that passes
		/// through a reparse point is reported as unresolvable rather than guessed at.
		/// </summary>
		public bool CanResolveLinks => resolveDirectoryLinkTarget is not null;

		/// <summary>
		/// Returns the key to compare <paramref name="path"/> by: its link resolved directory plus its
		/// file name unchanged. Returns <see langword="null"/> when the path cannot be interpreted or when
		/// its directory passes through a link this host cannot resolve.
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
		/// when it cannot be interpreted or passes through an unresolvable link. Segments that do not
		/// exist are kept as they are, so a path the build has not written yet can still be canonicalized.
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
		/// must already be comparison keys produced by this type.
		/// </summary>
		/// <remarks>
		/// Deliberately case sensitive on every platform, including Windows and macOS. This decides
		/// whether a file may be deleted, so it has to fail closed. On a case sensitive volume
		/// <c>…/r</c> and <c>…/R</c> are two different directories, and comparing them case insensitively
		/// would report a file that really lives in the sibling as being inside the root, letting a delete
		/// escape. Being stricter than the volume costs nothing worse than leaving a stale file behind,
		/// because the paths on both sides are built from the same MSBuild properties and so share their
		/// spelling.
		/// </remarks>
		public static bool IsUnder(string key, string root)
		{
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(root))
				return false;

			if (string.Equals(key, root, StringComparison.Ordinal))
				return true;

			if (key.Length <= root.Length || !key.StartsWith(root, StringComparison.Ordinal))
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

			string canonical;
			if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
			{
				// A root resolves to itself, which also terminates the walk.
				canonical = full;
			}
			else
			{
				var canonicalParent = Canonicalize(parent, hops);

				canonical = canonicalParent is null
					? null
					: ResolveDirectoryLink(Path.Combine(canonicalParent, name), hops);
			}

			directoryCache[full] = canonical;
			return canonical;
		}

		string ResolveDirectoryLink(string path, int hops)
		{
			switch (Probe(path))
			{
				case DirectoryKind.Missing:
					// Nothing is there at all, so nothing can be reached through it either and the lexical
					// spelling is the whole truth. This is also the case for an output the build has not
					// written yet, which still has to be canonicalizable.
					return path;

				case DirectoryKind.Ordinary:
					return path;

				case DirectoryKind.Unknown:
					// Something is there but it cannot be identified, so it cannot be ruled out as a link.
					// Refuse to place anything under it rather than assume the lexical spelling is real.
					return null;
			}

			// From here on this really is a link or junction. Where it points decides whether everything
			// below it is inside the root, so guessing is not an option: either resolve it or give up.
			if (resolveDirectoryLinkTarget is null || hops <= 0)
				return null;

			FileSystemInfo target;
			try
			{
				target = resolveDirectoryLinkTarget.Invoke(null, new object[] { path, /* returnFinalTarget: */ true }) as FileSystemInfo;
			}
			catch (Exception)
			{
				// A cycle of links, missing permissions or a racing delete. A dangling link still resolves,
				// to its absent target, and that target decides containment as usual.
				return null;
			}

			if (target is null)
				return null;

			var resolved = TrimTrailingSeparators(target.FullName);

			// Ordinal: on a case sensitive volume a link from "…/r" to "…/R" points at a different
			// directory, and treating the two as the same would leave the link unresolved.
			if (string.Equals(resolved, path, StringComparison.Ordinal))
				return path;

			// The target itself may live under directories that are links.
			return Canonicalize(resolved, hops - 1);
		}

		/// <summary>What a path turned out to be, as far as it can be determined.</summary>
		enum DirectoryKind
		{
			/// <summary>Nothing exists at this path.</summary>
			Missing,

			/// <summary>An ordinary entry that is not a link and can be trusted lexically.</summary>
			Ordinary,

			/// <summary>A symbolic link, junction or other reparse point.</summary>
			Link,

			/// <summary>Something is there, but it could not be identified.</summary>
			Unknown,
		}

		/// <summary>
		/// Classifies <paramref name="path"/> without following it.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="Directory.Exists(string)"/> cannot be used for this. It answers "can I open a
		/// directory here", so it reports <see langword="false"/> for a link whose target is missing, for
		/// a cycle of links, and for an entry it lacks permission to inspect. Treating those as "not a
		/// link" would hand back the lexical spelling and let everything apparently beneath them look
		/// contained.
		/// </para>
		/// <para>
		/// <see cref="FileSystemInfo.Attributes"/> cannot be used either: for a path that does not exist
		/// it yields <c>(FileAttributes)(-1)</c> rather than throwing, which has every bit set and so
		/// claims to be a reparse point.
		/// </para>
		/// <para>
		/// <see cref="File.GetAttributes(string)"/> does distinguish all of these, and works on every host
		/// including .NET Framework: it reports the attributes of the entry itself rather than of whatever
		/// it points at, throws for a path that is genuinely absent, and throws something else when the
		/// entry cannot be inspected.
		/// </para>
		/// </remarks>
		static DirectoryKind Probe(string path)
		{
			try
			{
				var attributes = File.GetAttributes(path);

				return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint
					? DirectoryKind.Link
					: DirectoryKind.Ordinary;
			}
			catch (FileNotFoundException)
			{
				return DirectoryKind.Missing;
			}
			catch (DirectoryNotFoundException)
			{
				return DirectoryKind.Missing;
			}
			catch (Exception)
			{
				// Denied permission, a path the platform rejects, or a racing change. Anything that is not
				// a definite "nothing is here" has to count as unsafe.
				return DirectoryKind.Unknown;
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
