using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	/// <summary>
	/// Drives the real <c>ResizetizeImages</c> target through MSBuild so the interaction between the
	/// task outputs, the stale-file wildcard and the incremental stamp files is covered end to end.
	/// </summary>
	/// <remarks>
	/// The same scenarios run twice: once from an ordinary directory and once from a directory reached
	/// through a symbolic link. The second shape is what a macOS temp path (<c>/tmp</c>,
	/// <c>/var/folders</c>) or a Windows junction looks like, and it used to make the cleanup step
	/// delete every image the task had just generated.
	/// </remarks>
	public class ResizetizeImagesIncrementalTests : BaseTest
	{
		const string ImageName = "camera.png";
		const string GeneratedImage = "obj/resizetizer/r/drawable/camera.png";

		public ResizetizeImagesIncrementalTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ImagesSurviveTheFirstBuild(bool throughLink)
		{
			var project = CreateProject(throughLink);
			if (project is null)
				return;

			Build(project);

			AssertGeneratedImageExists(project);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ImagesSurviveASecondBuildWithNoChanges(bool throughLink)
		{
			var project = CreateProject(throughLink);
			if (project is null)
				return;

			Build(project);
			var output = Build(project);

			AssertGeneratedImageExists(project);
			Assert.Contains("Skipping target \"ResizetizeImages\"", output, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ImagesAreRegeneratedWhenTheSourceImageChanges(bool throughLink)
		{
			var project = CreateProject(throughLink);
			if (project is null)
				return;

			Build(project);

			var generated = Path.Combine(project.PhysicalDirectory, GeneratedImage);
			var before = File.ReadAllBytes(generated);

			var source = Path.Combine(project.PhysicalDirectory, "images", ImageName);
			File.Copy(Path.Combine(AppContext.BaseDirectory, "images", "camera_color.png"), source, overwrite: true);
			// File.Copy carries the source timestamp over, but an edit would bump it.
			File.SetLastWriteTimeUtc(source, DateTime.UtcNow);

			var output = Build(project);

			AssertGeneratedImageExists(project);
			Assert.DoesNotContain("Skipping target \"ResizetizeImages\"", output, StringComparison.Ordinal);
			Assert.False(before.SequenceEqual(File.ReadAllBytes(generated)), "Expected the generated image to be rewritten.");
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DeletedImagesAreRestored(bool throughLink)
		{
			var project = CreateProject(throughLink);
			if (project is null)
				return;

			Build(project);

			File.Delete(Path.Combine(project.PhysicalDirectory, GeneratedImage));

			Build(project);

			AssertGeneratedImageExists(project);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void StaleImagesAreStillDeleted(bool throughLink)
		{
			var project = CreateProject(throughLink);
			if (project is null)
				return;

			Build(project);

			var orphan = Path.Combine(project.PhysicalDirectory, "obj", "resizetizer", "r", "drawable", "orphan.png");
			File.WriteAllText(orphan, "left over from an image that is no longer in the project");

			// Touch the source image so the target is not skipped as up to date.
			File.SetLastWriteTimeUtc(Path.Combine(project.PhysicalDirectory, "images", ImageName), DateTime.UtcNow);

			Build(project);

			AssertGeneratedImageExists(project);
			Assert.False(File.Exists(orphan), $"Expected the stale file to be deleted: {orphan}");
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void CleanRemovesImagesAndTheNextBuildRegeneratesThem(bool throughLink)
		{
			var project = CreateProject(throughLink);
			if (project is null)
				return;

			Build(project);

			Build(project, "_CleanResizetizer");
			Assert.False(
				File.Exists(Path.Combine(project.PhysicalDirectory, GeneratedImage)),
				"Expected the clean target to remove the generated image.");

			Build(project);

			AssertGeneratedImageExists(project);
		}

		void AssertGeneratedImageExists(TestProject project)
		{
			var image = Path.Combine(project.PhysicalDirectory, GeneratedImage);
			Assert.True(File.Exists(image), $"Expected the generated image to survive the build: {image}");
		}

		TestProject CreateProject(bool throughLink)
		{
			var physical = Path.Combine(DestinationDirectory, "project");
			Directory.CreateDirectory(Path.Combine(physical, "images"));

			File.Copy(
				Path.Combine(AppContext.BaseDirectory, "images", ImageName),
				Path.Combine(physical, "images", ImageName));

			var targets = Path.Combine(AppContext.BaseDirectory, "Microsoft.Maui.Resizetizer.After.targets");
			Assert.True(File.Exists(targets), $"Expected the target file to be copied to the test output: {targets}");

			File.WriteAllText(Path.Combine(physical, "App.proj"),
				$"""
				<Project>
				  <PropertyGroup>
				    <ResizetizerPlatformType>android</ResizetizerPlatformType>
				    <IntermediateOutputPath>obj\</IntermediateOutputPath>
				  </PropertyGroup>
				  <ItemGroup>
				    <MauiImage Include="images\{ImageName}" />
				  </ItemGroup>
				  <Import Project="{targets}" />
				</Project>
				""");

			var entry = physical;

			if (throughLink)
			{
				entry = Path.Combine(DestinationDirectory, "link");

				if (!SymbolicLink.TryCreateDirectoryLink(entry, physical, out var error))
				{
					Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
					return null;
				}
			}

			return new TestProject(physical, entry);
		}

		string Build(TestProject project, string target = "ResizetizeImages")
		{
			const int timeoutMilliseconds = 300_000;

			var startInfo = new ProcessStartInfo
			{
				FileName = GetDotNetHost(),
				WorkingDirectory = project.EntryDirectory,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			startInfo.ArgumentList.Add("msbuild");
			startInfo.ArgumentList.Add(Path.Combine(project.EntryDirectory, "App.proj"));
			startInfo.ArgumentList.Add($"-t:{target}");
			startInfo.ArgumentList.Add("-nologo");
			startInfo.ArgumentList.Add("-v:normal");
			// Node reuse would keep MSBuild processes alive between the builds of a single test.
			startInfo.ArgumentList.Add("-nodeReuse:false");

			using var process = Process.Start(startInfo);
			Assert.NotNull(process);

			var outputTask = process.StandardOutput.ReadToEndAsync();
			var errorTask = process.StandardError.ReadToEndAsync();

			if (!process.WaitForExit(timeoutMilliseconds))
			{
				process.Kill(entireProcessTree: true);
				process.WaitForExit();
				Assert.Fail($"MSBuild timed out after {timeoutMilliseconds}ms.\n{outputTask.GetAwaiter().GetResult()}{errorTask.GetAwaiter().GetResult()}");
			}

			var output = outputTask.GetAwaiter().GetResult();
			var error = errorTask.GetAwaiter().GetResult();

			Output.WriteLine(output);
			Output.WriteLine(error);

			Assert.Equal(0, process.ExitCode);

			return output + error;
		}

		static string GetDotNetHost()
		{
			var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

			return !string.IsNullOrWhiteSpace(dotnetHost) && File.Exists(dotnetHost)
				? dotnetHost
				: "dotnet";
		}

		sealed record TestProject(string PhysicalDirectory, string EntryDirectory);
	}
}
