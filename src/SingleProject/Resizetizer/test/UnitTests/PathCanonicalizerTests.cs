using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class PathCanonicalizerTests : BaseTest
	{
		public PathCanonicalizerTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Fact]
		public void NullAndEmptyPathsHaveNoKey()
		{
			var canonicalizer = new PathCanonicalizer();

			Assert.Null(canonicalizer.GetComparisonKey(null));
			Assert.Null(canonicalizer.GetComparisonKey(""));
			Assert.Null(canonicalizer.GetComparisonKey("   "));
			Assert.Null(canonicalizer.CanonicalizeDirectory(null));
			Assert.Null(canonicalizer.CanonicalizeDirectory(""));
		}

		[Fact]
		public void InvalidPathsHaveNoKey()
		{
			var canonicalizer = new PathCanonicalizer();

			Assert.Null(canonicalizer.GetComparisonKey("bad\0path"));
			Assert.Null(canonicalizer.CanonicalizeDirectory("bad\0path"));
		}

		[Fact]
		public void RelativePathsAreRooted()
		{
			var canonicalizer = new PathCanonicalizer();

			var canonical = canonicalizer.GetComparisonKey(Path.Combine("obj", "resizetizer", "r", "camera.png"));

			Assert.True(Path.IsPathRooted(canonical), $"Expected a rooted path, got '{canonical}'.");
		}

		[Fact]
		public void RedundantSegmentsAndTrailingSeparatorsAreNormalized()
		{
			var canonicalizer = new PathCanonicalizer();
			var directory = Path.Combine(DestinationDirectory, "images");
			Directory.CreateDirectory(directory);

			var messy = Path.Combine(DestinationDirectory, ".", "obj", "..", "images") + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar;

			Assert.Equal(canonicalizer.CanonicalizeDirectory(directory), canonicalizer.CanonicalizeDirectory(messy), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void MissingPathsDoNotThrowAndStayStable()
		{
			var canonicalizer = new PathCanonicalizer();
			var missing = Path.Combine(DestinationDirectory, "never", "created", "at", "all.png");

			var canonical = canonicalizer.GetComparisonKey(missing);

			Assert.True(Path.IsPathRooted(canonical), $"Expected a rooted path, got '{canonical}'.");
			Assert.EndsWith(Path.Combine("never", "created", "at", "all.png"), canonical, StringComparison.Ordinal);
			Assert.Equal(canonical, canonicalizer.GetComparisonKey(canonical), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void LinkedAndPhysicalDirectoriesCanonicalizeToTheSamePath()
		{
			var (physical, link) = CreateLinkedDirectory();
			if (link is null)
				return;

			var canonicalizer = new PathCanonicalizer();

			Assert.Equal(canonicalizer.CanonicalizeDirectory(physical), canonicalizer.CanonicalizeDirectory(link), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void ExistingFileBehindALinkCanonicalizesToThePhysicalFile()
		{
			var (physical, link) = CreateLinkedDirectory();
			if (link is null)
				return;

			File.WriteAllText(Path.Combine(physical, "camera.png"), "image");

			var canonicalizer = new PathCanonicalizer();

			Assert.Equal(
				canonicalizer.GetComparisonKey(Path.Combine(physical, "camera.png")),
				canonicalizer.GetComparisonKey(Path.Combine(link, "camera.png")),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void NotYetCreatedFileBehindALinkCanonicalizesToThePhysicalFile()
		{
			var (physical, link) = CreateLinkedDirectory();
			if (link is null)
				return;

			var canonicalizer = new PathCanonicalizer();

			// The stale-file comparison has to work for outputs that the build has not written yet, so
			// canonicalization may only resolve the part of the path that already exists.
			Assert.Equal(
				canonicalizer.GetComparisonKey(Path.Combine(physical, "future", "camera.png")),
				canonicalizer.GetComparisonKey(Path.Combine(link, "future", "camera.png")),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void UnrelatedPathsStayDifferent()
		{
			var canonicalizer = new PathCanonicalizer();

			var one = canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "drawable", "camera.png"));
			var two = canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "drawable", "bicycle.png"));

			Assert.NotEqual(one, two, PathCanonicalizer.Comparer);
		}

		/// <summary>
		/// Only the directory is link resolved. Two names in one directory must stay distinct even when
		/// one is a link to the other, otherwise a stale alias would look like a live output.
		/// </summary>
		[Fact]
		public void ALinkToAFileInTheSameDirectoryKeepsItsOwnKey()
		{
			var directory = Path.Combine(DestinationDirectory, "drawable");
			Directory.CreateDirectory(directory);

			var target = Path.Combine(directory, "camera.png");
			var alias = Path.Combine(directory, "orphan.png");
			File.WriteAllText(target, "image");

			if (!SymbolicLink.TryCreateFileLink(alias, target, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();

			Assert.NotEqual(
				canonicalizer.GetComparisonKey(target),
				canonicalizer.GetComparisonKey(alias),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void IsUnderAcceptsTheRootItselfAndItsChildren()
		{
			var canonicalizer = new PathCanonicalizer();
			var root = canonicalizer.CanonicalizeDirectory(DestinationDirectory);

			Assert.True(PathCanonicalizer.IsUnder(root, root));
			Assert.True(PathCanonicalizer.IsUnder(canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "camera.png")), root));
			Assert.True(PathCanonicalizer.IsUnder(canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "a", "b", "camera.png")), root));
		}

		[Fact]
		public void IsUnderRejectsSiblingsParentsAndEmptyInput()
		{
			var canonicalizer = new PathCanonicalizer();
			var root = canonicalizer.CanonicalizeDirectory(Path.Combine(DestinationDirectory, "r"));

			// A name that merely starts with the root's name is not inside it.
			Assert.False(PathCanonicalizer.IsUnder(canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "r-backup", "camera.png")), root));
			Assert.False(PathCanonicalizer.IsUnder(canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "camera.png")), root));
			Assert.False(PathCanonicalizer.IsUnder(null, root));
			Assert.False(PathCanonicalizer.IsUnder(root, null));
		}

		/// <summary>
		/// Containment has to be case sensitive on every platform. On a case sensitive volume "…/r" and
		/// "…/R" are two different directories, and a link inside the root that leads into the sibling
		/// would otherwise be reported as living inside the root, letting a delete escape it.
		/// </summary>
		[Theory]
		[InlineData("/x/R/deep/secret.png", "/x/r")]
		[InlineData("/x/R/secret.png", "/x/r")]
		[InlineData("/X/r/secret.png", "/x/r")]
		[InlineData(@"C:\x\R\deep\secret.png", @"C:\x\r")]
		public void IsUnderIsCaseSensitiveSoACaseOnlySiblingIsNeverContained(string key, string root)
		{
			key = key.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			root = root.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

			Assert.False(PathCanonicalizer.IsUnder(key, root), $"'{key}' must not be treated as inside '{root}'.");
		}

		[Fact]
		public void IsUnderStillAcceptsAnExactCaseMatch()
		{
			var root = Path.Combine(Path.DirectorySeparatorChar + "x", "r");
			var key = Path.Combine(root, "deep", "camera.png");

			Assert.True(PathCanonicalizer.IsUnder(key, root));
		}

		/// <summary>
		/// A host without <c>Directory.ResolveLinkTarget</c> — every .NET Framework host, including
		/// MSBuild.exe — cannot tell where a link leads, so it must report the path as unresolvable rather
		/// than fall back to the lexical spelling and treat whatever is behind the link as contained.
		/// </summary>
		[Fact]
		public void WithoutLinkResolutionAPathThroughALinkHasNoKey()
		{
			// Root the test where no ancestor is itself a link, so the only reparse point in play is the
			// one the test creates. macOS temp directories live under /var, which is a link to /private/var.
			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var physical = Path.Combine(basePath, "physical");
			var link = Path.Combine(basePath, "link");
			Directory.CreateDirectory(physical);

			if (!SymbolicLink.TryCreateDirectoryLink(link, physical, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var fallback = new PathCanonicalizer(allowLinkResolution: false);

			Assert.False(fallback.CanResolveLinks);

			// The link cannot be followed, so nothing behind it can be placed.
			Assert.Null(fallback.CanonicalizeDirectory(link));
			Assert.Null(fallback.GetComparisonKey(Path.Combine(link, "camera.png")));

			// A directory that is not a link is unaffected, so cleanup keeps working everywhere else.
			Assert.Equal(physical, fallback.CanonicalizeDirectory(physical), PathCanonicalizer.Comparer);
			Assert.Equal(
				Path.Combine(physical, "camera.png"),
				fallback.GetComparisonKey(Path.Combine(physical, "camera.png")),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void WithLinkResolutionThePathThroughALinkStillResolves()
		{
			var (physical, link) = CreateLinkedDirectory();
			if (link is null)
				return;

			var canonicalizer = new PathCanonicalizer();

			Assert.True(canonicalizer.CanResolveLinks, "This host is expected to resolve links.");
			Assert.Equal(
				canonicalizer.GetComparisonKey(Path.Combine(physical, "camera.png")),
				canonicalizer.GetComparisonKey(Path.Combine(link, "camera.png")),
				PathCanonicalizer.Comparer);
		}

		/// <summary>
		/// A link whose target is gone still reports where it was pointing, so containment is decided by
		/// that target rather than by the link's own location inside the root.
		/// </summary>
		[Fact]
		public void ADanglingLinkResolvesToItsAbsentTargetRatherThanItself()
		{
			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var root = Path.Combine(basePath, "r");
			var absent = Path.Combine(basePath, "gone");
			Directory.CreateDirectory(root);

			var alias = Path.Combine(root, "alias");
			if (!SymbolicLink.TryCreateDirectoryLink(alias, absent, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();
			var key = canonicalizer.GetComparisonKey(Path.Combine(alias, "precious.png"));

			// Either it resolves to the absent target, which is outside the root, or it is refused
			// outright. What it must never be is the lexical spelling inside the root.
			Assert.NotEqual(Path.Combine(alias, "precious.png"), key ?? string.Empty, PathCanonicalizer.Comparer);
			Assert.False(PathCanonicalizer.IsUnder(key, canonicalizer.CanonicalizeDirectory(root)));
		}

		/// <summary>
		/// A cycle of links cannot be followed, so nothing beneath it can be placed.
		/// </summary>
		[Fact]
		public void ACyclicLinkHasNoKey()
		{
			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var first = Path.Combine(basePath, "first");
			var second = Path.Combine(basePath, "second");

			if (!SymbolicLink.TryCreateDirectoryLink(first, second, out var error) ||
				!SymbolicLink.TryCreateDirectoryLink(second, first, out error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();

			Assert.Null(canonicalizer.GetComparisonKey(Path.Combine(first, "precious.png")));
		}

		/// <summary>
		/// A genuinely absent directory is not ambiguous. It has to keep canonicalizing lexically, because
		/// an output the build has not written yet must still be comparable against the wildcard's result.
		/// </summary>
		[Fact]
		public void AnAbsentDirectoryIsNotTreatedAsAmbiguous()
		{
			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var neverCreated = Path.Combine(basePath, "never", "created");

			var canonicalizer = new PathCanonicalizer();

			Assert.Equal(neverCreated, canonicalizer.CanonicalizeDirectory(neverCreated), PathCanonicalizer.Comparer);
			Assert.Equal(
				Path.Combine(neverCreated, "camera.png"),
				canonicalizer.GetComparisonKey(Path.Combine(neverCreated, "camera.png")),
				PathCanonicalizer.Comparer);
			Assert.True(PathCanonicalizer.IsUnder(canonicalizer.GetComparisonKey(Path.Combine(neverCreated, "camera.png")), basePath));
		}

		/// <summary>
		/// A wildcard descends through a directory link, so the key of what it finds resolves outside the
		/// root it was rooted at. That is what lets the caller refuse to delete it.
		/// </summary>
		[Fact]
		public void APathReachedThroughADirectoryLinkResolvesOutOfTheRoot()
		{
			var root = Path.Combine(DestinationDirectory, "r");
			var outside = Path.Combine(DestinationDirectory, "outside");
			Directory.CreateDirectory(root);
			Directory.CreateDirectory(outside);

			if (!SymbolicLink.TryCreateDirectoryLink(Path.Combine(root, "alias"), outside, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();
			var canonicalRoot = canonicalizer.CanonicalizeDirectory(root);
			var throughTheLink = canonicalizer.GetComparisonKey(Path.Combine(root, "alias", "precious.png"));

			Assert.False(PathCanonicalizer.IsUnder(throughTheLink, canonicalRoot));
		}

		/// <summary>
		/// The resolution cache is keyed on the exact spelling. On a case sensitive volume two directories
		/// whose names differ only by case are two different directories, and a case insensitive cache key
		/// would hand back the wrong one's target.
		/// </summary>
		[Fact]
		public void DirectoriesDifferingOnlyByCaseAreCachedSeparately()
		{
			if (!OperatingSystem.IsLinux())
				return;

			var lower = Path.Combine(DestinationDirectory, "drawable");
			var upper = Path.Combine(DestinationDirectory, "DRAWABLE");
			var target = Path.Combine(DestinationDirectory, "elsewhere");
			Directory.CreateDirectory(target);
			Directory.CreateDirectory(upper);

			if (!SymbolicLink.TryCreateDirectoryLink(lower, target, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();

			// Resolve the link first so a bad cache would already be populated with its target.
			Assert.Equal(canonicalizer.CanonicalizeDirectory(target), canonicalizer.CanonicalizeDirectory(lower), StringComparer.Ordinal);
			Assert.Equal(upper, canonicalizer.CanonicalizeDirectory(upper), StringComparer.Ordinal);
		}

		[Fact]
		public void CanonicalizingIsIdempotent()
		{
			var (physical, link) = CreateLinkedDirectory();
			if (link is null)
				return;

			File.WriteAllText(Path.Combine(physical, "camera.png"), "image");

			var canonicalizer = new PathCanonicalizer();
			var once = canonicalizer.GetComparisonKey(Path.Combine(link, "camera.png"));

			Assert.Equal(once, canonicalizer.GetComparisonKey(once), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void ChainedLinksResolveToTheFinalTarget()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var first = Path.Combine(DestinationDirectory, "first");
			var second = Path.Combine(DestinationDirectory, "second");
			Directory.CreateDirectory(physical);

			if (!SymbolicLink.TryCreateDirectoryLink(first, physical, out var error) ||
				!SymbolicLink.TryCreateDirectoryLink(second, first, out error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();

			Assert.Equal(
				canonicalizer.GetComparisonKey(Path.Combine(physical, "camera.png")),
				canonicalizer.GetComparisonKey(Path.Combine(second, "camera.png")),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void ComparerFollowsThePlatformFileSystem()
		{
			// Only Linux path comparison is case sensitive. Everywhere else two spellings that differ by
			// case are treated as the same file, which can only ever keep a stale file rather than delete
			// a live one.
			Assert.Equal(OperatingSystem.IsLinux() ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase, PathCanonicalizer.Comparer);
		}

		[Fact]
		public void RootsAreCanonicalizedWithoutRecursingForever()
		{
			var canonicalizer = new PathCanonicalizer();
			var root = Path.GetPathRoot(Path.GetFullPath(DestinationDirectory));

			Assert.False(string.IsNullOrEmpty(root));
			Assert.Equal(root, canonicalizer.CanonicalizeDirectory(root), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void WindowsPathsKeepTheirDriveAndAcceptBothSeparators()
		{
			if (!OperatingSystem.IsWindows())
				return;

			var canonicalizer = new PathCanonicalizer();
			var path = Path.Combine(DestinationDirectory, "drawable", "camera.png");

			Assert.Equal(
				canonicalizer.GetComparisonKey(path),
				canonicalizer.GetComparisonKey(path.Replace('\\', '/')),
				PathCanonicalizer.Comparer);

			Assert.Equal(Path.GetPathRoot(path), Path.GetPathRoot(canonicalizer.GetComparisonKey(path)), StringComparer.OrdinalIgnoreCase);
		}

		[Fact]
		public void WindowsJunctionsResolveLikeDirectoryLinks()
		{
			if (!OperatingSystem.IsWindows())
				return;

			var physical = Path.Combine(DestinationDirectory, "physical");
			var junction = Path.Combine(DestinationDirectory, "junction");
			Directory.CreateDirectory(physical);

			if (!Junction.TryCreate(junction, physical, out var error))
			{
				Output.WriteLine($"Skipping: junctions are not available on this machine: {error}");
				return;
			}

			var canonicalizer = new PathCanonicalizer();

			Assert.Equal(
				canonicalizer.GetComparisonKey(Path.Combine(physical, "camera.png")),
				canonicalizer.GetComparisonKey(Path.Combine(junction, "camera.png")),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void BackslashesAreOrdinaryCharactersOnUnix()
		{
			if (OperatingSystem.IsWindows())
				return;

			var canonicalizer = new PathCanonicalizer();

			var withBackslash = canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "drawable\\camera.png"));
			var withSeparator = canonicalizer.GetComparisonKey(Path.Combine(DestinationDirectory, "drawable", "camera.png"));

			Assert.NotEqual(withBackslash, withSeparator, PathCanonicalizer.Comparer);
		}

		(string Physical, string Link) CreateLinkedDirectory()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var link = Path.Combine(DestinationDirectory, "link");

			Directory.CreateDirectory(physical);

			if (!SymbolicLink.TryCreateDirectoryLink(link, physical, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return (physical, null);
			}

			return (physical, link);
		}
	}
}
