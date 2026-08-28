using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class DeleteStaleOutputFilesTaskTests : MSBuildTaskTestFixture<DeleteStaleOutputFilesTask>
	{
		public DeleteStaleOutputFilesTaskTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Fact]
		public void DeletesOnlyUnexpectedExistingFiles()
		{
			var root = CreateRoot();
			var kept = WriteFile(root, "drawable", "camera.png");
			var stale = WriteFile(root, "drawable", "orphan.png");

			var task = GetNewTask(root, kept);

			Assert.True(task.Execute());
			Assert.True(File.Exists(kept));
			Assert.False(File.Exists(stale));
		}

		[Fact]
		public void MissingFutureKnownOutputsRemainPermissive()
		{
			var root = CreateRoot();
			var stale = WriteFile(root, "drawable", "orphan.png");
			var future = Path.Combine(root, "drawable-xhdpi", "camera.png");

			var task = GetNewTask(root, future);

			Assert.True(task.Execute());
			Assert.False(File.Exists(stale));
			Assert.False(File.Exists(future));
		}

		[Fact]
		public void MissingRootIsANoOp()
		{
			var root = Path.Combine(DestinationDirectory, "missing");
			var task = GetNewTask(root);

			Assert.True(task.Execute());
			Assert.Empty(LogErrorEvents);
		}

		[Fact]
		public void ParentReplacementAfterValidationCannotRedirectDeletion()
		{
			var root = CreateRoot();
			var parent = Path.Combine(root, "inside");
			var movedParent = Path.Combine(root, "inside-old");
			var stale = WriteFile(parent, "orphan.png");
			var outside = Path.Combine(DestinationDirectory, "outside");
			var precious = WriteFile(outside, "orphan.png");

			var task = GetNewTask(root);
			var executed = task.Execute(() =>
			{
				Directory.Move(parent, movedParent);
				Assert.True(TryCreateDirectoryAlias(parent, outside));
			});

			Assert.True(executed);
			Assert.False(File.Exists(Path.Combine(movedParent, Path.GetFileName(stale))));
			Assert.Equal("image", File.ReadAllText(precious));
			Assert.Equal("image", File.ReadAllText(Path.Combine(parent, "orphan.png")));
		}

		[Fact]
		public void RootReplacementAfterValidationCannotRedirectDeletion()
		{
			var root = CreateRoot();
			var movedRoot = root + "-old";
			var stale = WriteFile(root, "drawable", "orphan.png");
			var outside = Path.Combine(DestinationDirectory, "outside-root");
			var precious = WriteFile(outside, "drawable", "orphan.png");

			var task = GetNewTask(root);
			var executed = task.Execute(() =>
			{
				Directory.Move(root, movedRoot);
				Assert.True(TryCreateDirectoryAlias(root, outside));
			});

			Assert.True(executed);
			Assert.False(File.Exists(Path.Combine(movedRoot, "drawable", Path.GetFileName(stale))));
			Assert.Equal("image", File.ReadAllText(precious));
			Assert.Equal("image", File.ReadAllText(Path.Combine(root, "drawable", "orphan.png")));
		}

		[Fact]
		public void RecreatedLeafIsNeverDeleted()
		{
			var root = CreateRoot();
			var stale = WriteFile(root, "drawable", "orphan.png");
			var moved = Path.Combine(Path.GetDirectoryName(stale), "orphan-old.png");

			var task = GetNewTask(root);
			var executed = task.Execute(() =>
			{
				File.Move(stale, moved);
				File.WriteAllText(stale, "replacement");
			});

			Assert.True(executed);
			Assert.Equal("replacement", File.ReadAllText(stale));
		}

		[Fact]
		public void DisappearingLeafIsANoOp()
		{
			var root = CreateRoot();
			var stale = WriteFile(root, "drawable", "orphan.png");

			var task = GetNewTask(root);
			var executed = task.Execute(() => File.Delete(stale));

			Assert.True(executed);
			Assert.False(File.Exists(stale));
		}

		[Fact]
		public void ReplacementLeafLinkIsNeverFollowedOrDeleted()
		{
			var root = CreateRoot();
			var stale = WriteFile(root, "drawable", "orphan.png");
			var moved = Path.Combine(Path.GetDirectoryName(stale), "orphan-old.png");
			var precious = WriteFile(DestinationDirectory, "outside", "precious.png");

			if (!SymbolicLink.TryCreateFileLink(Path.Combine(root, "probe-link"), precious, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}
			File.Delete(Path.Combine(root, "probe-link"));

			var task = GetNewTask(root);
			var executed = task.Execute(() =>
			{
				File.Move(stale, moved);
				Assert.True(SymbolicLink.TryCreateFileLink(stale, precious, out var linkError), linkError);
			});

			Assert.True(executed);
			Assert.True(File.Exists(stale));
			Assert.Equal("image", File.ReadAllText(precious));
		}

		[Fact]
		public void LeafLinkIsDeletedWithoutDeletingItsTarget()
		{
			var root = CreateRoot();
			var outside = Path.Combine(DestinationDirectory, "outside");
			var precious = WriteFile(outside, "precious.png");
			var link = Path.Combine(root, "orphan.png");

			if (!SymbolicLink.TryCreateFileLink(link, precious, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			var task = GetNewTask(root);

			Assert.True(task.Execute());
			Assert.False(File.Exists(link));
			Assert.Equal("image", File.ReadAllText(precious));
		}

		[Fact]
		public void DirectoryAliasesAreNotTraversed()
		{
			var root = CreateRoot();
			var outside = Path.Combine(DestinationDirectory, "outside");
			var precious = WriteFile(outside, "precious.png");
			var alias = Path.Combine(root, "alias");
			var stale = WriteFile(root, "orphan.png");

			if (!TryCreateDirectoryAlias(alias, outside))
				return;

			var task = GetNewTask(root);

			Assert.True(task.Execute());
			Assert.Equal("image", File.ReadAllText(precious));
			Assert.False(File.Exists(stale));
		}

		[Fact]
		public void DirectoriesAreNeverDeleted()
		{
			var root = CreateRoot();
			var directory = Path.Combine(root, "empty");
			Directory.CreateDirectory(directory);

			var task = GetNewTask(root);

			Assert.True(task.Execute());
			Assert.True(Directory.Exists(directory));
		}

		[Fact]
		public void UnicodePathsAreDeleted()
		{
			var root = CreateRoot("r-ü");
			var stale = WriteFile(root, "描画", "orphan.png");

			var task = GetNewTask(root);

			Assert.True(task.Execute());
			Assert.False(File.Exists(stale));
		}

		DeleteStaleOutputFilesTask GetNewTask(string root, params string[] knownOutputs) =>
			new DeleteStaleOutputFilesTask
			{
				Root = root,
				KnownOutputs = knownOutputs.Select(path => (ITaskItem)new TaskItem(path)).ToArray(),
				BuildEngine = this,
			};

		string CreateRoot(string name = "root")
		{
			var root = Path.Combine(DestinationDirectory, name);
			Directory.CreateDirectory(root);
			return root;
		}

		static string WriteFile(string root, params string[] segments)
		{
			var path = segments.Aggregate(root, Path.Combine);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, "image");
			return path;
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
