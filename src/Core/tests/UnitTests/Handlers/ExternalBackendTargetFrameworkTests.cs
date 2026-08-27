using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.Maui.UnitTests.Handlers
{
	/// <summary>
	/// Guards the external-backend test assembly against target-framework drift.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>Core.ExternalBackend</c> only proves what it is compiled against. The failure it guards is
	/// target-framework specific — <c>CS0738</c> on a platform target framework, <c>CS9333</c>/
	/// <c>CS8766</c> on the platform-neutral and netstandard ones — so a target framework that
	/// <c>Microsoft.Maui.Core</c> ships but the guard project does not compile is an untested target
	/// framework, and the gap would be silent.
	/// </para>
	/// <para>
	/// These tests read both project files and compare them directly, so adding a target framework to
	/// Core without adding it here fails the build rather than quietly reducing coverage.
	/// </para>
	/// </remarks>
	[Category(TestCategory.Core)]
	public class ExternalBackendTargetFrameworkTests
	{
		const string CoreProjectPath = "src/Core/src/Core.csproj";
		const string ExternalBackendProjectPath = "src/Core/tests/ExternalBackend/Core.ExternalBackend.csproj";

		[Fact]
		public void ExternalBackendMirrorsCoreTargetFrameworks()
		{
			var core = ReadTargetFrameworks(CoreProjectPath);
			var externalBackend = ReadTargetFrameworks(ExternalBackendProjectPath);

			// Compared as the raw MSBuild expression on purpose. Both sides expand the same
			// $(MauiPlatforms), so keeping the expressions identical is what makes the platform set -
			// including whether Tizen is currently enabled - impossible to drift between the two.
			Assert.Equal(core, externalBackend);
		}

		[Fact]
		public void ExternalBackendCompilesNetStandardTargetFrameworks()
		{
			var externalBackend = ReadTargetFrameworks(ExternalBackendProjectPath);

			Assert.Contains("netstandard2.0", externalBackend, StringComparison.Ordinal);
			Assert.Contains("netstandard2.1", externalBackend, StringComparison.Ordinal);
		}

		[Fact]
		public void ExternalBackendInheritsThePlatformSetFromMauiPlatforms()
		{
			var externalBackend = ReadTargetFrameworks(ExternalBackendProjectPath);

			// Every platform target framework - Android, iOS, Mac Catalyst, Windows and Tizen - reaches
			// both projects through this one property. Tizen is therefore gated purely by
			// IncludeTizenTargetFrameworks, identically for Core and for this guard project.
			Assert.Contains("$(MauiPlatforms)", externalBackend, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData("FakeNativeViews.Standard.cs")]
		[InlineData("FakeNativeViews.Android.cs")]
		[InlineData("FakeNativeViews.iOS.cs")]
		[InlineData("FakeNativeViews.Windows.cs")]
		[InlineData("FakeNativeViews.Tizen.cs")]
		public void EveryPlatformHasFakeNativeViewSources(string fileName)
		{
			// MultiTargeting.targets selects these by file suffix. A platform that is in the target
			// framework list but has no sources would fail to compile the moment it is enabled, so the
			// sources exist for every platform - including Tizen, which is currently gated off by
			// IncludeTizenTargetFrameworks rather than by missing source.
			var path = Path.Combine(RepoRoot, "src", "Core", "tests", "ExternalBackend", fileName);

			Assert.True(File.Exists(path), $"Missing external-backend platform source: {fileName}");

			var source = File.ReadAllText(path);

			foreach (var type in new[] { "FakeNativeView", "FakeNativeLabel", "FakeNativeContentView", "FakeNativeLayoutView" })
			{
				Assert.Contains($"class {type}", source, StringComparison.Ordinal);
			}
		}

		static string ReadTargetFrameworks(string relativeProjectPath)
		{
			var path = Path.Combine(RepoRoot, relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));

			Assert.True(File.Exists(path), $"Could not find project: {relativeProjectPath}");

			var project = XDocument.Load(path);

			// The unconditioned TargetFrameworks element. Core also has a second, conditioned one for
			// IncludePreviousTfms, which is not part of the shipping set.
			var targetFrameworks = project
				.Descendants()
				.Where(e => e.Name.LocalName == "TargetFrameworks" && e.Attribute("Condition") is null)
				.Select(e => e.Value.Trim())
				.ToList();

			Assert.Single(targetFrameworks);

			return targetFrameworks[0];
		}

		static string RepoRoot => FindRepoRoot(GetSourceDirectory());

		static string FindRepoRoot(string start)
		{
			var directory = new DirectoryInfo(start);

			while (directory is not null)
			{
				if (File.Exists(Path.Combine(directory.FullName, "Microsoft.Maui.sln")))
				{
					return directory.FullName;
				}

				directory = directory.Parent;
			}

			throw new InvalidOperationException($"Could not locate the repository root above '{start}'.");
		}

		static string GetSourceDirectory([CallerFilePath] string filePath = "") =>
			Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Could not resolve the test source directory.");
	}
}
