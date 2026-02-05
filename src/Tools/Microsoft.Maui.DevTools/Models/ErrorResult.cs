// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.Maui.DevTools.Models;

/// <summary>
/// Structured error result for JSON output.
/// </summary>
public record ErrorResult
{
	[JsonPropertyName("code")]
	public required string Code { get; init; }

	[JsonPropertyName("category")]
	public required string Category { get; init; }

	[JsonPropertyName("severity")]
	public string Severity { get; init; } = "error";

	[JsonPropertyName("message")]
	public required string Message { get; init; }

	[JsonPropertyName("native_error")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? NativeError { get; init; }

	[JsonPropertyName("context")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public Dictionary<string, object>? Context { get; init; }

	[JsonPropertyName("remediation")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RemediationResult? Remediation { get; init; }

	[JsonPropertyName("docs_url")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? DocsUrl { get; init; }

	[JsonPropertyName("correlation_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? CorrelationId { get; init; }

	/// <summary>
	/// Derives error category from error code.
	/// </summary>
	public static string GetCategory(string code)
	{
		if (code.Length < 3) return "unknown";
		
		return code[1] switch
		{
			'1' => "tool",
			'2' => "platform",
			'3' => "user",
			'4' => "network",
			'5' => "permission",
			_ => "unknown"
		};
	}

	/// <summary>
	/// Converts an exception to a structured ErrorResult.
	/// </summary>
	public static ErrorResult FromException(Exception exception)
	{
		if (exception is Errors.MauiToolException mex)
		{
			return new ErrorResult
			{
				Code = mex.Code,
				Category = GetCategory(mex.Code),
				Message = mex.Message,
				NativeError = mex.NativeError,
				Remediation = mex.Remediation != null ? new RemediationResult
				{
					Type = mex.Remediation.Type.ToString().ToLowerInvariant(),
					Command = mex.Remediation.Command,
					ManualSteps = mex.Remediation.ManualSteps
				} : null
			};
		}

		return new ErrorResult
		{
			Code = Errors.ErrorCodes.InternalError,
			Category = "tool",
			Message = exception.Message
		};
	}
}

/// <summary>
/// Remediation information in JSON output.
/// </summary>
public record RemediationResult
{
	[JsonPropertyName("type")]
	public required string Type { get; init; }

	[JsonPropertyName("command")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Command { get; init; }

	[JsonPropertyName("manual_steps")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? ManualSteps { get; init; }
}

/// <summary>
/// JSON output envelope for all commands.
/// </summary>
public record CommandResult<T>
{
	[JsonPropertyName("success")]
	public bool Success { get; init; }

	[JsonPropertyName("correlation_id")]
	public required string CorrelationId { get; init; }

	[JsonPropertyName("duration_ms")]
	public long DurationMs { get; init; }

	[JsonPropertyName("result")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public T? Result { get; init; }

	[JsonPropertyName("errors")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<ErrorResult>? Errors { get; init; }

	[JsonPropertyName("warnings")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<ErrorResult>? Warnings { get; init; }
}

/// <summary>
/// Dry-run output for preview operations.
/// </summary>
public record DryRunResult
{
	[JsonPropertyName("dry_run")]
	public bool DryRun { get; init; } = true;

	[JsonPropertyName("planned_operations")]
	public required List<PlannedOperation> PlannedOperations { get; init; }

	[JsonPropertyName("estimated_duration_seconds")]
	public int? EstimatedDurationSeconds { get; init; }

	[JsonPropertyName("requires_approval")]
	public bool RequiresApproval { get; init; }

	[JsonPropertyName("approval_reason")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ApprovalReason { get; init; }
}

/// <summary>
/// A planned operation in dry-run mode.
/// </summary>
public record PlannedOperation
{
	[JsonPropertyName("action")]
	public required string Action { get; init; }

	[JsonPropertyName("target")]
	public required string Target { get; init; }

	[JsonPropertyName("size_mb")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? SizeMb { get; init; }
}
