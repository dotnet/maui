// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Output;

/// <summary>
/// Console output formatter for human-readable output.
/// </summary>
public class ConsoleOutputFormatter : IOutputFormatter
{
	private readonly TextWriter _output;
	private readonly TextWriter _error;
	private readonly bool _useColor;
	private readonly bool _verbose;

	public ConsoleOutputFormatter(TextWriter? output = null, bool verbose = false)
		: this(output, null, true, verbose)
	{
	}

	public ConsoleOutputFormatter(TextWriter? output, TextWriter? error, bool useColor = true, bool verbose = false)
	{
		_output = output ?? Console.Out;
		_error = error ?? Console.Error;
		_useColor = useColor && !Console.IsOutputRedirected;
		_verbose = verbose;
	}

	public bool Verbose => _verbose;

	public void Write<T>(T result)
	{
		WriteResult(result);
	}

	public void WriteResult<T>(T result)
	{
		// For complex objects, use reflection to display properties
		if (result is null)
			return;

		var type = result.GetType();
		
		// Handle doctor report specially
		if (result is DoctorReport report)
		{
			WriteDoctorReport(report);
			return;
		}

		// Handle device list specially
		if (result is DeviceListResult deviceList)
		{
			WriteDeviceList(deviceList);
			return;
		}

		// Default: just write ToString
		_output.WriteLine(result.ToString());
	}

	private void WriteDoctorReport(DoctorReport report)
	{
		_output.WriteLine();
		WriteColored("MAUI Doctor", ConsoleColor.Cyan);
		_output.WriteLine();
		_output.WriteLine();

		foreach (var check in report.Checks)
		{
			var icon = check.Status switch
			{
				CheckStatus.Ok => ("✓", ConsoleColor.Green),
				CheckStatus.Warning => ("⚠", ConsoleColor.Yellow),
				CheckStatus.Error => ("✗", ConsoleColor.Red),
				CheckStatus.Skipped => ("○", ConsoleColor.Gray),
				_ => ("?", ConsoleColor.Gray)
			};

			WriteColored($"  {icon.Item1} ", icon.Item2);
			_output.Write($"{check.Name}");
			
			if (!string.IsNullOrEmpty(check.Message))
			{
				WriteColored($" - {check.Message}", ConsoleColor.Gray);
			}
			_output.WriteLine();

			if (check.Fix != null && !check.Fix.AutoFixable)
			{
				WriteColored($"    Fix: ", ConsoleColor.Yellow);
				_output.WriteLine(check.Fix.Description);
			}
		}

		_output.WriteLine();
		
		var statusColor = report.Status switch
		{
			HealthStatus.Healthy => ConsoleColor.Green,
			HealthStatus.Degraded => ConsoleColor.Yellow,
			HealthStatus.Unhealthy => ConsoleColor.Red,
			_ => ConsoleColor.Gray
		};

		WriteColored($"Status: {report.Status}", statusColor);
		_output.WriteLine($" ({report.Summary.Ok} ok, {report.Summary.Warning} warning, {report.Summary.Error} error)");
		_output.WriteLine();
	}

	private void WriteDeviceList(DeviceListResult result)
	{
		if (result.Devices.Count == 0)
		{
			WriteWarning("No devices found");
			return;
		}

		WriteTable(result.Devices,
			("ID", d => d.Id),
			("Name", d => d.Name),
			("Platform", d => d.Platform),
			("Type", d => d.Type.ToString()),
			("State", d => d.State.ToString()));
	}

	public void WriteError(Exception exception)
	{
		var errorResult = ErrorResult.FromException(exception);
		if (exception is not Errors.MauiToolException)
		{
			WriteColored($"Error: ", ConsoleColor.Red);
			_error.WriteLine(exception.Message);
		}
		else
		{
			WriteError(errorResult);
		}
	}

	public void WriteError(ErrorResult error)
	{
		WriteColoredToError($"Error [{error.Code}]: ", ConsoleColor.Red);
		_error.WriteLine(error.Message);

		if (error.Remediation != null)
		{
			if (error.Remediation.Command != null)
			{
				WriteColoredToError("  Fix: ", ConsoleColor.Yellow);
				_error.WriteLine(error.Remediation.Command);
			}
			else if (error.Remediation.ManualSteps != null)
			{
				WriteColoredToError("  Manual steps required:", ConsoleColor.Yellow);
				_error.WriteLine();
				foreach (var step in error.Remediation.ManualSteps)
				{
					_error.WriteLine($"    - {step}");
				}
			}
		}
	}

	public void WriteSuccess(string message)
	{
		WriteColored("✓ ", ConsoleColor.Green);
		_output.WriteLine(message);
	}

	public void WriteWarning(string message)
	{
		WriteColored("⚠ ", ConsoleColor.Yellow);
		_output.WriteLine(message);
	}

	public void WriteInfo(string message)
	{
		WriteColored("ℹ ", ConsoleColor.Cyan);
		_output.WriteLine(message);
	}

	public void WriteProgress(string message, int? percentage = null)
	{
		if (percentage.HasValue)
		{
			_output.Write($"\r[{percentage,3}%] {message}");
		}
		else
		{
			_output.WriteLine($"  {message}");
		}
	}

	public void WriteTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> Selector)[] columns)
	{
		var itemsList = items.ToList();
		if (itemsList.Count == 0)
			return;

		// Calculate column widths
		var widths = columns.Select((c, i) =>
			Math.Max(c.Header.Length, itemsList.Max(item => c.Selector(item)?.Length ?? 0))).ToArray();

		// Write header
		_output.WriteLine();
		for (int i = 0; i < columns.Length; i++)
		{
			WriteColored(columns[i].Header.PadRight(widths[i] + 2), ConsoleColor.Cyan);
		}
		_output.WriteLine();

		// Write separator
		for (int i = 0; i < columns.Length; i++)
		{
			_output.Write(new string('-', widths[i]));
			_output.Write("  ");
		}
		_output.WriteLine();

		// Write rows
		foreach (var item in itemsList)
		{
			for (int i = 0; i < columns.Length; i++)
			{
				var value = columns[i].Selector(item) ?? "";
				_output.Write(value.PadRight(widths[i] + 2));
			}
			_output.WriteLine();
		}
		_output.WriteLine();
	}

	private void WriteColored(string text, ConsoleColor color)
	{
		if (_useColor)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			_output.Write(text);
			Console.ForegroundColor = originalColor;
		}
		else
		{
			_output.Write(text);
		}
	}

	private void WriteColoredToError(string text, ConsoleColor color)
	{
		if (_useColor)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			_error.Write(text);
			Console.ForegroundColor = originalColor;
		}
		else
		{
			_error.Write(text);
		}
	}
}
