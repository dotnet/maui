using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
	/// The inputs are embedded into this assembly at build time (see the <c>EmbeddedResource</c> items in
	/// Core.UnitTests.csproj) rather than read from the working tree, because unit tests run on Helix
	/// from a payload directory with no repository on disk. Embedding still captures the live files on
	/// every build, so real drift is caught, and the assertion is hermetic wherever it runs.
	/// </para>
	/// </remarks>
	[Category(TestCategory.Core)]
	public class ExternalBackendTargetFrameworkTests
	{
		const string CoreProject = "ProjectFiles/Core.csproj";
		const string ExternalBackendProject = "ProjectFiles/Core.ExternalBackend.csproj";

		[Fact]
		public void ExternalBackendMirrorsCoreTargetFrameworks()
		{
			var core = ReadTargetFrameworks(CoreProject);
			var externalBackend = ReadTargetFrameworks(ExternalBackendProject);

			// Compared as the raw MSBuild expression on purpose. Both sides expand the same
			// $(MauiPlatforms), so keeping the expressions identical is what makes the platform set —
			// including whether Tizen is currently enabled — impossible to drift between the two.
			Assert.Equal(core, externalBackend);
		}

		[Fact]
		public void ExternalBackendCompilesNetStandardTargetFrameworks()
		{
			var externalBackend = ReadTargetFrameworks(ExternalBackendProject);

			Assert.Contains("netstandard2.0", externalBackend, StringComparison.Ordinal);
			Assert.Contains("netstandard2.1", externalBackend, StringComparison.Ordinal);
		}

		[Fact]
		public void ExternalBackendInheritsThePlatformSetFromMauiPlatforms()
		{
			var externalBackend = ReadTargetFrameworks(ExternalBackendProject);

			// Every platform target framework — Android, iOS, Mac Catalyst, Windows and Tizen — reaches
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
			// sources exist for every platform — including Tizen, which is currently gated off by
			// IncludeTizenTargetFrameworks rather than by missing source.
			var source = ReadEmbeddedText($"ProjectFiles/{fileName}");

			foreach (var type in new[] { "FakeNativeView", "FakeNativeLabel", "FakeNativeContentView", "FakeNativeLayoutView" })
			{
				Assert.Contains($"class {type}", source, StringComparison.Ordinal);
			}
		}

		static string ReadTargetFrameworks(string resourceName)
		{
			var project = XDocument.Parse(ReadEmbeddedText(resourceName));

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

		static string ReadEmbeddedText(string resourceName)
		{
			var assembly = typeof(ExternalBackendTargetFrameworkTests).Assembly;

			using var stream = assembly.GetManifestResourceStream(resourceName);

			Assert.True(
				stream is not null,
				$"Missing embedded resource '{resourceName}'. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

			using var reader = new StreamReader(stream!);

			return reader.ReadToEnd();
		}
	}
}
