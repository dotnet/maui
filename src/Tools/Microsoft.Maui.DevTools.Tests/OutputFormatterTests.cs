// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Output;
using Spectre.Console;
using Spectre.Console.Testing;
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

	private static (SpectreOutputFormatter formatter, TestConsole console) CreateTestFormatter(bool verbose = false)
	{
		var console = new TestConsole();
		var formatter = new SpectreOutputFormatter(console, verbose);
		return (formatter, console);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteSuccess_OutputsMessage()
	{
		var (formatter, console) = CreateTestFormatter();

		formatter.WriteSuccess("Operation completed");
		var output = console.Output;

		Assert.Contains("Operation completed", output);
		Assert.Contains("✓", output);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteWarning_OutputsMessage()
	{
		var (formatter, console) = CreateTestFormatter();

		formatter.WriteWarning("This is a warning");
		var output = console.Output;

		Assert.Contains("This is a warning", output);
		Assert.Contains("⚠", output);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteInfo_OutputsMessage()
	{
		var (formatter, console) = CreateTestFormatter();

		formatter.WriteInfo("Information message");
		var output = console.Output;

		Assert.Contains("Information message", output);
		Assert.Contains("ℹ", output);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteError_IncludesErrorCode()
	{
		var (formatter, console) = CreateTestFormatter();

		var error = new ErrorResult
		{
			Code = ErrorCodes.DeviceNotFound,
			Category = "tool",
			Message = "Device not found"
		};

		formatter.WriteError(error);
		var output = console.Output;

		Assert.Contains("E1006", output);
		Assert.Contains("Device not found", output);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteDoctorReport_FormatsCorrectly()
	{
		var (formatter, console) = CreateTestFormatter();

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
		var output = console.Output;

		Assert.Contains(".NET SDK", output);
		Assert.Contains("JDK", output);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteDeviceList_FormatsTable()
	{
		var (formatter, console) = CreateTestFormatter();

		var result = new DeviceListResult
		{
			Devices = new List<Device>
			{
				new Device
				{
					Name = "Pixel 6",
					Id = "emulator-5554",
					Platforms = new[] { "android" },
					Type = DeviceType.Emulator,
					State = DeviceState.Booted,
					IsEmulator = true,
					IsRunning = true
				}
			}
		};

		formatter.WriteResult(result);
		var output = console.Output;

		Assert.Contains("Pixel 6", output);
		Assert.Contains("emulator-5554", output);
		Assert.Contains("android", output);
	}

	[Fact]
	public void SpectreOutputFormatter_WriteTable_FormatsColumns()
	{
		var (formatter, console) = CreateTestFormatter();

		var items = new[] { ("Apple", "Fruit"), ("Carrot", "Vegetable") };
		formatter.WriteTable(items,
			("Name", i => i.Item1),
			("Category", i => i.Item2));

		var output = console.Output;

		Assert.Contains("Name", output);
		Assert.Contains("Category", output);
		Assert.Contains("Apple", output);
		Assert.Contains("Carrot", output);
	}
}
