using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Maui.UiEvidence;

static class UiEvidenceJson
{
	public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	public static T Read<T>(string path) =>
		JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
		?? throw new InvalidDataException($"JSON document is empty: {path}");

	public static void Write<T>(string path, T value)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
		File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
	}
}

sealed record ScenarioRegistry(
	int SchemaVersion,
	ScenarioLimits Limits,
	string[] ProductRoots,
	UiEvidenceScenario[] Scenarios);

sealed record ScenarioLimits(int MaxScenarioPlatformPairs);

sealed record UiEvidenceScenario(
	string Id,
	string Title,
	string Description,
	string AppScenario,
	string ReadyAutomationId,
	string[] Platforms,
	ScenarioPathRule[] PathRules,
	ScenarioCheckpoint[] Checkpoints,
	ScenarioComparison Comparison);

sealed record ScenarioPathRule(string Pattern, string Coverage, string[]? Platforms);

sealed record ScenarioCheckpoint(string Id, string[] RequiredAutomationIds);

sealed record ScenarioComparison(
	double MaxIntraDifference,
	double MinInterDifference,
	double MinimumCrossToIntraMultiplier);

sealed record UiEvidenceRequest(
	int SchemaVersion,
	string RequestKey,
	string Repository,
	int PullRequestNumber,
	string BaseCommitSha,
	string HeadCommitSha,
	string HarnessSha,
	string RegistrySha256,
	string ScenarioId,
	string Platform,
	string Coverage,
	string[] ExpectedRunOrder,
	int ExpectedVariantRuns);

sealed record UiEvidencePayloadManifest(
	int SchemaVersion,
	string RequestKey,
	string BaseAppRelativePath,
	string HeadAppRelativePath,
	string BaseAppSha256,
	string HeadAppSha256,
	string BaseAppDirectorySha256,
	string HeadAppDirectorySha256,
	UiEvidencePayloadFile[] BaseFiles,
	UiEvidencePayloadFile[] HeadFiles,
	string DevFlowCommit,
	string DevFlowPackageVersion);

sealed record UiEvidencePayloadFile(
	string RelativePath,
	long SizeBytes,
	string Sha256);

sealed record UiEvidenceRunResult
{
	public int SchemaVersion { get; init; } = 1;
	public required string RequestKey { get; init; }
	public required string ScenarioId { get; init; }
	public required string Platform { get; init; }
	public required string Variant { get; init; }
	public required int VariantRunOrdinal { get; init; }
	public required int SequenceOrdinal { get; init; }
	public required string CommitSha { get; init; }
	public required string HarnessSha { get; init; }
	public required string Status { get; set; }
	public required string StartedAtUtc { get; init; }
	public required string FinishedAtUtc { get; set; }
	public required string AppArtifactSha256 { get; init; }
	public required UiEvidenceEnvironment Environment { get; init; }
	public List<UiEvidenceAssertion> Assertions { get; init; } = [];
	public List<UiEvidenceCheckpoint> Checkpoints { get; init; } = [];
	public UiEvidenceDevFlow DevFlow { get; set; } =
		new("not-requested", [], null, null, null, null, false);
	public List<string> ErrorCodes { get; init; } = [];
}

sealed record UiEvidenceEnvironment(
	string TargetPlatform,
	string HostOperatingSystem,
	string RuntimeVersion,
	string MachineName,
	string? DeviceId,
	string? DeviceModel,
	string? PlatformVersion,
	string? DisplaySize,
	string? DisplayDensity,
	string? Orientation,
	string AppiumUrl);

sealed record UiEvidenceAssertion(
	string Id,
	string Status,
	UiEvidenceRect? Bounds);

sealed record UiEvidenceCheckpoint(
	string Id,
	string ScreenshotPath,
	string ScreenshotSha256,
	int Width,
	int Height);

sealed record UiEvidenceRect(double X, double Y, double Width, double Height);

sealed record UiEvidenceDevFlow(
	string Status,
	string[] LayoutFindingKeys,
	string? TreePath,
	string? TreeSha256,
	string? LayoutPath,
	string? LayoutSha256,
	bool LayoutStable);

sealed record SafeDevFlowTreeNode(
	string Id,
	string? ParentId,
	string Type,
	string Framework,
	string? AutomationId,
	string? Role,
	bool IsVisible,
	bool IsEnabled,
	double Opacity,
	string? NativeType,
	string? SourceFile,
	int? SourceLine,
	int? SourceColumn,
	UiEvidenceRect? Bounds,
	SafeDevFlowTreeNode[] Children);

sealed record SafeLayoutEvidence(
	string SchemaVersion,
	string RuleSetVersion,
	string Coverage,
	bool Stable,
	int NodeCount,
	SafeLayoutFinding[] Findings);

sealed record SafeLayoutFinding(
	string Id,
	string RuleId,
	string? Subtype,
	string Outcome,
	string Severity,
	string Confidence,
	string Actionability,
	string ElementId,
	string ElementType,
	string? AutomationId,
	string? SourceFile,
	int? SourceLine,
	int? SourceColumn);

sealed record UiEvidenceComparisonSummary
{
	public int SchemaVersion { get; init; } = 1;
	public required UiEvidenceRequest Request { get; init; }
	public required bool ProvenanceValidated { get; init; }
	public required bool EnvironmentComparable { get; init; }
	public required bool EvidenceComplete { get; init; }
	public required UiEvidenceEnvironment Environment { get; init; }
	public required string TrustLevel { get; init; }
	public required string Verdict { get; init; }
	public required UiEvidenceCheckpointComparison[] CheckpointComparisons { get; init; }
	public required string[] NewLayoutFindingKeys { get; init; }
	public required string[] Errors { get; init; }
	public required string[] Warnings { get; init; }
}

sealed record UiEvidenceCheckpointComparison(
	string CheckpointId,
	string Status,
	double BaseIntraDifference,
	double HeadIntraDifference,
	double MinimumCrossDifference,
	double MaximumCrossDifference,
	string? DiffPath);
