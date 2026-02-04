// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Output;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class OutputFormatterTests
{
	[Fact]
	public void JsonOutputFormatter_WriteError_ProducesValidJson()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new JsonOutputFormatter(writer);

		var error = new ErrorResult
		{
			Code = ErrorCodes.JdkNotFound,
			Category = "platform",
			Message = "JDK not found"
		};

		formatter.WriteError(error);
		var output = sb.ToString();

		Assert.Contains("\"code\":", output);
		Assert.Contains("\"E2001\"", output);
		Assert.Contains("\"message\":", output);
		Assert.Contains("JDK not found", output);
	}

	[Fact]
	public void JsonOutputFormatter_Write_ProducesValidJson()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new JsonOutputFormatter(writer);

		var data = new { name = "test", value = 42 };
		formatter.Write(data);

		var output = sb.ToString();
		Assert.Contains("\"name\":", output);
		Assert.Contains("\"test\"", output);
		Assert.Contains("\"value\":", output);
		Assert.Contains("42", output);
	}

	[Fact]
	public void JsonOutputFormatter_WriteException_ConvertsToErrorResult()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new JsonOutputFormatter(writer);

		var exception = new MauiToolException(
			ErrorCodes.AndroidSdkNotFound,
			"Android SDK not found");

		formatter.WriteError(exception);
		var output = sb.ToString();

		Assert.Contains("\"E2101\"", output);
		Assert.Contains("Android SDK not found", output);
	}

	[Fact]
	public void ConsoleOutputFormatter_WriteSuccess_OutputsMessage()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new ConsoleOutputFormatter(writer, verbose: false);

		formatter.WriteSuccess("Operation completed");
		var output = sb.ToString();

		Assert.Contains("Operation completed", output);
	}

	[Fact]
	public void ConsoleOutputFormatter_WriteWarning_OutputsMessage()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new ConsoleOutputFormatter(writer, verbose: false);

		formatter.WriteWarning("This is a warning");
		var output = sb.ToString();

		Assert.Contains("This is a warning", output);
	}

	[Fact]
	public void ConsoleOutputFormatter_WriteInfo_OutputsMessage()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new ConsoleOutputFormatter(writer, verbose: false);

		formatter.WriteInfo("Information message");
		var output = sb.ToString();

		Assert.Contains("Information message", output);
	}

	[Fact]
	public void ConsoleOutputFormatter_WriteError_IncludesErrorCode()
	{
		var sb = new StringBuilder();
		var errorSb = new StringBuilder();
		using var writer = new StringWriter(sb);
		using var errorWriter = new StringWriter(errorSb);
		var formatter = new ConsoleOutputFormatter(writer, errorWriter, useColor: false, verbose: false);

		var error = new ErrorResult
		{
			Code = ErrorCodes.DeviceNotFound,
			Category = "tool",
			Message = "Device not found"
		};

		formatter.WriteError(error);
		var output = errorSb.ToString();

		Assert.Contains("E1006", output);
		Assert.Contains("Device not found", output);
	}

	[Fact]
	public void ConsoleOutputFormatter_WriteDoctorReport_FormatsCorrectly()
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var formatter = new ConsoleOutputFormatter(writer, verbose: false);

		var report = new DoctorReport
		{
			CorrelationId = "test123",
			Timestamp = DateTime.UtcNow,
			Status = HealthStatus.Healthy,
			Checks = new List<HealthCheck>
			{
				new HealthCheck
				{
					Category = "dotnet",
					Name = ".NET SDK",
					Status = CheckStatus.Ok,
					Message = "8.0.100"
				},
				new HealthCheck
				{
					Category = "android",
					Name = "JDK",
					Status = CheckStatus.Warning,
					Message = "JDK found but outdated"
				}
			},
			Summary = new DoctorSummary { Total = 2, Ok = 1, Warning = 1, Error = 0 }
		};

		formatter.WriteResult(report);
		var output = sb.ToString();

		Assert.Contains(".NET SDK", output);
		Assert.Contains("JDK", output);
	}
}
