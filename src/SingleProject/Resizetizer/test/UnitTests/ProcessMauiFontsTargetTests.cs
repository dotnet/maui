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
	/// to prevent regression of the "fonts missing on first build" bug (#23268) and
	/// the "splash screens randomly missing" bug (#33092).
	///
	/// Root cause: ProcessMauiFonts and ProcessMauiSplashScreens used Inputs/Outputs
	/// for incremental builds. When the target was skipped (stamp file up-to-date),
	/// platform item groups (AndroidAsset, BundleResource, etc.) were never populated
	/// — causing fonts/splash screens to silently disappear from build output.
	/// </summary>
	public class ResizetizeTargetStructureTests
	{
		static readonly XNamespace MSBuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";

		readonly ITestOutputHelper _output;
		readonly XDocument _targetsDoc;

		public ResizetizeTargetStructureTests(ITestOutputHelper output)
		{
			_output = output;

			// Navigate from test output dir (artifacts/bin/.../net10.0/) to repo root
			var repoRoot = Path.GetFullPath(Path.Combine(
				Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
			var targetsFilePath = Path.Combine(repoRoot,
				"src", "SingleProject", "Resizetizer", "src", "nuget",
				"buildTransitive", "Microsoft.Maui.Resizetizer.After.targets");

			Assert.True(File.Exists(targetsFilePath),
				$"Targets file not found at: {targetsFilePath}");
			_output.WriteLine($"Loading targets from: {targetsFilePath}");

			_targetsDoc = XDocument.Load(targetsFilePath);
		}

		XElement FindTarget(string name) =>
			_targetsDoc.Root!
				.Elements(MSBuildNs + "Target")
				.FirstOrDefault(t => t.Attribute("Name")?.Value == name);

		XElement FindAndroidPropertyGroup() =>
			_targetsDoc.Root!
				.Elements(MSBuildNs + "PropertyGroup")
				.FirstOrDefault(pg =>
				{
					var cond = pg.Attribute("Condition")?.Value;
					return cond != null
						&& cond.Contains("_ResizetizerIsAndroidApp", StringComparison.Ordinal)
						&& !cond.Contains("_ResizetizerIsiOSApp", StringComparison.Ordinal);
				});

		// ──────────────────────────────────────────────────────────
		//  ProcessMauiFonts — #23268
		// ──────────────────────────────────────────────────────────

		[Fact]
		public void ProcessMauiFonts_ShouldNotHaveInputsAttribute()
		{
			var target = FindTarget("ProcessMauiFonts");
			Assert.NotNull(target);
			Assert.True(target.Attribute("Inputs") is null,
				"ProcessMauiFonts must not use Inputs for incremental builds. " +
				"See https://github.com/dotnet/maui/issues/23268");
		}

		[Fact]
		public void ProcessMauiFonts_ShouldNotHaveOutputsAttribute()
		{
			var target = FindTarget("ProcessMauiFonts");
			Assert.NotNull(target);
			Assert.True(target.Attribute("Outputs") is null,
				"ProcessMauiFonts must not use Outputs for incremental builds. " +
				"See https://github.com/dotnet/maui/issues/23268");
		}

		[Fact]
		public void Android_ProcessMauiFontsBeforeTargets_ShouldIncludeComputeAndroidAssetsPaths()
		{
			var androidPG = FindAndroidPropertyGroup();
			Assert.NotNull(androidPG);

			var beforeTargets = androidPG.Element(MSBuildNs + "ProcessMauiFontsBeforeTargets");
			Assert.NotNull(beforeTargets);
			Assert.Contains("_ComputeAndroidAssetsPaths", beforeTargets.Value, StringComparison.Ordinal);
		}

		[Fact]
		public void ProcessMauiFonts_MatchesWorkingProcessMauiAssetsPattern()
		{
			var assetsTarget = FindTarget("ProcessMauiAssets");
			Assert.NotNull(assetsTarget);
			Assert.Null(assetsTarget.Attribute("Inputs"));
			Assert.Null(assetsTarget.Attribute("Outputs"));

			var fontsTarget = FindTarget("ProcessMauiFonts");
			Assert.NotNull(fontsTarget);
			Assert.Null(fontsTarget.Attribute("Inputs"));
			Assert.Null(fontsTarget.Attribute("Outputs"));
		}

		// ──────────────────────────────────────────────────────────
		//  ProcessMauiSplashScreens — #33092
		// ──────────────────────────────────────────────────────────

		/// <summary>
		/// ProcessMauiSplashScreens must NOT have Inputs. Same root cause as fonts:
		/// when skipped, platform item groups (LibraryResourceDirectories,
		/// BundleResource, ContentWithTargetPath) may not be populated, causing
		/// splash screens to randomly disappear from builds.
		/// The custom tasks (GenerateSplashAndroidResources, etc.) have built-in
		/// file-level incrementality via Resizer.IsUpToDate(), so removing
		/// Inputs/Outputs does not cause expensive re-rendering.
		/// </summary>
		[Fact]
		public void ProcessMauiSplashScreens_ShouldNotHaveInputsAttribute()
		{
			var target = FindTarget("ProcessMauiSplashScreens");
			Assert.NotNull(target);
			Assert.True(target.Attribute("Inputs") is null,
				"ProcessMauiSplashScreens must not use Inputs for incremental builds. " +
				"See https://github.com/dotnet/maui/issues/33092");
		}

		[Fact]
		public void ProcessMauiSplashScreens_ShouldNotHaveOutputsAttribute()
		{
			var target = FindTarget("ProcessMauiSplashScreens");
			Assert.NotNull(target);
			Assert.True(target.Attribute("Outputs") is null,
				"ProcessMauiSplashScreens must not use Outputs for incremental builds. " +
				"See https://github.com/dotnet/maui/issues/33092");
		}

		/// <summary>
		/// All three content-producing targets should follow the same pattern
		/// as ProcessMauiAssets (no Inputs/Outputs).
		/// </summary>
		[Fact]
		public void AllContentTargets_FollowProcessMauiAssetsPattern()
		{
			foreach (var targetName in new[] { "ProcessMauiAssets", "ProcessMauiFonts", "ProcessMauiSplashScreens" })
			{
				var target = FindTarget(targetName);
				Assert.NotNull(target);
				Assert.True(target.Attribute("Inputs") is null,
					$"{targetName} must not use Inputs. See #23268 / #33092");
				Assert.True(target.Attribute("Outputs") is null,
					$"{targetName} must not use Outputs. See #23268 / #33092");
			}
		}
	}
}
