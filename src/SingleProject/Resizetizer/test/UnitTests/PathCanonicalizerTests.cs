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
		public void NullAndEmptyPathsAreReturnedUnchanged()
		{
			var canonicalizer = new PathCanonicalizer();

			Assert.Null(canonicalizer.Canonicalize(null));
			Assert.Equal("", canonicalizer.Canonicalize(""));
			Assert.Equal("   ", canonicalizer.Canonicalize("   "));
		}

		[Fact]
		public void InvalidPathsAreReturnedUnchanged()
		{
			var canonicalizer = new PathCanonicalizer();

			Assert.Equal("bad\0path", canonicalizer.Canonicalize("bad\0path"));
		}

		[Fact]
		public void RelativePathsAreRooted()
		{
			var canonicalizer = new PathCanonicalizer();

			var canonical = canonicalizer.Canonicalize(Path.Combine("obj", "resizetizer", "r", "camera.png"));

			Assert.True(Path.IsPathRooted(canonical), $"Expected a rooted path, got '{canonical}'.");
		}

		[Fact]
		public void RedundantSegmentsAndTrailingSeparatorsAreNormalized()
		{
			var canonicalizer = new PathCanonicalizer();
			var directory = Path.Combine(DestinationDirectory, "images");
			Directory.CreateDirectory(directory);

			var messy = Path.Combine(DestinationDirectory, ".", "obj", "..", "images") + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar;

			Assert.Equal(canonicalizer.Canonicalize(directory), canonicalizer.Canonicalize(messy), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void MissingPathsDoNotThrowAndStayStable()
		{
			var canonicalizer = new PathCanonicalizer();
			var missing = Path.Combine(DestinationDirectory, "never", "created", "at", "all.png");

			var canonical = canonicalizer.Canonicalize(missing);

			Assert.True(Path.IsPathRooted(canonical), $"Expected a rooted path, got '{canonical}'.");
			Assert.EndsWith(Path.Combine("never", "created", "at", "all.png"), canonical, StringComparison.Ordinal);
			Assert.Equal(canonical, canonicalizer.Canonicalize(canonical), PathCanonicalizer.Comparer);
		}

		[Fact]
		public void LinkedAndPhysicalDirectoriesCanonicalizeToTheSamePath()
		{
			var (physical, link) = CreateLinkedDirectory();
			if (link is null)
				return;

			var canonicalizer = new PathCanonicalizer();

			Assert.Equal(canonicalizer.Canonicalize(physical), canonicalizer.Canonicalize(link), PathCanonicalizer.Comparer);
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
				canonicalizer.Canonicalize(Path.Combine(physical, "camera.png")),
				canonicalizer.Canonicalize(Path.Combine(link, "camera.png")),
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
				canonicalizer.Canonicalize(Path.Combine(physical, "future", "camera.png")),
				canonicalizer.Canonicalize(Path.Combine(link, "future", "camera.png")),
				PathCanonicalizer.Comparer);
		}

		[Fact]
		public void UnrelatedPathsStayDifferent()
		{
			var canonicalizer = new PathCanonicalizer();

			var one = canonicalizer.Canonicalize(Path.Combine(DestinationDirectory, "drawable", "camera.png"));
			var two = canonicalizer.Canonicalize(Path.Combine(DestinationDirectory, "drawable", "bicycle.png"));

			Assert.NotEqual(one, two, PathCanonicalizer.Comparer);
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
