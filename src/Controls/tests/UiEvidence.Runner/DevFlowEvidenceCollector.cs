#if MAUI_UI_EVIDENCE_DEVFLOW
using Microsoft.Maui.DevFlow.Driver;
#endif
using System.Text.RegularExpressions;

namespace Microsoft.Maui.UiEvidence;

static class DevFlowEvidenceCollector
{
	static readonly Regex SafeTokenPattern = new(
		@"^[A-Za-z0-9_.:-]+$",
		RegexOptions.CultureInvariant | RegexOptions.Compiled);
	static readonly Regex SafeFileNamePattern = new(
		@"^[A-Za-z0-9_.-]+$",
		RegexOptions.CultureInvariant | RegexOptions.Compiled);

	public static async Task<UiEvidenceDevFlow> CaptureAsync(string outputRoot, int port)
	{
#if MAUI_UI_EVIDENCE_DEVFLOW
		try
		{
			using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var client = new AgentClient("127.0.0.1", port);
			AgentStatus? status = null;
			while (!timeout.IsCancellationRequested)
			{
				try
				{
					status = await client.GetStatusAsync();
					if (status is not null)
						break;
				}
				catch
				{
				}
				await Task.Delay(500, timeout.Token);
			}

			if (status is null)
				return new UiEvidenceDevFlow("unavailable", [], null, null, null, null, false);

			var tree = await client.GetTreeAsync(12, window: null, includeNative: false);
			if (tree.Sum(CountTreeNodes) > 2000)
				return new UiEvidenceDevFlow("capture-failed", [], null, null, null, null, false);
			var safeTree = tree.Select(ConvertTree).ToArray();
			var treePath = Path.Combine(outputRoot, "devflow", "tree.json");
			UiEvidenceJson.Write(treePath, safeTree);
			var treeHash = GetSha256(treePath);

			var layout = await client.AnalyzeLayoutAsync(
				new LayoutInspectionRequest
				{
					Profile = "agent",
					MinimumSeverity = "minor",
					IncludeEvidence = true,
					IncludePasses = false,
					Scope = new LayoutInspectionScope
					{
						IncludeDescendants = true,
						IncludeNativeElements = false,
						IncludeBlazorElements = false,
						MaxDepth = 12
					},
					Privacy = new LayoutPrivacyOptions { Text = "none" }
				},
				timeout.Token);

			string? layoutRelativePath = null;
			string? layoutHash = null;
			var layoutStable = false;
			var findingKeys = Array.Empty<string>();
			if (layout is not null)
			{
				if (layout.Snapshot.NodeCount > 2000)
					return new UiEvidenceDevFlow("capture-failed", [], null, null, null, null, false);

				var safeLayout = new SafeLayoutEvidence(
					layout.SchemaVersion,
					layout.RuleSetVersion,
					layout.Coverage.Overall,
					layout.Snapshot.Stable,
					layout.Snapshot.NodeCount,
					layout.Findings.Select(finding => new SafeLayoutFinding(
						SafeToken(finding.Id),
						SafeToken(finding.RuleId),
						SafeOptionalToken(finding.Subtype),
						SafeToken(finding.Outcome),
						SafeToken(finding.Severity),
						SafeToken(finding.Confidence),
						SafeToken(finding.Actionability),
						SafeToken(finding.Element.Id),
						SafeToken(finding.Element.Type),
						SafeOptionalToken(finding.Element.AutomationId),
						SafeFileName(finding.Element.SourceFile),
						finding.Element.SourceLine,
						finding.Element.SourceColumn)).ToArray());
				var layoutPath = Path.Combine(outputRoot, "devflow", "layout.json");
				UiEvidenceJson.Write(layoutPath, safeLayout);
				layoutRelativePath = Path.GetRelativePath(outputRoot, layoutPath).Replace('\\', '/');
				layoutHash = GetSha256(layoutPath);
				layoutStable = safeLayout.Stable;
				findingKeys = safeLayout.Findings
					.Where(finding => finding.Outcome == "violation")
					.Select(finding => $"{finding.RuleId}|{finding.ElementType}|{finding.AutomationId ?? "none"}")
					.Distinct(StringComparer.Ordinal)
					.OrderBy(value => value, StringComparer.Ordinal)
					.ToArray();
			}

			return new UiEvidenceDevFlow(
				"captured",
				findingKeys,
				Path.GetRelativePath(outputRoot, treePath).Replace('\\', '/'),
				treeHash,
				layoutRelativePath,
				layoutHash,
				layoutStable);
		}
		catch
		{
			return new UiEvidenceDevFlow("capture-failed", [], null, null, null, null, false);
		}
#else
		await Task.CompletedTask;
		return new UiEvidenceDevFlow("unavailable", [], null, null, null, null, false);
#endif
	}

#if MAUI_UI_EVIDENCE_DEVFLOW
	static int CountTreeNodes(ElementInfo element) =>
		1 + (element.Children?.Sum(CountTreeNodes) ?? 0);

	static SafeDevFlowTreeNode ConvertTree(ElementInfo element) =>
		new(
			SafeToken(element.Id),
			SafeOptionalToken(element.ParentId),
			SafeToken(element.Type),
			SafeToken(element.Framework),
			SafeOptionalToken(element.AutomationId),
			SafeOptionalToken(element.Role),
			element.IsVisible,
			element.IsEnabled,
			element.Opacity,
			SafeOptionalToken(element.NativeType),
			SafeFileName(element.SourceFile),
			element.SourceLine,
			element.SourceColumn,
			element.Bounds is null
				? null
				: new UiEvidenceRect(
					element.Bounds.X,
					element.Bounds.Y,
					element.Bounds.Width,
					element.Bounds.Height),
			element.Children?.Select(ConvertTree).ToArray() ?? []);
#endif

	internal static string SafeToken(string? value) =>
		!string.IsNullOrWhiteSpace(value) &&
		value.Length <= 100 &&
		SafeTokenPattern.IsMatch(value)
			? value
			: "unknown";

	internal static string? SafeOptionalToken(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;

		var safe = SafeToken(value);
		return safe == "unknown" ? null : safe;
	}

	static string? SafeFileName(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;

		var fileName = Path.GetFileName(value);
		return fileName.Length <= 120 &&
			SafeFileNamePattern.IsMatch(fileName)
				? fileName
				: null;
	}

	static string GetSha256(string path)
	{
		using var stream = File.OpenRead(path);
		return Convert.ToHexString(
			System.Security.Cryptography.SHA256.HashData(stream)
		).ToLowerInvariant();
	}
}
