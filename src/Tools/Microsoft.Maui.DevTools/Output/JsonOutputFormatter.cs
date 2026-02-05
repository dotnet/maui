// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Output;

/// <summary>
/// JSON output formatter for machine-readable output.
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
	private static readonly JsonSerializerOptions s_options = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
	};

	private readonly TextWriter _output;

	public JsonOutputFormatter(TextWriter? output = null)
	{
		_output = output ?? Console.Out;
	}

	public void Write<T>(T result)
	{
		WriteResult(result);
	}

	public void WriteResult<T>(T result)
	{
		var json = JsonSerializer.Serialize(result, s_options);
		_output.WriteLine(json);
	}

	public void WriteError(Exception exception)
	{
		WriteError(ErrorResult.FromException(exception));
	}

	public void WriteError(ErrorResult error)
	{
		WriteResult(error);
	}

	public void WriteSuccess(string message)
	{
		WriteResult(new { status = "success", message });
	}

	public void WriteWarning(string message)
	{
		WriteResult(new { status = "warning", message });
	}

	public void WriteInfo(string message)
	{
		WriteResult(new { status = "info", message });
	}

	public void WriteProgress(string message, int? percentage = null)
	{
		WriteResult(new { status = "progress", message, percentage });
	}

	public void WriteTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> Selector)[] columns)
	{
		var rows = items.Select(item => 
			columns.ToDictionary(c => c.Header.ToLowerInvariant(), c => c.Selector(item)));
		WriteResult(rows.ToList());
	}

	/// <summary>
	/// Serializes an object to JSON string.
	/// </summary>
	public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, s_options);

	/// <summary>
	/// Deserializes JSON string to object.
	/// </summary>
	public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, s_options);
}
