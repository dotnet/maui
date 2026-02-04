// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Output;

/// <summary>
/// Interface for output formatting.
/// </summary>
public interface IOutputFormatter
{
	/// <summary>
	/// Writes a generic object to output.
	/// </summary>
	void Write<T>(T result);

	/// <summary>
	/// Writes the result to output.
	/// </summary>
	void WriteResult<T>(T result);

	/// <summary>
	/// Writes an error from an exception.
	/// </summary>
	void WriteError(Exception exception);

	/// <summary>
	/// Writes an error to output.
	/// </summary>
	void WriteError(ErrorResult error);

	/// <summary>
	/// Writes a success message.
	/// </summary>
	void WriteSuccess(string message);

	/// <summary>
	/// Writes a warning message.
	/// </summary>
	void WriteWarning(string message);

	/// <summary>
	/// Writes an info message.
	/// </summary>
	void WriteInfo(string message);

	/// <summary>
	/// Writes a progress update.
	/// </summary>
	void WriteProgress(string message, int? percentage = null);

	/// <summary>
	/// Writes a table of data.
	/// </summary>
	void WriteTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> Selector)[] columns);
}
