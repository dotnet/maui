// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.Maui.DevTools.Models;

/// <summary>
/// Result of the doctor command.
/// </summary>
public record DoctorReport
{
	[JsonPropertyName("schema_version")]
	public string SchemaVersion { get; init; } = "1.0";

	[JsonPropertyName("correlation_id")]
	public required string CorrelationId { get; init; }

	[JsonPropertyName("timestamp")]
	public required DateTime Timestamp { get; init; }

	[JsonPropertyName("status")]
	public required HealthStatus Status { get; init; }

	[JsonPropertyName("checks")]
	public required List<HealthCheck> Checks { get; init; }

	[JsonPropertyName("summary")]
	public required DoctorSummary Summary { get; init; }
}

/// <summary>
/// Summary of doctor checks.
/// </summary>
public record DoctorSummary
{
	[JsonPropertyName("total")]
	public int Total { get; init; }

	[JsonPropertyName("ok")]
	public int Ok { get; init; }

	[JsonPropertyName("warning")]
	public int Warning { get; init; }

	[JsonPropertyName("error")]
	public int Error { get; init; }
}

/// <summary>
/// Individual health check result.
/// </summary>
public record HealthCheck
{
	[JsonPropertyName("category")]
	public required string Category { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("status")]
	public required CheckStatus Status { get; init; }

	[JsonPropertyName("message")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; init; }

	[JsonPropertyName("details")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public Dictionary<string, object>? Details { get; init; }

	[JsonPropertyName("fix")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public FixInfo? Fix { get; init; }
}

/// <summary>
/// Information about how to fix an issue.
/// </summary>
public record FixInfo
{
	[JsonPropertyName("issue_id")]
	public required string IssueId { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("auto_fixable")]
	public bool AutoFixable { get; init; }

	[JsonPropertyName("command")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Command { get; init; }

	[JsonPropertyName("manual_steps")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? ManualSteps { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthStatus
{
	Healthy,
	Unhealthy,
	Degraded
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CheckStatus
{
	Ok,
	Warning,
	Error,
	Skipped,
	NotApplicable
}
