using ImageMagick;
using Xunit;

namespace Microsoft.Maui.UiEvidence;

[CollectionDefinition("UI evidence current directory", DisableParallelization = true)]
public class UiEvidenceCurrentDirectoryCollection;

[Collection("UI evidence current directory")]
public sealed class UiEvidenceCompareCommandTests : IDisposable
{
	readonly string _root = Path.Combine(
		Path.GetTempPath(),
		"maui-ui-evidence-compare-tests-" + Guid.NewGuid().ToString("N"));

	public UiEvidenceCompareCommandTests()
	{
		Directory.CreateDirectory(_root);
	}

	[Fact]
	public void Execute_StableEqualRuns_ReportsNoDifference()
	{
		var paths = CreateFixture(headColor: MagickColors.White);

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("no-difference-observed", summary.Verdict);
		Assert.True(summary.EvidenceComplete);
	}

	[Fact]
	public void Execute_StableChangedHead_ReportsVisualChange()
	{
		var paths = CreateFixture(headColor: MagickColors.Black);

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("visual-change-advisory", summary.Verdict);
		Assert.Equal("change-detected", summary.CheckpointComparisons[0].Status);
		Assert.NotNull(summary.CheckpointComparisons[0].DiffPath);
	}

	[Fact]
	public void Execute_HeadAssertionFailure_ReportsFunctionalFailure()
	{
		var paths = CreateFixture(headColor: MagickColors.White, headStatus: "scenario-failed");

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("head-functional-failure-advisory", summary.Verdict);
	}

	[Fact]
	public void Execute_StableNewLayoutFinding_ReportsLayoutChange()
	{
		var paths = CreateFixture(
			headColor: MagickColors.White,
			headFindings: ["layout.visible-zero-area|Button|UiEvidencePrimaryButton"]);

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("layout-change-advisory", summary.Verdict);
		Assert.Single(summary.NewLayoutFindingKeys);
	}

	[Fact]
	public void Execute_WindowsStableEqualRuns_RemainsInconclusive()
	{
		var paths = CreateFixture(headColor: MagickColors.White, platform: "windows");

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("inconclusive", summary.Verdict);
		Assert.Contains("windows-co-resident-no-difference-untrusted", summary.Warnings);
	}

	[Fact]
	public void Execute_ScreenshotChangedAfterCapture_Throws()
	{
		var paths = CreateFixture(headColor: MagickColors.White);
		File.AppendAllText(Path.Combine(paths.RunsRoot, "head-run1", "screenshots", "initial.png"), "tampered");

		Assert.Throws<InvalidDataException>(() => Execute(paths));
	}

	[Fact]
	public void Execute_UnexpectedAppArtifact_Throws()
	{
		var paths = CreateFixture(headColor: MagickColors.White);
		var runPath = Path.Combine(paths.RunsRoot, "head-run1", "run-result.json");
		var run = UiEvidenceJson.Read<UiEvidenceRunResult>(runPath) with
		{
			AppArtifactSha256 = new string('9', 64)
		};
		UiEvidenceJson.Write(runPath, run);

		Assert.Throws<InvalidDataException>(() => Execute(paths));
	}

	[Fact]
	public void Execute_DevFlowUnavailable_IsInconclusive()
	{
		var paths = CreateFixture(headColor: MagickColors.White, devFlowComplete: false);

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("inconclusive", summary.Verdict);
		Assert.Contains("devflow-incomplete", summary.Warnings);
	}

