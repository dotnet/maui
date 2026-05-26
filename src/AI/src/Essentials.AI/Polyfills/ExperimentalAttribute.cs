// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET8_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that an API element is experimental and subject to change without notice.
/// </summary>
[AttributeUsage(
	AttributeTargets.Class |
	AttributeTargets.Struct |
	AttributeTargets.Enum |
	AttributeTargets.Interface |
	AttributeTargets.Delegate |
	AttributeTargets.Method |
	AttributeTargets.Constructor |
	AttributeTargets.Property |
	AttributeTargets.Field |
	AttributeTargets.Event |
	AttributeTargets.Assembly)]
internal sealed class ExperimentalAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExperimentalAttribute"/> class.
	/// </summary>
	/// <param name="diagnosticId">The diagnostic ID associated with this experimental API.</param>
	public ExperimentalAttribute(string diagnosticId)
	{
		DiagnosticId = diagnosticId;
	}

	/// <summary>
	/// Gets the ID that the compiler will use when reporting a use of the API the attribute applies to.
	/// </summary>
	public string DiagnosticId { get; }

	/// <summary>
	/// Gets or sets the URL for corresponding documentation.
	/// The API accepts a format string instead of an actual URL, creating a generic URL that includes the diagnostic ID.
	/// </summary>
	public string? UrlFormat { get; set; }
}

#endif
