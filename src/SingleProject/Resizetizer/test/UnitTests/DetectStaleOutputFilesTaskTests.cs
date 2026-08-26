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
		public void AlternateDirectorySeparatorsDoNotMakeAFileStale()
		{
			var kept = Path.Combine(DestinationDirectory, "drawable", "camera.png");
			var spelledDifferently = kept.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

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
	}
}
