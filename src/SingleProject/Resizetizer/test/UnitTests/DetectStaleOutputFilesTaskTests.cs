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

		DetectStaleOutputFilesTask GetNewTask(string[] files, string[] knownOutputs) =>
			new DetectStaleOutputFilesTask
			{
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

			var task = GetNewTask(new[] { file }, Array.Empty<string>());

			Assert.True(task.Execute());
			Assert.Equal(file, Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void OnlyUnexpectedFilesAreStale()
		{
			var kept = Path.Combine(DestinationDirectory, "camera.png");
			var stale = Path.Combine(DestinationDirectory, "orphan.png");

			var task = GetNewTask(new[] { kept, stale }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Equal(stale, Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void StaleFilesKeepTheirOriginalItemSpecAndMetadata()
		{
			var stale = Path.Combine(DestinationDirectory, "orphan.png");
			var item = new TaskItem(stale);
			item.SetMetadata("_ResizetizerDpiPath", "drawable-xhdpi");

			var task = new DetectStaleOutputFilesTask
			{
				Files = new ITaskItem[] { item },
				KnownOutputs = Array.Empty<ITaskItem>(),
				BuildEngine = this,
			};

			Assert.True(task.Execute());

			var result = Assert.Single(task.StaleFiles);
			Assert.Equal(stale, result.ItemSpec);
			Assert.Equal("drawable-xhdpi", result.GetMetadata("_ResizetizerDpiPath"));
		}

		[Fact]
		public void RedundantSegmentsDoNotMakeAFileStale()
		{
			var kept = Path.Combine(DestinationDirectory, "camera.png");
			var spelledDifferently = Path.Combine(DestinationDirectory, ".", "obj", "..", "camera.png");

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
			Assert.Equal(stale, Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// The item specs handed back are what <c>&lt;Delete&gt;</c> acts on, so they must be the exact
		/// strings that came in. Returning the canonical spelling instead would let a delete reach
		/// outside the intermediate directory the wildcard was rooted at.
		/// </summary>
		[Fact]
		public void StaleFilesAreReturnedVerbatimEvenWhenCanonicalizationChangesTheSpelling()
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
			Assert.NotEqual(stale, canonicalizer.Canonicalize(stale), PathCanonicalizer.Comparer);

			var task = GetNewTask(new[] { stale }, System.Array.Empty<string>());

			Assert.True(task.Execute());
			Assert.Equal(stale, Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// A file inside the intermediate directory that is itself a link to somewhere else canonicalizes
		/// to a path outside that directory, but it is still reported by its inside spelling, so
		/// <c>&lt;Delete&gt;</c> removes the link and never the file it points at.
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

			var task = GetNewTask(new[] { inside }, System.Array.Empty<string>());

			Assert.True(task.Execute());
			Assert.Equal(inside, Assert.Single(task.StaleFiles).ItemSpec);
		}

		/// <summary>
		/// Every returned item has to come from <see cref="DetectStaleOutputFilesTask.Files"/>. Nothing may
		/// be invented, and nothing that was declared as an output may be returned.
		/// </summary>
		[Fact]
		public void StaleFilesAreAlwaysASubsetOfTheInputFiles()
		{
			var files = new[]
			{
				Path.Combine(DestinationDirectory, "drawable", "camera.png"),
				Path.Combine(DestinationDirectory, "drawable", "orphan.png"),
				Path.Combine(DestinationDirectory, "drawable-xhdpi", "camera.png"),
			};

			var known = new[]
			{
				files[0],
				// Declared as an output but not on disk, and a sibling that was never enumerated.
				Path.Combine(DestinationDirectory, "drawable-hdpi", "camera.png"),
			};

			var task = GetNewTask(files, known);

			Assert.True(task.Execute());

			var stale = task.StaleFiles.Select(f => f.ItemSpec).ToArray();
			Assert.All(stale, s => Assert.Contains(s, files));
			Assert.Equal(new[] { files[1], files[2] }, stale);
		}

		[Fact]
		public void EmptyAndWhitespaceItemSpecsAreIgnored()
		{
			var stale = Path.Combine(DestinationDirectory, "orphan.png");

			var task = GetNewTask(new[] { "", "   ", stale }, new[] { "", "   " });

			Assert.True(task.Execute());
			Assert.Equal(stale, Assert.Single(task.StaleFiles).ItemSpec);
		}

		[Fact]
		public void CaseOnlyDifferencesFollowThePlatformFileSystem()
		{
			var kept = Path.Combine(DestinationDirectory, "drawable", "camera.png");
			var differentCase = Path.Combine(DestinationDirectory, "drawable", "CAMERA.png");

			var task = GetNewTask(new[] { differentCase }, new[] { kept });

			Assert.True(task.Execute());

			if (OperatingSystem.IsLinux())
			{
				// Linux paths are case sensitive, so these really are two different files.
				Assert.Equal(differentCase, Assert.Single(task.StaleFiles).ItemSpec);
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

			// MSBuild rewrites separators when it builds an item spec: on Unix a backslash becomes a
			// forward slash, and on Windows both characters are separators anyway. Either way the task
			// only ever receives paths that already use the platform separator, so canonicalization does
			// not have to guess which character was meant.
			var spelledWithBackslashes = kept.Replace(Path.DirectorySeparatorChar, '\\');

			var task = GetNewTask(new[] { spelledWithBackslashes }, new[] { kept });

			Assert.True(task.Execute());
			Assert.Empty(task.StaleFiles);
		}
	}
}