	[Fact]
	public void Execute_SampledCoverage_IsInconclusive()
	{
		var paths = CreateFixture(headColor: MagickColors.White, coverage: "sampled");

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		Assert.Equal(
			"inconclusive",
			UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output).Verdict);
	}

	[Fact]
	public void Execute_EnvironmentMismatch_IsInconclusive()
	{
		var paths = CreateFixture(headColor: MagickColors.White, mismatchedEnvironment: true);

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("inconclusive", summary.Verdict);
		Assert.False(summary.EnvironmentComparable);
	}

	[Fact]
	public void Execute_HeadInfrastructureFailure_IsInconclusive()
	{
		var paths = CreateFixture(headColor: MagickColors.White, headStatus: "timed-out");

		var exitCode = Execute(paths);

		Assert.Equal(0, exitCode);
		var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
		Assert.Equal("inconclusive", summary.Verdict);
		Assert.Contains("head-failure-not-repeatable", summary.Warnings);
	}

	[Fact]
	public void Execute_ChangedRegistryBytes_ThrowsBeforeWritingSummary()
	{
		var paths = CreateFixture(headColor: MagickColors.White);
		File.AppendAllText(paths.Registry, "\n");

		var error = Assert.Throws<InvalidDataException>(() => Execute(paths));

		Assert.Contains("registry hash", error.Message, StringComparison.Ordinal);
		Assert.False(File.Exists(paths.Output));
	}

	[Fact]
	public void Execute_FilenameOnlyOutput_WritesVisualDiffBesideSummary()
	{
		var paths = CreateFixture(headColor: MagickColors.Black) with { Output = "comparison-summary.json" };
		var originalDirectory = Directory.GetCurrentDirectory();
		try
		{
			Directory.SetCurrentDirectory(_root);
			Assert.Equal(0, Execute(paths));
			var summary = UiEvidenceJson.Read<UiEvidenceComparisonSummary>(paths.Output);
			Assert.Equal("visual-change-advisory", summary.Verdict);
			Assert.True(File.Exists(Path.Combine(_root, summary.CheckpointComparisons[0].DiffPath!)));
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDirectory);
		}
	}

	[Fact]
	public void Execute_UnsafeScreenshotPath_Throws()
	{
		var paths = CreateFixture(headColor: MagickColors.White);
		var runPath = Path.Combine(paths.RunsRoot, "head-run1", "run-result.json");
		var run = UiEvidenceJson.Read<UiEvidenceRunResult>(runPath);
		run.Checkpoints[0] = run.Checkpoints[0] with { ScreenshotPath = "../outside.png" };
		UiEvidenceJson.Write(runPath, run);

		Assert.Throws<InvalidDataException>(() => Execute(paths));
	}

	static int Execute(FixturePaths paths) =>
		UiEvidenceCompareCommand.Execute(CommandLineOptions.Parse(
		[
			"--request", paths.Request,
			"--registry", paths.Registry,
			"--payload-manifest", paths.PayloadManifest,
			"--runs-root", paths.RunsRoot,
			"--output", paths.Output
		]));

	FixturePaths CreateFixture(
		IMagickColor<byte> headColor,
		string headStatus = "passed",
		string[]? headFindings = null,
		string platform = "android",
		string coverage = "direct",
		bool devFlowComplete = true,
		bool mismatchedEnvironment = false)
	{
		const string baseSha = "1111111111111111111111111111111111111111";
		const string headSha = "2222222222222222222222222222222222222222";
		const string harnessSha = "3333333333333333333333333333333333333333";
		const string requestKey = "maui-ui-0123456789abcdef01234567";
		var requestPath = Path.Combine(_root, "request.json");
		var registryPath = Path.Combine(_root, "registry.json");
		var payloadManifestPath = Path.Combine(_root, "payload-manifest.json");
		var runsRoot = Path.Combine(_root, "runs");
		var outputPath = Path.Combine(_root, "result", "comparison-summary.json");

		var request = new UiEvidenceRequest(
			1,
			requestKey,
			"dotnet/maui",
			42,
			baseSha,
			headSha,
			harnessSha,
			new string('4', 64),
			"layout-controls-smoke",
			platform,
			coverage,
			["base-1", "head-1", "head-2", "base-2"],
			2);
		UiEvidenceJson.Write(
			payloadManifestPath,
			new UiEvidencePayloadManifest(
				1,
				requestKey,
				"base/app/app.exe",
				"head/app/app.exe",
				new string('5', 64),
				new string('5', 64),
				new string('7', 64),
				new string('7', 64),
				[],
				[],
				new string('8', 40),
				"0.1.0-ui.test"));

		var scenario = new UiEvidenceScenario(
			"layout-controls-smoke",
			"Layout",
			"Test",
			"layout-controls-smoke",
			"UiEvidenceReady",
			[platform],
			[],
			[new ScenarioCheckpoint("initial", ["UiEvidenceReady"])],
			new ScenarioComparison(0.005, 0.001, 3));
		UiEvidenceJson.Write(
			registryPath,
			new ScenarioRegistry(1, new ScenarioLimits(8), [], [scenario]));
		request = request with
		{
			RegistrySha256 = Convert.ToHexString(
				System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(registryPath))).ToLowerInvariant()
		};
		UiEvidenceJson.Write(requestPath, request);

		var runs = new[]
		{
			(Name: "base-run1", Variant: "base", VariantRun: 1, Sequence: 1, Commit: baseSha, Status: "passed", Color: MagickColors.White, Findings: Array.Empty<string>()),
			(Name: "head-run1", Variant: "head", VariantRun: 1, Sequence: 2, Commit: headSha, Status: headStatus, Color: headColor, Findings: headFindings ?? []),
			(Name: "head-run2", Variant: "head", VariantRun: 2, Sequence: 3, Commit: headSha, Status: headStatus, Color: headColor, Findings: headFindings ?? []),
			(Name: "base-run2", Variant: "base", VariantRun: 2, Sequence: 4, Commit: baseSha, Status: "passed", Color: MagickColors.White, Findings: Array.Empty<string>())
		};

		foreach (var run in runs)
		{
			var runDirectory = Path.Combine(runsRoot, run.Name);
			var screenshotDirectory = Path.Combine(runDirectory, "screenshots");
			var devFlowDirectory = Path.Combine(runDirectory, "devflow");
			Directory.CreateDirectory(screenshotDirectory);
			Directory.CreateDirectory(devFlowDirectory);
			var screenshotPath = Path.Combine(screenshotDirectory, "initial.png");
			using (var image = new MagickImage(run.Color, 32, 32))
			{
				image.Format = MagickFormat.Png;
				image.Write(screenshotPath);
			}
			var screenshotHash = Convert.ToHexString(
				System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(screenshotPath))
			).ToLowerInvariant();
			var treePath = Path.Combine(devFlowDirectory, "tree.json");
			var layoutPath = Path.Combine(devFlowDirectory, "layout.json");
			UiEvidenceJson.Write(treePath, Array.Empty<SafeDevFlowTreeNode>());
			UiEvidenceJson.Write(
				layoutPath,
				new SafeLayoutEvidence("1.0", "1.0", "partial", true, 1, []));
			var treeHash = Convert.ToHexString(
				System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(treePath))
			).ToLowerInvariant();
			var layoutHash = Convert.ToHexString(
				System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(layoutPath))
			).ToLowerInvariant();

			var result = new UiEvidenceRunResult
			{
				RequestKey = requestKey,
				ScenarioId = "layout-controls-smoke",
				Platform = platform,
				Variant = run.Variant,
				VariantRunOrdinal = run.VariantRun,
				SequenceOrdinal = run.Sequence,
				CommitSha = run.Commit,
				HarnessSha = harnessSha,
				Status = run.Status,
				StartedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
				FinishedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
				AppArtifactSha256 = new string('5', 64),
				Environment = new UiEvidenceEnvironment(
					platform,
					"Windows",
					"10.0",
					mismatchedEnvironment && run.Name == "head-run2" ? "other-machine" : "machine",
					null,
					"machine",
					"10.0",
					null,
					null,
					null,
					"http://127.0.0.1:4723"),
				Assertions = [new UiEvidenceAssertion("UiEvidenceReady", run.Status == "passed" ? "passed" : "failed", null)],
				Checkpoints = [new UiEvidenceCheckpoint("initial", "screenshots/initial.png", screenshotHash, 32, 32)],
				DevFlow = new UiEvidenceDevFlow(
					devFlowComplete ? "captured" : "unavailable",
					run.Findings,
					devFlowComplete ? "devflow/tree.json" : null,
					devFlowComplete ? treeHash : null,
					devFlowComplete ? "devflow/layout.json" : null,
					devFlowComplete ? layoutHash : null,
					devFlowComplete)
			};
			UiEvidenceJson.Write(Path.Combine(runDirectory, "run-result.json"), result);
		}

		return new FixturePaths(requestPath, registryPath, payloadManifestPath, runsRoot, outputPath);
	}

	public void Dispose()
	{
		Directory.Delete(_root, recursive: true);
	}

	sealed record FixturePaths(
		string Request,
		string Registry,
		string PayloadManifest,
		string RunsRoot,
		string Output);
}
