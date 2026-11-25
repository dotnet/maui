using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	/// <summary>
	/// Under multithreaded MSBuild the process working directory is not the project directory, so tasks
	/// must resolve relative paths and read environment variables through their <see cref="TaskEnvironment"/>.
	/// </summary>
	public class MultiThreadedTaskTests : MSBuildTaskTestFixture<ResizetizeImages>
	{
		readonly string projectDirectory;

		public MultiThreadedTaskTests(ITestOutputHelper output)
			: base(output)
		{
			projectDirectory = Path.Combine(DestinationDirectory, "project");
			Directory.CreateDirectory(projectDirectory);
			Assert.NotEqual(Path.GetFullPath(projectDirectory), Directory.GetCurrentDirectory());
		}

		TaskEnvironment CreateEnvironment(Dictionary<string, string> variables = null) =>
			TaskEnvironment.CreateWithProjectDirectoryAndEnvironment(projectDirectory, variables ?? new Dictionary<string, string>());

		[Fact]
		public void ResizetizeImagesWritesRelativeOutputPathUnderProjectDirectory()
		{
			var task = new ResizetizeImages
			{
				PlatformType = "android",
				IntermediateOutputPath = "obj",
				Images = new[] { new TaskItem(Path.GetFullPath("images/camera.svg"), new Dictionary<string, string> { ["Resize"] = "true" }) },
				BuildEngine = this,
				TaskEnvironment = CreateEnvironment(),
			};

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);

			Assert.True(File.Exists(Path.Combine(projectDirectory, "obj", "drawable-mdpi", "camera.png")));
			Assert.All(task.CopiedResources, r => Assert.StartsWith(Path.GetFullPath(projectDirectory), r.ItemSpec, System.StringComparison.Ordinal));
		}

		[Fact]
		public void DeleteStaleOutputFilesResolvesRelativeRootAgainstProjectDirectory()
		{
			var root = Path.Combine(projectDirectory, "obj");
			Directory.CreateDirectory(root);
			var kept = Path.Combine(root, "kept.png");
			var stale = Path.Combine(root, "stale.png");
			File.WriteAllText(kept, "");
			File.WriteAllText(stale, "");

			var task = new DeleteStaleOutputFilesTask
			{
				Root = "obj",
				KnownOutputs = new ITaskItem[] { new TaskItem(Path.Combine("obj", "kept.png")) },
				BuildEngine = this,
				TaskEnvironment = CreateEnvironment(),
			};

			Assert.True(task.Execute(), LogErrorEvents.FirstOrDefault()?.Message);
			Assert.True(File.Exists(kept));
			Assert.False(File.Exists(stale));
		}

		[Fact]
		public void FileRetryPolicyReadsTaskEnvironmentVariables()
		{
			var policy = FileRetryPolicy.FromEnvironment(CreateEnvironment(new Dictionary<string, string>
			{
				[FileRetryPolicy.AttemptsVariable] = "3",
				[FileRetryPolicy.DelayVariable] = "5",
			}));

			Assert.Equal(3, policy.Attempts);
			Assert.Equal(5, policy.DelayMilliseconds);
		}

		[Fact]
		public void FileRetryPolicyFallsBackToDefaultsForInvalidValues()
		{
			var policy = FileRetryPolicy.FromEnvironment(CreateEnvironment(new Dictionary<string, string>
			{
				[FileRetryPolicy.AttemptsVariable] = "many",
			}));

			Assert.Equal(FileRetryPolicy.Default.Attempts, policy.Attempts);
			Assert.Equal(FileRetryPolicy.Default.DelayMilliseconds, policy.DelayMilliseconds);
		}
	}
}
