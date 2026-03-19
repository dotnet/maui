using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	/// <summary>
	/// Verifies the MSBuild target structure in Microsoft.Maui.Resizetizer.After.targets
	/// to prevent regression of the "fonts missing on first build" bug (#23268).
	///
	/// Root cause: ProcessMauiFonts used Inputs/Outputs for incremental builds. When
	/// the target was skipped (stamp file up-to-date), platform item groups
	/// (AndroidAsset, BundleResource, etc.) were never populated — causing fonts
	/// to silently disappear from build output.
	/// </summary>
	public class ProcessMauiFontsTargetTests
	{
		static readonly XNamespace MSBuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";

		readonly ITestOutputHelper _output;
		readonly XDocument _targetsDoc;
		readonly string _targetsFilePath;

		public ProcessMauiFontsTargetTests(ITestOutputHelper output)
		{
			_output = output;

			// Navigate from test output dir (artifacts/bin/.../net10.0/) to repo root
			var repoRoot = Path.GetFullPath(Path.Combine(
				Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
			_targetsFilePath = Path.Combine(repoRoot,
				"src", "SingleProject", "Resizetizer", "src", "nuget",
				"buildTransitive", "Microsoft.Maui.Resizetizer.After.targets");

			Assert.True(File.Exists(_targetsFilePath),
				$"Targets file not found at: {_targetsFilePath}");
			_output.WriteLine($"Loading targets from: {_targetsFilePath}");

			_targetsDoc = XDocument.Load(_targetsFilePath);
		}

		XElement FindTarget(string name) =>
			_targetsDoc.Root!
				.Elements(MSBuildNs + "Target")
				.FirstOrDefault(t => t.Attribute("Name")?.Value == name);

		/// <summary>
		/// ProcessMauiFonts must NOT have an Inputs attribute. When Inputs/Outputs are
		/// present, MSBuild skips the target body on incremental builds — meaning the
		/// ItemGroups that register fonts with each platform (AndroidAsset,
		/// BundleResource, ContentWithTargetPath) are never evaluated. The fonts then
		/// silently disappear from the build output.
		/// </summary>
		[Fact]
		public void ProcessMauiFonts_ShouldNotHaveInputsAttribute()
		{
			var target = FindTarget("ProcessMauiFonts");
			Assert.NotNull(target);

			var inputs = target.Attribute("Inputs");
			Assert.True(inputs is null,
				"ProcessMauiFonts must not use Inputs for incremental builds. " +
				"When the target is skipped, platform item groups (AndroidAsset, " +
				"BundleResource, etc.) are never populated, causing fonts to be " +
				"missing from the build. See https://github.com/dotnet/maui/issues/23268");
		}

		/// <summary>
		/// ProcessMauiFonts must NOT have an Outputs attribute (counterpart to Inputs).
		/// </summary>
		[Fact]
		public void ProcessMauiFonts_ShouldNotHaveOutputsAttribute()
		{
			var target = FindTarget("ProcessMauiFonts");
			Assert.NotNull(target);

			var outputs = target.Attribute("Outputs");
			Assert.True(outputs is null,
				"ProcessMauiFonts must not use Outputs for incremental builds. " +
				"See https://github.com/dotnet/maui/issues/23268");
		}

		/// <summary>
		/// On Android, ProcessMauiFonts adds fonts to @(AndroidAsset). The Android SDK's
		/// _ComputeAndroidAssetsPaths target consumes @(AndroidAsset). Without explicit
		/// ordering, MSBuild may run _ComputeAndroidAssetsPaths before ProcessMauiFonts,
		/// causing fonts to be missing from the APK even when the target body executes.
		/// </summary>
		[Fact]
		public void Android_ProcessMauiFontsBeforeTargets_ShouldIncludeComputeAndroidAssetsPaths()
		{
			// Find the Android-only PropertyGroup (not the combined IsCompatibleApp one)
			var androidPG = _targetsDoc.Root!
				.Elements(MSBuildNs + "PropertyGroup")
				.FirstOrDefault(pg =>
				{
					var cond = pg.Attribute("Condition")?.Value;
					return cond != null
						&& cond.Contains("_ResizetizerIsAndroidApp", StringComparison.Ordinal)
						&& !cond.Contains("_ResizetizerIsiOSApp", StringComparison.Ordinal);
				});

			Assert.NotNull(androidPG);

			var beforeTargets = androidPG.Element(MSBuildNs + "ProcessMauiFontsBeforeTargets");
			Assert.NotNull(beforeTargets);
			Assert.Contains("_ComputeAndroidAssetsPaths", beforeTargets.Value, StringComparison.Ordinal);
		}

		/// <summary>
		/// Verifies consistency: ProcessMauiFonts should follow the same pattern as
		/// ProcessMauiAssets (which has always worked). ProcessMauiAssets does NOT use
		/// Inputs/Outputs — confirming this is the correct pattern for targets that
		/// populate platform item groups.
		/// </summary>
		[Fact]
		public void ProcessMauiFonts_MatchesWorkingProcessMauiAssetsPattern()
		{
			// ProcessMauiAssets works correctly and does NOT use Inputs/Outputs
			var assetsTarget = FindTarget("ProcessMauiAssets");
			Assert.NotNull(assetsTarget);
			Assert.Null(assetsTarget.Attribute("Inputs"));
			Assert.Null(assetsTarget.Attribute("Outputs"));

			// ProcessMauiFonts should follow the same pattern
			var fontsTarget = FindTarget("ProcessMauiFonts");
			Assert.NotNull(fontsTarget);
			Assert.Null(fontsTarget.Attribute("Inputs"));
			Assert.Null(fontsTarget.Attribute("Outputs"));
		}
	}
}
