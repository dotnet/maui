using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class DetectStaleOutputFilesTaskTests : MSBuildTaskTestFixture<DetectStaleOutputFilesTask>
	{
		public DetectStaleOutputFilesTaskTests(ITestOutputHelper output)
			: base(output)
		{
		}

		DetectStaleOutputFilesTask GetNewTask(string[] files, string[] knownOutputs, string root = null) =>
			new DetectStaleOutputFilesTask
			{
				Root = root ?? DestinationDirectory,
				Files = files?.Select(f => (ITaskItem)new TaskItem(f)).ToArray(),
				KnownOutputs = knownOutputs?.Select(f => (ITaskItem)new TaskItem(f)).ToArray(),
				BuildEngine = this,
			};

		[Fact]
		public void NoFilesProducesNoStaleFiles()
		{
			var task = GetNewTask(null, null);

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		[Fact]
		public void EverythingIsStaleWhenNothingIsExpected()
		{
			var file = Path.Combine(DestinationDirectory, "camera.png");
			WriteFile(file);

			var task = GetNewTask(new[] { file }, Array.Empty<string>());

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(file), Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void OnlyUnexpectedFilesAreStale()
		{
			var kept = Path.Combine(DestinationDirectory, "camera.png");
			var stale = Path.Combine(DestinationDirectory, "orphan.png");
			WriteFile(kept);
			WriteFile(stale);

			var task = GetNewTask(new[] { kept, stale }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void StaleFilesUseDeletionSafeItemSpecsAndKeepMetadata()
		{
			var stale = Path.Combine(DestinationDirectory, "orphan.png");
			WriteFile(stale);
			var item = new TaskItem(stale);
			item.SetMetadata("_ResizetizerDpiPath", "drawable-xhdpi");

			var task = new DetectStaleOutputFilesTask
			{
				Root = DestinationDirectory,
				Files = new ITaskItem[] { item },
				KnownOutputs = Array.Empty<ITaskItem>(),
				BuildEngine = this,
			};

			Assert.True(task.Execute());

			var result = Assert.Single(task.StaleFiles);
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), result.ItemSpec);
			Assert.Equal("drawable-xhdpi", result.GetMetadata("_ResizetizerDpiPath"));
		}

		[Fact]
		public void RedundantSegmentsDoNotMakeAFileStale()
		{
			var kept = Path.Combine(DestinationDirectory, "camera.png");
			var spelledDifferently = Path.Combine(DestinationDirectory, ".", "obj", "..", "camera.png");
			WriteFile(kept);

			var task = GetNewTask(new[] { spelledDifferently }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		[Fact]
		public void FilesReachedThroughALinkedDirectoryAreNotStale()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var link = Path.Combine(DestinationDirectory, "link");
			Directory.CreateDirectory(Path.Combine(physical, "drawable"));

			if (!SymbolicLink.TryCreateDirectoryLink(link, physical, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var kept = Path.Combine(physical, "drawable", "camera.png");
			File.WriteAllText(kept, "image");

			// This is the shape of the bug: MSBuild enumerates the directory through the link while the
			// task reports the file it wrote through the physical path.
			var task = GetNewTask(new[] { Path.Combine(link, "drawable", "camera.png") }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		[Fact]
		public void StaleFilesInALinkedDirectoryAreStillDetected()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var link = Path.Combine(DestinationDirectory, "link");
			Directory.CreateDirectory(Path.Combine(physical, "drawable"));

			if (!SymbolicLink.TryCreateDirectoryLink(link, physical, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var kept = Path.Combine(physical, "drawable", "camera.png");
			var stale = Path.Combine(link, "drawable", "orphan.png");
			File.WriteAllText(kept, "image");
			File.WriteAllText(stale, "image");

			var task = GetNewTask(new[] { Path.Combine(link, "drawable", "camera.png"), stale }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void EquivalentAliasSpellingsProduceOneDeletionPath()
		{
			var root = Path.Combine(DestinationDirectory, "root");
			var physical = Path.Combine(root, "physical");
			var alias = Path.Combine(root, "alias");
			Directory.CreateDirectory(physical);

			if (!TryCreateDirectoryAlias(alias, physical))
				return;

			var stale = Path.Combine(physical, "orphan.png");
			WriteFile(stale);

			var task = GetNewTask(
				new[] { stale, Path.Combine(alias, "orphan.png") },
				Array.Empty<string>(),
				root);

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// The item specs handed back are what <c>&lt;Delete&gt;</c> acts on, so they must use the resolved
		/// parent rather than the wildcard spelling. Retargeting that wildcard alias later must not redirect
		/// the delete.
		/// </summary>
		[Fact]
		public void StaleFilesUseTheResolvedParentWhenCanonicalizationChangesTheSpelling()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var link = Path.Combine(DestinationDirectory, "link");
			Directory.CreateDirectory(Path.Combine(physical, "drawable"));

			if (!SymbolicLink.TryCreateDirectoryLink(link, physical, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var stale = Path.Combine(link, "drawable", "orphan.png");
			File.WriteAllText(stale, "image");

			var canonicalizer = new PathCanonicalizer();
			Assert.NotEqual(stale, canonicalizer.GetComparisonKey(stale), PathCanonicalizer.Comparer);

			var task = GetNewTask(new[] { stale }, Array.Empty<string>());

			Assert.True(task.Execute());
			Assert.Equal(canonicalizer.GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
			Assert.NotEqual(stale, Assert.Single(task.StaleFiles).ItemSpec, PathCanonicalizer.Comparer);
		}

		/// <summary>
		/// A file inside the intermediate directory that is itself a link to somewhere else is still
		/// reported by its inside spelling, because only the directory part of a path is link resolved.
		/// <c>&lt;Delete&gt;</c> then removes the link and never the file it points at.
		/// </summary>
		[Fact]
		public void ALinkPointingOutsideTheDirectoryIsReportedByItsInsidePath()
		{
			var outside = Path.Combine(DestinationDirectory, "outside");
			var intermediate = Path.Combine(DestinationDirectory, "intermediate");
			Directory.CreateDirectory(outside);
			Directory.CreateDirectory(intermediate);

			var target = Path.Combine(outside, "source.png");
			File.WriteAllText(target, "not a build output");

			var inside = Path.Combine(intermediate, "linked.png");
			if (!SymbolicLink.TryCreateFileLink(inside, target, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var task = GetNewTask(new[] { inside }, Array.Empty<string>(), root: intermediate);

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(inside), Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// A recursive MSBuild wildcard walks through a directory link, so it can name a file that is not
		/// actually inside the intermediate folder. Deleting it would destroy the real file rather than
		/// the link, so it must never be reported.
		/// </summary>
		[Fact]
		public void FilesReachedThroughADirectoryLinkOutOfTheRootAreNeverStale()
		{
			var outside = Path.Combine(DestinationDirectory, "outside");
			var intermediate = Path.Combine(DestinationDirectory, "intermediate");
			Directory.CreateDirectory(outside);
			Directory.CreateDirectory(intermediate);

			var precious = Path.Combine(outside, "precious.png");
			File.WriteAllText(precious, "not a build output");

			if (!SymbolicLink.TryCreateDirectoryLink(Path.Combine(intermediate, "alias"), outside, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			// This is exactly what the wildcard yields once it descends through the link.
			var throughTheLink = Path.Combine(intermediate, "alias", "precious.png");
			var genuinelyStale = Path.Combine(intermediate, "orphan.png");
			File.WriteAllText(genuinelyStale, "left over");

			var task = GetNewTask(new[] { throughTheLink, genuinelyStale }, Array.Empty<string>(), root: intermediate);

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(genuinelyStale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// Only the directory part of a path is link resolved. If the file name were resolved too, a stale
		/// alias of a live output would compare equal to it and survive cleanup forever.
		/// </summary>
		[Fact]
		public void AStaleLinkToALiveOutputIsStillDeleted()
		{
			var intermediate = Path.Combine(DestinationDirectory, "intermediate");
			Directory.CreateDirectory(intermediate);

			var kept = Path.Combine(intermediate, "camera.png");
			File.WriteAllText(kept, "image");

			var alias = Path.Combine(intermediate, "orphan.png");
			if (!SymbolicLink.TryCreateFileLink(alias, kept, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var task = GetNewTask(new[] { kept, alias }, new[] { kept }, root: intermediate);

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(alias), Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void ASiblingOfTheRootIsNotTreatedAsBeingInsideIt()
		{
			var intermediate = Path.Combine(DestinationDirectory, "r");
			var sibling = Path.Combine(DestinationDirectory, "r-backup");
			Directory.CreateDirectory(intermediate);
			Directory.CreateDirectory(sibling);

			var outsideRoot = Path.Combine(sibling, "camera.png");
			File.WriteAllText(outsideRoot, "image");

			var task = GetNewTask(new[] { outsideRoot }, Array.Empty<string>(), root: intermediate);

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		[Fact]
		public void AnUnresolvableRootDisablesDetectionRatherThanDeleting()
		{
			var stale = Path.Combine(DestinationDirectory, "orphan.png");

			var task = GetNewTask(new[] { stale }, Array.Empty<string>(), root: "   ");

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		[Fact]
		public void CanonicalizeDirectoryTaskAnchorsAnExistingLinkedRoot()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var alias = Path.Combine(DestinationDirectory, "alias");
			Directory.CreateDirectory(physical);

			if (!TryCreateDirectoryAlias(alias, physical))
				return;

			var task = new CanonicalizeDirectoryTask
			{
				Directory = alias,
				BuildEngine = this,
			};

			Assert.True(task.Execute());
			Assert.Equal(
				new PathCanonicalizer().CanonicalizeExistingDirectory(physical),
				task.CanonicalDirectory.ItemSpec,
				PathCanonicalizer.Comparer);
		}

		/// <summary>
		/// On a case sensitive volume the root's case-only sibling is a different directory, so a link
		/// inside the root that leads into it must not make its contents look contained. A recursive
		/// MSBuild wildcard really does surface these: its own cycle detection compares paths case
		/// insensitively, so it walks straight into the sibling believing it is still inside the root.
		/// </summary>
		[Fact]
		public void FilesInACaseOnlySiblingOfTheRootAreNeverStale()
		{
			var root = Path.Combine(DestinationDirectory, "r");
			var sibling = Path.Combine(DestinationDirectory, "R");
			Directory.CreateDirectory(root);

			if (!IsCaseSensitiveVolume(DestinationDirectory))
			{
				Output.WriteLine("Skipping: this volume is case insensitive, so the sibling is the root itself.");
				return;
			}

			Directory.CreateDirectory(Path.Combine(sibling, "deep"));
			var outside = Path.Combine(sibling, "deep", "secret.png");
			File.WriteAllText(outside, "not a build output");

			if (!SymbolicLink.TryCreateDirectoryLink(Path.Combine(root, "alias"), Path.Combine(sibling, "deep"), out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var throughTheLink = Path.Combine(root, "alias", "secret.png");
			var genuinelyStale = Path.Combine(root, "orphan.png");
			File.WriteAllText(genuinelyStale, "left over");

			var task = GetNewTask(new[] { throughTheLink, genuinelyStale }, Array.Empty<string>(), root: root);

			Assert.True(task.Execute());
			Assert.Equal(genuinelyStale, Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// A host without <c>Directory.ResolveLinkTarget</c>, which is every .NET Framework host including
		/// MSBuild.exe, cannot tell where a junction leads. It has to leave anything behind one alone
		/// instead of assuming the lexical spelling is the real location.
		/// </summary>
		[Fact]
		public void WithoutLinkResolutionFilesBehindAReparsePointAreNeverStale()
		{
			// Root the test where no ancestor is itself a link, so the only reparse point in play is the
			// one the test creates. macOS temp directories live under /var, which is a link to /private/var.
			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var root = Path.Combine(basePath, "r");
			var outside = Path.Combine(basePath, "outside");
			Directory.CreateDirectory(root);
			Directory.CreateDirectory(outside);

			var precious = Path.Combine(outside, "precious.png");
			File.WriteAllText(precious, "not a build output");

			if (!SymbolicLink.TryCreateDirectoryLink(Path.Combine(root, "alias"), outside, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var throughTheLink = Path.Combine(root, "alias", "precious.png");
			var genuinelyStale = Path.Combine(root, "orphan.png");
			File.WriteAllText(genuinelyStale, "left over");

			var task = GetNewTask(new[] { throughTheLink, genuinelyStale }, Array.Empty<string>(), root: root);
			task.AllowLinkResolution = false;

			Assert.True(task.Execute());

			// The file behind the junction is left alone, while ordinary cleanup still happens.
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(genuinelyStale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// This is the actual full-framework shape: MSBuild.exe can identify a Windows junction as a
		/// reparse point but cannot resolve its target through <c>Directory.ResolveLinkTarget</c>.
		/// </summary>
		[Fact]
		public void WithoutLinkResolutionFilesBehindAWindowsJunctionAreNeverStale()
		{
			if (!OperatingSystem.IsWindows())
				return;

			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var root = Path.Combine(basePath, "r");
			var outside = Path.Combine(basePath, "outside");
			Directory.CreateDirectory(root);
			Directory.CreateDirectory(outside);

			var precious = Path.Combine(outside, "precious.png");
			WriteFile(precious);

			var alias = Path.Combine(root, "alias");
			if (!Junction.TryCreate(alias, outside, out var error))
			{
				Output.WriteLine($"Skipping: junctions are not available on this machine: {error}");
				return;
			}

			var stale = Path.Combine(root, "orphan.png");
			WriteFile(stale);

			var task = GetNewTask(new[] { Path.Combine(alias, "precious.png"), stale }, Array.Empty<string>(), root: root);
			task.AllowLinkResolution = false;

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// When the root itself sits behind a reparse point the fallback host cannot place anything, so it
		/// detects nothing at all rather than deleting on a guess.
		/// </summary>
		[Fact]
		public void WithoutLinkResolutionARootBehindAReparsePointDetectsNothing()
		{
			var physical = Path.Combine(DestinationDirectory, "physical");
			var link = Path.Combine(DestinationDirectory, "link");
			Directory.CreateDirectory(physical);

			if (!SymbolicLink.TryCreateDirectoryLink(link, physical, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var stale = Path.Combine(physical, "orphan.png");
			File.WriteAllText(stale, "left over");

			var task = GetNewTask(new[] { Path.Combine(link, "orphan.png") }, Array.Empty<string>(), root: link);
			task.AllowLinkResolution = false;

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		/// <summary>
		/// A link whose target no longer exists, a cycle of links, and an entry that cannot be inspected
		/// all make <see cref="Directory.Exists(string)"/> report <see langword="false"/>, exactly like an
		/// ordinary absent directory. Anything reached through one of them must still be left alone: the
		/// wildcard enumerated it while the link was intact, and by the time <c>&lt;Delete&gt;</c> runs the
		/// link may point somewhere real again.
		/// </summary>
		[Theory]
		[InlineData(BrokenLinkKind.Dangling)]
		[InlineData(BrokenLinkKind.Cyclic)]
		[InlineData(BrokenLinkKind.Inaccessible)]
		public void FilesBehindABrokenOrUnreadableLinkAreNeverStale(BrokenLinkKind kind)
		{
			AssertOnlyTheOrdinaryStaleFileIsReported(kind, allowLinkResolution: true);
		}

		/// <summary>
		/// The same three states on a host that cannot resolve links at all, which is every .NET Framework
		/// host including MSBuild.exe.
		/// </summary>
		[Theory]
		[InlineData(BrokenLinkKind.Dangling)]
		[InlineData(BrokenLinkKind.Cyclic)]
		[InlineData(BrokenLinkKind.Inaccessible)]
		public void WithoutLinkResolutionFilesBehindABrokenOrUnreadableLinkAreNeverStale(BrokenLinkKind kind)
		{
			AssertOnlyTheOrdinaryStaleFileIsReported(kind, allowLinkResolution: false);
		}

		void AssertOnlyTheOrdinaryStaleFileIsReported(BrokenLinkKind kind, bool allowLinkResolution)
		{
			// Root the test where no ancestor is itself a link, so the only unusual entry in play is the
			// one the test creates. macOS temp directories live under /var, which is a link to /private/var,
			// and a host that cannot resolve links could not place the root itself.
			var basePath = new PathCanonicalizer().CanonicalizeDirectory(DestinationDirectory);
			var root = Path.Combine(basePath, "r");
			Directory.CreateDirectory(root);

			if (!TryCreateBrokenEntry(kind, root, basePath, out var traversed, out var skip))
			{
				Output.WriteLine(skip);
				return;
			}

			try
			{
				var throughTheBrokenEntry = Path.Combine(traversed, "precious.png");
				var genuinelyStale = Path.Combine(root, "orphan.png");
				File.WriteAllText(genuinelyStale, "left over");

				var task = GetNewTask(new[] { throughTheBrokenEntry, genuinelyStale }, Array.Empty<string>(), root: root);
				task.AllowLinkResolution = allowLinkResolution;

				Assert.True(task.Execute());

				// Ordinary cleanup still happens; only the path through the broken entry is spared.
				Assert.Equal(new PathCanonicalizer().GetDeletionPath(genuinelyStale), Assert.Single(task.StaleFiles).ItemSpec);
			}
			finally
			{
				Unlock(kind, root);
			}
		}

		/// <summary>
		/// A wildcard item whose directory has disappeared is unsafe: it may be recreated as a link before
		/// Delete runs. Rejecting it must not disable cleanup of an independently verified stale file.
		/// </summary>
		[Fact]
		public void AMissingEnumeratedCandidateIsRejectedWithoutDisablingCleanup()
		{
			var root = Path.Combine(DestinationDirectory, "r");
			Directory.CreateDirectory(root);

			var stale = Path.Combine(root, "drawable", "orphan.png");
			Directory.CreateDirectory(Path.GetDirectoryName(stale));
			File.WriteAllText(stale, "left over");

			// A sibling directory that was never created at all.
			var neverCreated = Path.Combine(root, "drawable-xhdpi", "orphan.png");

			var task = GetNewTask(new[] { stale, neverCreated }, Array.Empty<string>(), root: root);

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// Known outputs are future paths, not wildcard snapshots. Their missing suffixes must remain
		/// comparable without weakening the stricter treatment of enumerated candidates.
		/// </summary>
		[Fact]
		public void MissingFutureKnownOutputsDoNotDisableOrdinaryCleanup()
		{
			var root = Path.Combine(DestinationDirectory, "r");
			Directory.CreateDirectory(root);

			var stale = Path.Combine(root, "drawable", "orphan.png");
			WriteFile(stale);
			var future = Path.Combine(root, "drawable-xhdpi", "camera.png");

			var task = GetNewTask(new[] { stale }, new[] { future }, root: root);

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		public enum BrokenLinkKind
		{
			/// <summary>A link whose target does not exist.</summary>
			Dangling,

			/// <summary>A link that eventually points back at itself.</summary>
			Cyclic,

			/// <summary>A real directory that cannot be inspected because its parent denies access.</summary>
			Inaccessible,
		}

		static bool TryCreateBrokenEntry(BrokenLinkKind kind, string root, string outside, out string traversed, out string skip)
		{
			skip = null;
			traversed = Path.Combine(root, "alias");

			switch (kind)
			{
				case BrokenLinkKind.Dangling:
					// The target has to be outside the root: a dangling link that pointed back inside it
					// would resolve to an inside path and be legitimately stale.
					if (!SymbolicLink.TryCreateDirectoryLink(traversed, Path.Combine(outside, "gone"), out var danglingError))
					{
						skip = $"Skipping: symbolic links are not available on this machine: {danglingError}";
						return false;
					}

					return true;

				case BrokenLinkKind.Cyclic:
					var other = Path.Combine(root, "alias-other");
					if (!SymbolicLink.TryCreateDirectoryLink(traversed, other, out var cyclicError) ||
						!SymbolicLink.TryCreateDirectoryLink(other, traversed, out cyclicError))
					{
						skip = $"Skipping: symbolic links are not available on this machine: {cyclicError}";
						return false;
					}

					return true;

				default:
					if (OperatingSystem.IsWindows())
					{
						// Denying read access needs ACL work that does not model the Unix case this covers,
						// and the unidentifiable branch is already exercised by the other two states.
						skip = "Skipping: denying directory access is not modelled on Windows.";
						return false;
					}

					// A directory can only be inspected through a parent that grants access, so the parent
					// is what gets locked.
					var locked = Path.Combine(root, "locked");
					traversed = Path.Combine(locked, "inner");
					Directory.CreateDirectory(traversed);
					File.WriteAllText(Path.Combine(traversed, "precious.png"), "not a build output");

					if (Chmod(locked, 0) != 0)
					{
						skip = "Skipping: could not remove access from the directory.";
						return false;
					}

					return true;
			}
		}

		static void Unlock(BrokenLinkKind kind, string root)
		{
			// Leave the directory removable so the test's own cleanup can run.
			if (kind == BrokenLinkKind.Inaccessible && !OperatingSystem.IsWindows())
				Chmod(Path.Combine(root, "locked"), Convert.ToInt32("755", 8));
		}

		[System.Runtime.InteropServices.DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
		static extern int Chmod(string path, int mode);

		static bool IsCaseSensitiveVolume(string directory)
		{
			var probe = Path.Combine(directory, "CaseProbe");
			Directory.CreateDirectory(probe);

			return !Directory.Exists(Path.Combine(directory, "caseprobe"));
		}

		/// <summary>
		/// Every returned deletion path has to be the resolved-parent form of a member of
		/// <see cref="DetectStaleOutputFilesTask.Files"/>. Nothing may be invented, and nothing that was
		/// declared as an output may be returned.
		/// </summary>
		[Fact]
		public void StaleFilesAreAlwaysDeletionPathsForInputFiles()
		{
			var files = new[]
			{
				Path.Combine(DestinationDirectory, "drawable", "camera.png"),
				Path.Combine(DestinationDirectory, "drawable", "orphan.png"),
				Path.Combine(DestinationDirectory, "drawable-xhdpi", "camera.png"),
			};
			foreach (var file in files)
				WriteFile(file);

			var known = new[]
			{
				files[0],
				// Declared as an output but not on disk, and a sibling that was never enumerated.
				Path.Combine(DestinationDirectory, "drawable-hdpi", "camera.png"),
			};

			var task = GetNewTask(files, known);

			Assert.True(task.Execute());

			var stale = task.StaleFiles.Select(f => f.ItemSpec).ToArray();
			var canonicalizer = new PathCanonicalizer();
			Assert.All(stale, s => Assert.Contains(s, files.Select(canonicalizer.GetDeletionPath)));
			Assert.Equal(new[] { canonicalizer.GetDeletionPath(files[1]), canonicalizer.GetDeletionPath(files[2]) }, stale);
		}

		[Fact]
		public void EmptyAndWhitespaceItemSpecsAreIgnored()
		{
			var stale = Path.Combine(DestinationDirectory, "orphan.png");
			WriteFile(stale);

			var task = GetNewTask(new[] { "", "   ", stale }, new[] { "", "   " });

			Assert.True(task.Execute());
			Assert.Equal(new PathCanonicalizer().GetDeletionPath(stale), Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void CaseOnlyDifferencesFollowThePlatformFileSystem()
		{
			var kept = Path.Combine(DestinationDirectory, "drawable", "camera.png");
			var differentCase = Path.Combine(DestinationDirectory, "drawable", "CAMERA.png");
			WriteFile(kept);
			if (OperatingSystem.IsLinux())
				WriteFile(differentCase);

			var task = GetNewTask(new[] { differentCase }, new[] { kept });

			Assert.True(task.Execute());

			if (OperatingSystem.IsLinux())
			{
				// Linux paths are case sensitive, so these really are two different files.
				Assert.Equal(new PathCanonicalizer().GetDeletionPath(differentCase), Assert.Single(task.StaleFiles).ItemSpec);
			}
			else
			{
				// Windows and macOS are treated case insensitively. On a case sensitive macOS volume this
				// can only ever keep a stale file, which is much safer than deleting a live one, and
				// Resizetizer already rejects output names that differ only by case.
				Assert.Empty(task.StaleFiles);
			}
		}

		[Fact]
		public void MSBuildNormalizesSeparatorsBeforeTheTaskSeesThem()
		{
			var kept = Path.Combine(DestinationDirectory, "drawable", "camera.png");
			WriteFile(kept);

			// MSBuild rewrites separators when it builds an item spec: on Unix a backslash becomes a
			// forward slash, and on Windows both characters are separators anyway. Either way the task
			// only ever receives paths that already use the platform separator, so canonicalization does
			// not have to guess which character was meant.
			var spelledWithBackslashes = kept.Replace(Path.DirectorySeparatorChar, '\\');

			var task = GetNewTask(new[] { spelledWithBackslashes }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}

		static void WriteFile(string path)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, "image");
		}

		bool TryCreateDirectoryAlias(string alias, string target)
		{
			string error;
			bool created;
			if (OperatingSystem.IsWindows())
				created = Junction.TryCreate(alias, target, out error);
			else
				created = SymbolicLink.TryCreateDirectoryLink(alias, target, out error);

			if (!created)
				Output.WriteLine($"Skipping: directory aliases are not available on this machine: {error}");

			return created;
		}
	}
}
