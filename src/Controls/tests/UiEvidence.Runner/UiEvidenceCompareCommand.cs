using ImageMagick;
using System.Security.Cryptography;
using VisualTestUtils;
using VisualTestUtils.MagickNet;

namespace Microsoft.Maui.UiEvidence;

static class UiEvidenceCompareCommand
{
	static readonly string[] RunDirectories = ["base-run1", "head-run1", "head-run2", "base-run2"];

	public static int Execute(CommandLineOptions options)
	{
		var request = UiEvidenceJson.Read<UiEvidenceRequest>(options.Required("request"));
		var payload = UiEvidenceJson.Read<UiEvidencePayloadManifest>(options.Required("payload-manifest"));
		if (payload.SchemaVersion != 1 || payload.RequestKey != request.RequestKey)
			throw new InvalidDataException("Payload manifest does not match the UI evidence request.");
		var registryPath = options.Required("registry");
		var registryHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(registryPath))).ToLowerInvariant();
		if (registryHash != request.RegistrySha256)
			throw new InvalidDataException("Scenario registry hash does not match the UI evidence request.");
		var registry = UiEvidenceJson.Read<ScenarioRegistry>(registryPath);
		var outputPath = Path.GetFullPath(options.Required("output"));
		var scenario = registry.Scenarios.SingleOrDefault(item => item.Id == request.ScenarioId)
			?? throw new InvalidDataException($"Scenario '{request.ScenarioId}' is missing from the registry.");
		var runsRoot = Path.GetFullPath(options.Required("runs-root"));
		var runRoots = RunDirectories
			.Select(directory => GetSafeRelativePath(runsRoot, directory))
			.ToArray();
		var runs = RunDirectories
			.Select((_, index) => UiEvidenceJson.Read<UiEvidenceRunResult>(
				Path.Combine(runRoots[index], "run-result.json")))
			.ToArray();

		ValidateProvenance(request, runs);
		ValidateAppArtifacts(payload, runs);
		var errors = new List<string>();
		var warnings = new List<string>();
		var comparisons = new List<UiEvidenceCheckpointComparison>();

		foreach (var checkpoint in scenario.Checkpoints)
		{
			var screenshots = runs
				.Select(run => run.Checkpoints.SingleOrDefault(item => item.Id == checkpoint.Id))
				.ToArray();
			if (screenshots.Any(item => item is null))
			{
				comparisons.Add(new UiEvidenceCheckpointComparison(
					checkpoint.Id,
					"missing",
					0,
					0,
					0,
					0,
					null));
				errors.Add($"missing-screenshot:{checkpoint.Id}");
				continue;
			}

			var paths = screenshots
				.Select((item, index) => GetSafeRelativePath(runRoots[index], item!.ScreenshotPath))
				.ToArray();
			for (var index = 0; index < paths.Length; index++)
			{
				if (!File.Exists(paths[index]))
					throw new InvalidDataException($"Screenshot is missing for {RunDirectories[index]}.");
				if (GetSha256(paths[index]) != screenshots[index]!.ScreenshotSha256)
					throw new InvalidDataException($"Screenshot hash changed for {RunDirectories[index]}.");
			}
			var baseIntra = Compare(paths[0], paths[3]);
			var headIntra = Compare(paths[1], paths[2]);
			var cross = new[]
			{
				(First: paths[0], Second: paths[1], Difference: Compare(paths[0], paths[1])),
				(First: paths[0], Second: paths[2], Difference: Compare(paths[0], paths[2])),
				(First: paths[3], Second: paths[1], Difference: Compare(paths[3], paths[1])),
				(First: paths[3], Second: paths[2], Difference: Compare(paths[3], paths[2]))
			};
			var minimumCross = cross.Min(item => item.Difference);
			var maximumCross = cross.Max(item => item.Difference);
			var observedIntra = Math.Max(baseIntra, headIntra);
			var stable = baseIntra <= scenario.Comparison.MaxIntraDifference &&
				headIntra <= scenario.Comparison.MaxIntraDifference;
			var changeThreshold = Math.Max(
				scenario.Comparison.MinInterDifference,
				observedIntra * scenario.Comparison.MinimumCrossToIntraMultiplier);

			string status;
			string? diffPath = null;
			if (!stable)
			{
				status = "unstable";
			}
			else if (minimumCross > changeThreshold)
			{
				status = "change-detected";
				diffPath = Path.Combine("diffs", $"{checkpoint.Id}-diff.png").Replace('\\', '/');
				var absoluteDiffPath = Path.Combine(Path.GetDirectoryName(outputPath)!, diffPath);
				Directory.CreateDirectory(Path.GetDirectoryName(absoluteDiffPath)!);
				var generator = new MagickNetVisualDiffGenerator();
				var diffPair = cross.MaxBy(item => item.Difference);
				var diff = generator.GenerateDiff(
					new ImageSnapshot(diffPair.First),
					new ImageSnapshot(diffPair.Second));
				diff.Save(Path.GetDirectoryName(absoluteDiffPath)!, Path.GetFileNameWithoutExtension(absoluteDiffPath));
			}
			else if (maximumCross <= Math.Max(
				scenario.Comparison.MaxIntraDifference,
				observedIntra * scenario.Comparison.MinimumCrossToIntraMultiplier))
			{
				status = "no-difference-observed";
			}
			else
			{
				status = "unstable";
			}

			comparisons.Add(new UiEvidenceCheckpointComparison(
				checkpoint.Id,
				status,
				baseIntra,
				headIntra,
				minimumCross,
				maximumCross,
				diffPath));
		}

		var baseRuns = new[] { runs[0], runs[3] };
		var headRuns = new[] { runs[1], runs[2] };
		var basePassed = baseRuns.All(run => run.Status == "passed");
		var headPassed = headRuns.All(run => run.Status == "passed");
		var devFlowComplete = runs
			.Select((run, index) => ValidateDevFlowEvidence(run, runRoots[index]))
			.All(value => value);
		if (!devFlowComplete)
			warnings.Add("devflow-incomplete");
		var evidenceComplete = devFlowComplete &&
			runs.All(run => run.Checkpoints.Count == scenario.Checkpoints.Length) &&
			comparisons.All(comparison => comparison.Status is not ("missing" or "unstable"));
		var environmentComparable = runs
			.Select(run => System.Text.Json.JsonSerializer.Serialize(run.Environment, UiEvidenceJson.Options))
			.Distinct(StringComparer.Ordinal)
			.Count() == 1;
		if (!environmentComparable)
			errors.Add("environment-mismatch");

		var baseFindings = baseRuns
			.Select(run => run.DevFlow.LayoutFindingKeys.ToHashSet(StringComparer.Ordinal))
			.ToArray();
		var headFindings = headRuns
			.Select(run => run.DevFlow.LayoutFindingKeys.ToHashSet(StringComparer.Ordinal))
			.ToArray();
		var stableHeadFindings = headFindings[0]
			.Intersect(headFindings[1], StringComparer.Ordinal)
			.ToHashSet(StringComparer.Ordinal);
		var anyBaseFindings = baseFindings[0]
			.Union(baseFindings[1], StringComparer.Ordinal)
			.ToHashSet(StringComparer.Ordinal);
		var newLayoutFindings = stableHeadFindings
			.Except(anyBaseFindings, StringComparer.Ordinal)
			.OrderBy(value => value, StringComparer.Ordinal)
			.ToArray();
		if (!devFlowComplete)
			newLayoutFindings = [];

		var headFailureSignatures = headRuns.Select(GetFailureSignature).ToArray();
		var repeatableHeadScenarioFailure =
			basePassed &&
			headRuns.All(run => run.Status == "scenario-failed") &&
			headFailureSignatures[0].Length > 0 &&
			headFailureSignatures[0] == headFailureSignatures[1];
		if (!headPassed && !repeatableHeadScenarioFailure)
			warnings.Add("head-failure-not-repeatable");
		var verdict = !environmentComparable
			? "inconclusive"
			: repeatableHeadScenarioFailure
			? "head-functional-failure-advisory"
			: !basePassed || !headPassed
				? "inconclusive"
				: comparisons.Any(comparison => comparison.Status == "change-detected")
				? "visual-change-advisory"
				: devFlowComplete && newLayoutFindings.Length > 0
					? "layout-change-advisory"
					: !evidenceComplete || !basePassed || !headPassed || request.Coverage != "direct"
						? "inconclusive"
						: "no-difference-observed";
		if (request.Platform == "windows" && verdict == "no-difference-observed")
		{
			verdict = "inconclusive";
			warnings.Add("windows-co-resident-no-difference-untrusted");
		}

		var summary = new UiEvidenceComparisonSummary
		{
			Request = request,
			ProvenanceValidated = true,
			EnvironmentComparable = environmentComparable,
			EvidenceComplete = evidenceComplete,
			Environment = runs[0].Environment,
			TrustLevel = request.Platform == "android" ? "isolated-emulator" : "co-resident-advisory",
			Verdict = verdict,
			CheckpointComparisons = comparisons.ToArray(),
			NewLayoutFindingKeys = newLayoutFindings,
			Errors = errors.ToArray(),
			Warnings = warnings.ToArray()
		};
		UiEvidenceJson.Write(options.Required("output"), summary);
		return 0;
	}

	static void ValidateProvenance(UiEvidenceRequest request, UiEvidenceRunResult[] runs)
	{
		if (runs.Length != 4)
			throw new InvalidDataException("Exactly four UI evidence runs are required.");

		var expected = new[]
		{
			(Variant: "base", Run: 1, Sequence: 1, Commit: request.BaseCommitSha),
			(Variant: "head", Run: 1, Sequence: 2, Commit: request.HeadCommitSha),
			(Variant: "head", Run: 2, Sequence: 3, Commit: request.HeadCommitSha),
			(Variant: "base", Run: 2, Sequence: 4, Commit: request.BaseCommitSha)
		};

		for (var index = 0; index < runs.Length; index++)
		{
			var run = runs[index];
			var item = expected[index];
			if (run.RequestKey != request.RequestKey ||
				run.ScenarioId != request.ScenarioId ||
				run.Platform != request.Platform ||
				run.HarnessSha != request.HarnessSha ||
				run.Variant != item.Variant ||
				run.VariantRunOrdinal != item.Run ||
				run.SequenceOrdinal != item.Sequence ||
				run.CommitSha != item.Commit)
			{
				throw new InvalidDataException($"Run '{RunDirectories[index]}' provenance does not match the request.");
			}
		}
	}

	static void ValidateAppArtifacts(UiEvidencePayloadManifest payload, UiEvidenceRunResult[] runs)
	{
		foreach (var run in runs)
		{
			var expectedHash = run.Variant == "base"
				? payload.BaseAppSha256
				: payload.HeadAppSha256;
			if (run.AppArtifactSha256 != expectedHash)
				throw new InvalidDataException($"Run '{run.Variant}-{run.VariantRunOrdinal}' used an unexpected app artifact.");
		}
	}

	static bool ValidateDevFlowEvidence(UiEvidenceRunResult run, string runRoot)
	{
		if (run.DevFlow.Status != "captured" ||
			run.DevFlow.TreePath is null ||
			run.DevFlow.TreeSha256 is null ||
			run.DevFlow.LayoutPath is null ||
			run.DevFlow.LayoutSha256 is null ||
			!run.DevFlow.LayoutStable)
		{
			return false;
		}

		var treePath = GetSafeRelativePath(runRoot, run.DevFlow.TreePath);
		var layoutPath = GetSafeRelativePath(runRoot, run.DevFlow.LayoutPath);
		if (!File.Exists(treePath) ||
			!File.Exists(layoutPath) ||
			GetSha256(treePath) != run.DevFlow.TreeSha256 ||
			GetSha256(layoutPath) != run.DevFlow.LayoutSha256)
		{
			throw new InvalidDataException($"DevFlow evidence changed for {run.Variant}-{run.VariantRunOrdinal}.");
		}

		var layout = UiEvidenceJson.Read<SafeLayoutEvidence>(layoutPath);
		return layout.Stable && layout.NodeCount is > 0 and <= 2000;
	}

	static string GetFailureSignature(UiEvidenceRunResult run)
	{
		var failedAssertions = run.Assertions
			.Where(assertion => assertion.Status == "failed")
			.Select(assertion => assertion.Id);
		return string.Join(
			"|",
			failedAssertions
				.Concat(run.ErrorCodes)
				.OrderBy(value => value, StringComparer.Ordinal));
	}

	static string GetSafeRelativePath(string root, string relativePath)
	{
		if (string.IsNullOrWhiteSpace(relativePath) ||
			Path.IsPathRooted(relativePath) ||
			relativePath.IndexOf("\\", StringComparison.Ordinal) >= 0 ||
			relativePath.Length > 240)
		{
			throw new InvalidDataException("Evidence contains an unsafe relative path.");
		}
		var segments = relativePath.Split('/');
		if (segments.Any(segment => segment is "" or "." or ".."))
			throw new InvalidDataException("Evidence contains an unsafe relative path.");

		var fullRoot = Path.GetFullPath(root);
		var path = Path.GetFullPath(Path.Combine(fullRoot, relativePath));
		var comparison = OperatingSystem.IsWindows()
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;
		if (!path.StartsWith(
			fullRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
			comparison))
		{
			throw new InvalidDataException("Evidence path escapes its run directory.");
		}
		return path;
	}

	static string GetSha256(string path)
	{
		using var stream = File.OpenRead(path);
		return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
	}

	static double Compare(string firstPath, string secondPath)
	{
		using var first = new MagickImage(firstPath);
		using var second = new MagickImage(secondPath);
		if (first.Width != second.Width || first.Height != second.Height)
			return 1;

		return first.Compare(second, ErrorMetric.RootMeanSquared, Channels.All);
	}
}
