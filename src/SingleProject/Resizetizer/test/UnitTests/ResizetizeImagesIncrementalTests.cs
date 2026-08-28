using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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

			var source = Path.Combine(project.ImagesDirectory, ImageName);
			File.Copy(Path.Combine(AppContext.BaseDirectory, "images", "camera_color.png"), source, overwrite: true);
			// File.Copy carries the source timestamp over, but an edit would bump it.
			TouchSourceImage(project);

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
			TouchSourceImage(project);

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

		/// <summary>
		/// The project directory itself is ordinary, but the intermediate output directory that receives
		/// the generated images is reached through a link. This is what a redirected <c>obj</c> looks like.
		/// </summary>
		[Fact]
		public void ImagesSurviveWhenOnlyTheIntermediateOutputIsLinked()
		{
			var project = CreateProject(throughLink: false, linkedIntermediateOutput: true);
			if (project is null)
				return;

			Build(project);
			AssertGeneratedImageExists(project);

			Build(project);
			AssertGeneratedImageExists(project);

			// The images really do land on the far side of the link.
			var real = Path.Combine(project.PhysicalDirectory, "obj-real", "resizetizer", "r", "drawable", ImageName);
			Assert.True(File.Exists(real), $"Expected the generated image behind the link: {real}");
		}

		/// <summary>
		/// The source images are reached through a link while the project and its output are ordinary.
		/// </summary>
		[Fact]
		public void ImagesSurviveWhenOnlyTheInputImagesAreLinked()
		{
			var project = CreateProject(throughLink: false, linkedImages: true);
			if (project is null)
				return;

			Build(project);
			AssertGeneratedImageExists(project);

			Build(project);
			AssertGeneratedImageExists(project);
		}

		/// <summary>
		/// A link that has gone dangling between enumeration and cleanup must not be followed either. The
		/// wildcard still names what it found, and by the time <c>&lt;Delete&gt;</c> runs the link may point
		/// at something real again, so a path through it can never be treated as living inside the folder.
		/// </summary>
		[Fact]
		public void CleanupNeverReachesThroughADanglingDirectoryLink()
		{
			var project = CreateProject(throughLink: false);
			if (project is null)
				return;

			Build(project);
			AssertGeneratedImageExists(project);

			var outside = Path.Combine(project.PhysicalDirectory, "outside");
			Directory.CreateDirectory(outside);
			var precious = Path.Combine(outside, "precious.png");
			File.WriteAllText(precious, "not a build output");

			// Points at a directory that does not exist yet, so the link is dangling right now.
			var future = Path.Combine(project.PhysicalDirectory, "outside-later");
			var alias = Path.Combine(project.PhysicalDirectory, "obj", "resizetizer", "r", "alias");
			if (!SymbolicLink.TryCreateDirectoryLink(alias, future, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			TouchSourceImage(project);

			Build(project);

			AssertGeneratedImageExists(project);
			Assert.True(File.Exists(precious), $"Cleanup deleted a file outside the intermediate folder: {precious}");

			// The link itself is still inside the folder, so removing it is fine; what must not happen is
			// anything being deleted through it.
			Assert.False(Directory.Exists(future), "The dangling link's target should never have been created.");
		}

		/// <summary>
		/// The recursive wildcard that finds stale files walks through a directory link, so it can name a
		/// file that is not really inside the intermediate folder. Deleting that would destroy the real
		/// file rather than the link, so the cleanup has to leave it alone.
		/// </summary>
		[Fact]
		public void CleanupNeverReachesThroughADirectoryLinkOutOfTheIntermediateFolder()
		{
			var project = CreateProject(throughLink: false);
			if (project is null)
				return;

			Build(project);
			AssertGeneratedImageExists(project);

			var outside = Path.Combine(project.PhysicalDirectory, "outside");
			Directory.CreateDirectory(outside);
			var precious = Path.Combine(outside, "precious.png");
			File.WriteAllText(precious, "not a build output");

			var alias = Path.Combine(project.PhysicalDirectory, "obj", "resizetizer", "r", "alias");
			if (!SymbolicLink.TryCreateDirectoryLink(alias, outside, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			// Touch the source image so the target is not skipped as up to date.
			TouchSourceImage(project);

			Build(project);

			AssertGeneratedImageExists(project);
			Assert.True(File.Exists(precious), $"Cleanup deleted a file outside the intermediate folder: {precious}");
		}

		/// <summary>
		/// The stale-file task has already accepted the wildcard item when this hook retargets its directory
		/// alias. Delete must use the resolved parent returned by the task, not traverse the changed alias.
		/// </summary>
		[Fact]
		public void CleanupUsesTheResolvedParentWhenAnAliasIsRetargetedAfterValidation()
		{
			var project = CreateProject(throughLink: false);
			if (project is null)
				return;

			Build(project);

			var root = Path.Combine(project.PhysicalDirectory, "obj", "resizetizer", "r");
			var inside = Path.Combine(root, "inside");
			var outside = Path.Combine(project.PhysicalDirectory, "outside");
			Directory.CreateDirectory(inside);
			Directory.CreateDirectory(outside);

			var stale = Path.Combine(inside, "orphan.png");
			var precious = Path.Combine(outside, "orphan.png");
			File.WriteAllText(stale, "left over");
			File.WriteAllText(precious, "not a build output");

			var alias = Path.Combine(root, "alias");
			if (!TryCreateDirectoryAlias(alias, inside, out var useJunction))
				return;

			TouchSourceImage(project);
			var marker = Path.Combine(project.PhysicalDirectory, "replace-alias.marker");

			Build(project, properties: new Dictionary<string, string>
			{
				["_TestBeforeStaleFileDeletionTargets"] = "_TestReplaceDirectoryAlias",
				["_TestAliasPath"] = alias,
				["_TestAliasTargetPath"] = outside,
				["_TestReplaceAliasMarker"] = marker,
				["_TestUseJunction"] = useJunction ? "true" : "false",
			});

			Assert.True(File.Exists(marker), "The before-deletion mutation hook did not run.");
			Assert.False(File.Exists(stale), $"Expected the stale file at the resolved inside path to be deleted: {stale}");
			Assert.True(File.Exists(precious), $"Cleanup followed the retargeted alias outside the intermediate folder: {precious}");
			Assert.Equal("not a build output", File.ReadAllText(Path.Combine(alias, "orphan.png")));
		}

		/// <summary>
		/// This exercises both race windows. The wildcard first enumerates <c>alias/orphan.png</c> while
		/// the alias points inside. The first hook removes the alias before validation; the second recreates
		/// it pointing outside before Delete. A missing component in an enumerated candidate must therefore
		/// be rejected, while the independently enumerated physical stale file is still cleaned.
		/// </summary>
		[Fact]
		public void CleanupRejectsAnEnumeratedCandidateWhoseAliasDisappearsBeforeValidation()
		{
			var project = CreateProject(throughLink: false);
			if (project is null)
				return;

			Build(project);

			var root = Path.Combine(project.PhysicalDirectory, "obj", "resizetizer", "r");
			var inside = Path.Combine(root, "inside");
			var outside = Path.Combine(project.PhysicalDirectory, "outside");
			Directory.CreateDirectory(inside);
			Directory.CreateDirectory(outside);

			var stale = Path.Combine(inside, "orphan.png");
			var precious = Path.Combine(outside, "orphan.png");
			File.WriteAllText(stale, "left over");
			File.WriteAllText(precious, "not a build output");

			var alias = Path.Combine(root, "alias");
			if (!TryCreateDirectoryAlias(alias, inside, out var useJunction))
				return;

			TouchSourceImage(project);
			var removeMarker = Path.Combine(project.PhysicalDirectory, "remove-alias.marker");
			var replaceMarker = Path.Combine(project.PhysicalDirectory, "replace-alias.marker");

			Build(project, properties: new Dictionary<string, string>
			{
				["_TestAfterStaleFileEnumerationTargets"] = "_TestRemoveDirectoryAlias",
				["_TestBeforeStaleFileDeletionTargets"] = "_TestReplaceDirectoryAlias",
				["_TestAliasPath"] = alias,
				["_TestAliasTargetPath"] = outside,
				["_TestRemoveAliasMarker"] = removeMarker,
				["_TestReplaceAliasMarker"] = replaceMarker,
				["_TestUseJunction"] = useJunction ? "true" : "false",
			});

			Assert.True(File.Exists(removeMarker), "The after-enumeration mutation hook did not run.");
			Assert.True(File.Exists(replaceMarker), "The before-deletion mutation hook did not run.");
			Assert.False(File.Exists(stale), $"Expected the ordinary stale file to still be deleted: {stale}");
			Assert.True(File.Exists(precious), $"Cleanup accepted the missing alias and later followed it outside: {precious}");
			Assert.Equal("not a build output", File.ReadAllText(Path.Combine(alias, "orphan.png")));
		}

		/// <summary>
		/// The root is canonicalized before the wildcard snapshot. Retargeting the linked intermediate
		/// directory after enumeration must not let both Root and the candidate move together into an
		/// external tree and pass containment there.
		/// </summary>
		[Fact]
		public void CleanupKeepsTheOriginalRootWhenTheIntermediateAliasIsRetargeted()
		{
			var project = CreateProject(throughLink: false, linkedIntermediateOutput: true);
			if (project is null)
				return;

			Build(project);

			var stale = Path.Combine(project.PhysicalDirectory, "obj-real", "resizetizer", "r", "drawable", "orphan.png");
			File.WriteAllText(stale, "left over");

			var outsideIntermediate = Path.Combine(project.PhysicalDirectory, "outside-obj");
			var outsideRoot = Path.Combine(outsideIntermediate, "resizetizer", "r", "drawable");
			Directory.CreateDirectory(outsideRoot);
			var precious = Path.Combine(outsideRoot, "orphan.png");
			File.WriteAllText(precious, "not a build output");

			TouchSourceImage(project);
			var marker = Path.Combine(project.PhysicalDirectory, "replace-root.marker");

			Build(project, properties: new Dictionary<string, string>
			{
				["_TestAfterStaleFileEnumerationTargets"] = "_TestReplaceDirectoryAlias",
				["_TestAliasPath"] = Path.Combine(project.PhysicalDirectory, "obj"),
				["_TestAliasTargetPath"] = outsideIntermediate,
				["_TestReplaceAliasMarker"] = marker,
				["_TestUseJunction"] = OperatingSystem.IsWindows() ? "true" : "false",
			});

			Assert.True(File.Exists(marker), "The after-enumeration root mutation hook did not run.");
			Assert.True(File.Exists(precious), $"Cleanup followed the retargeted intermediate root: {precious}");
			Assert.True(File.Exists(stale), "The candidate from the old root should be kept when its lexical root moves.");
		}

		/// <summary>
		/// Canonicalizing the parent must not canonicalize the leaf. Delete should remove a stale leaf link
		/// inside the intermediate folder and leave the file it points at untouched.
		/// </summary>
		[Fact]
		public void CleanupDeletesALeafLinkWithoutDeletingItsTarget()
		{
			var project = CreateProject(throughLink: false);
			if (project is null)
				return;

			Build(project);

			var outside = Path.Combine(project.PhysicalDirectory, "outside");
			Directory.CreateDirectory(outside);
			var precious = Path.Combine(outside, "precious.png");
			File.WriteAllText(precious, "not a build output");

			var alias = Path.Combine(project.PhysicalDirectory, "obj", "resizetizer", "r", "drawable", "orphan.png");
			if (!SymbolicLink.TryCreateFileLink(alias, precious, out var error))
			{
				Output.WriteLine($"Skipping: symbolic links are not available on this machine: {error}");
				return;
			}

			TouchSourceImage(project);
			Build(project);

			Assert.False(File.Exists(alias), $"Expected the stale leaf link to be deleted: {alias}");
			Assert.True(File.Exists(precious), $"Cleanup deleted the leaf link's target: {precious}");
		}

		void AssertGeneratedImageExists(TestProject project)
		{
			var image = Path.Combine(project.PhysicalDirectory, GeneratedImage);
			Assert.True(File.Exists(image), $"Expected the generated image to survive the build: {image}");
		}

		/// <summary>
		/// Marks the source image as edited so that the next build cannot treat the target as up to date.
		/// </summary>
		/// <remarks>
		/// The new timestamp has to be definitively newer than everything the previous build wrote, not
		/// merely "now". Only about a hundred milliseconds pass between the build writing its stamp file
		/// and this call, so a file system that stores timestamps to the nearest second or two (ext3,
		/// HFS+, FAT, some container overlays) rounds both into the same value. MSBuild treats an input
		/// that is not strictly newer than its outputs as up to date and skips the target, which would
		/// make these tests fail for a reason that has nothing to do with what they cover.
		/// </remarks>
		static void TouchSourceImage(TestProject project)
		{
			var newest = DateTime.UtcNow;

			var intermediate = Path.Combine(project.PhysicalDirectory, "obj");
			if (Directory.Exists(intermediate))
			{
				foreach (var written in Directory.EnumerateFiles(intermediate, "*", SearchOption.AllDirectories))
				{
					var time = File.GetLastWriteTimeUtc(written);
					if (time > newest)
						newest = time;
				}
			}

			// Five seconds clears the two second granularity of the coarsest file systems.
			File.SetLastWriteTimeUtc(Path.Combine(project.ImagesDirectory, ImageName), newest.AddSeconds(5));
		}


		TestProject CreateProject(bool throughLink, bool linkedIntermediateOutput = false, bool linkedImages = false)
		{
			var physical = Path.Combine(DestinationDirectory, "project");
			Directory.CreateDirectory(physical);

			var imagesDirectory = Path.Combine(physical, "images");

			if (linkedImages)
			{
				// The project refers to images\, but that is a link to where the files really live.
				var realImages = Path.Combine(physical, "images-real");
				Directory.CreateDirectory(realImages);

				if (!SymbolicLink.TryCreateDirectoryLink(imagesDirectory, realImages, out var imagesError))
				{
					Output.WriteLine($"Skipping: symbolic links are not available on this machine: {imagesError}");
					return null;
				}

				imagesDirectory = realImages;
			}
			else
			{
				Directory.CreateDirectory(imagesDirectory);
			}

			File.Copy(
				Path.Combine(AppContext.BaseDirectory, "images", ImageName),
				Path.Combine(imagesDirectory, ImageName));

			if (linkedIntermediateOutput)
			{
				// The project writes to obj\, but that is a link to where the outputs really land.
				var realIntermediate = Path.Combine(physical, "obj-real");
				Directory.CreateDirectory(realIntermediate);

				if (!TryCreateDirectoryAlias(Path.Combine(physical, "obj"), realIntermediate, out _))
					return null;
			}

			var sourceTargets = Path.Combine(AppContext.BaseDirectory, "Microsoft.Maui.Resizetizer.After.targets");
			Assert.True(File.Exists(sourceTargets), $"Expected the target file to be copied to the test output: {sourceTargets}");
			var targets = Path.Combine(physical, "Microsoft.Maui.Resizetizer.After.targets");
			CreateInstrumentedTargets(sourceTargets, targets);
			var mutationTaskAssembly = System.Security.SecurityElement.Escape(typeof(DirectoryAliasMutationTask).Assembly.Location);

			File.WriteAllText(Path.Combine(physical, "App.proj"),
				$"""
				<Project>
				  <UsingTask
				      TaskName="Microsoft.Maui.Resizetizer.Tests.DirectoryAliasMutationTask"
				      AssemblyFile="{mutationTaskAssembly}" />
				  <PropertyGroup>
				    <ResizetizerPlatformType>android</ResizetizerPlatformType>
				    <IntermediateOutputPath>obj\</IntermediateOutputPath>
				  </PropertyGroup>
				  <ItemGroup>
				    <MauiImage Include="images\{ImageName}" />
				  </ItemGroup>
				  <Target Name="_TestRemoveDirectoryAlias">
				    <DirectoryAliasMutationTask
				        AliasPath="$(_TestAliasPath)"
				        MarkerFile="$(_TestRemoveAliasMarker)" />
				  </Target>
				  <Target Name="_TestReplaceDirectoryAlias">
				    <DirectoryAliasMutationTask
				        AliasPath="$(_TestAliasPath)"
				        TargetPath="$(_TestAliasTargetPath)"
				        MarkerFile="$(_TestReplaceAliasMarker)"
				        UseJunction="$(_TestUseJunction)" />
				  </Target>
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

			return new TestProject(physical, entry, imagesDirectory);
		}

		static void CreateInstrumentedTargets(string source, string destination)
		{
			var document = XDocument.Load(source);
			var ns = document.Root.Name.Namespace;
			Assert.Single(document.Descendants(ns + "_ResizetizerTaskAssemblyName")).Value =
				Path.Combine(Path.GetDirectoryName(source), "Microsoft.Maui.Resizetizer.dll");
			var target = Assert.Single(
				document.Root.Elements(ns + "Target"),
				element => string.Equals((string)element.Attribute("Name"), "ResizetizeImages", StringComparison.Ordinal));
			var detection = Assert.Single(target.Elements(ns + "DetectStaleOutputFilesTask"));

			detection.AddBeforeSelf(
				new XElement(
					ns + "CallTarget",
					new XAttribute("Condition", "'$(_TestAfterStaleFileEnumerationTargets)' != ''"),
					new XAttribute("Targets", "$(_TestAfterStaleFileEnumerationTargets)")));
			detection.AddAfterSelf(
				new XElement(
					ns + "CallTarget",
					new XAttribute("Condition", "'$(_TestBeforeStaleFileDeletionTargets)' != ''"),
					new XAttribute("Targets", "$(_TestBeforeStaleFileDeletionTargets)")));

			document.Save(destination);
		}

		bool TryCreateDirectoryAlias(string alias, string target, out bool useJunction)
		{
			useJunction = OperatingSystem.IsWindows();

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

		string Build(TestProject project, string target = "ResizetizeImages", IReadOnlyDictionary<string, string> properties = null)
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

			foreach (var property in properties ?? new Dictionary<string, string>())
				startInfo.ArgumentList.Add($"-p:{property.Key}={property.Value}");

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

		sealed record TestProject(string PhysicalDirectory, string EntryDirectory, string ImagesDirectory);
	}
}
