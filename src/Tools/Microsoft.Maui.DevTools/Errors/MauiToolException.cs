// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.DevTools.Errors;

/// <summary>
/// Exception thrown by MAUI Dev Tools with structured error information.
/// </summary>
public class MauiToolException : Exception
{
	public string Code { get; }
	public RemediationInfo? Remediation { get; }
	public Dictionary<string, object>? Context { get; }
	public string? NativeError { get; }

	public MauiToolException(string code, string message)
		: base(message)
	{
		Code = code;
	}

	public MauiToolException(string code, string message, Exception innerException)
		: base(message, innerException)
	{
		Code = code;
	}

	public MauiToolException(string code, string message, RemediationInfo? remediation = null, 
		Dictionary<string, object>? context = null, string? nativeError = null)
		: base(message)
	{
		Code = code;
		Remediation = remediation;
		Context = context;
		NativeError = nativeError;
	}

	/// <summary>
	/// Creates an auto-fixable exception with a remediation command.
	/// </summary>
	public static MauiToolException AutoFixable(string code, string message, string fixCommand, 
		Dictionary<string, object>? context = null, string? nativeError = null)
	{
		return new MauiToolException(code, message, 
			new RemediationInfo(RemediationType.AutoFixable, fixCommand),
			context, nativeError);
	}

	/// <summary>
	/// Creates an exception requiring manual user action.
	/// </summary>
	public static MauiToolException UserActionRequired(string code, string message, string[] manualSteps,
		Dictionary<string, object>? context = null, string? nativeError = null)
	{
		return new MauiToolException(code, message,
			new RemediationInfo(RemediationType.UserAction, null, manualSteps),
			context, nativeError);
	}

	/// <summary>
	/// Creates a terminal exception that cannot be fixed.
	/// </summary>
	public static MauiToolException Terminal(string code, string message,
		Dictionary<string, object>? context = null, string? nativeError = null)
	{
		return new MauiToolException(code, message,
			new RemediationInfo(RemediationType.Terminal),
			context, nativeError);
	}
}

/// <summary>
/// Remediation information for an error.
/// </summary>
public record RemediationInfo(
	RemediationType Type,
	string? Command = null,
	string[]? ManualSteps = null,
	string? DocsUrl = null
);

/// <summary>
/// Type of remediation available for an error.
/// </summary>
public enum RemediationType
{
	/// <summary>The tool can fix this automatically.</summary>
	AutoFixable,
	/// <summary>User must take manual steps.</summary>
	UserAction,
	/// <summary>Cannot be fixed (e.g., unsupported OS).</summary>
	Terminal,
	/// <summary>Tool doesn't recognize this error.</summary>
	Unknown
}
