using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class DevicePerformanceTests : BaseTestFixture
	{
		public DevicePerformanceTests()
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo
			{
				Model = "Test device",
				VersionString = "1.0",
				DeviceType = DeviceType.Virtual
			});
		}

		[Fact]
		public void CorrectnessDoesNotDefaultToSuccess()
		{
			Assert.False(new DevicePerformanceCorrectness().Passed);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ReportingUsesVerificationAfterCounterFinalization(bool mismatch)
		{
			var counters = new Dictionary<string, double> { ["completed"] = 0 };
			int verificationCalls = 0;
			var result = await DevicePerformanceMeasurement.MeasureAsync(
				"test-scenario", 2, 3,
				_ =>
				{
					counters["completed"]++;
					return Task.CompletedTask;
				},
				counters,
				_ =>
				{
					verificationCalls++;
					return Task.CompletedTask;
				});

			Assert.Equal(5, verificationCalls);
			Assert.Equal(5, result.Counters["completed"]);
			Assert.Equal(3, result.MeasurementsMilliseconds.Length);
			Assert.False(result.Correctness.Passed);

			result.Counters["lateMismatch"] = mismatch ? 1 : 0;
			var previousOutput = Console.Out;
			var previousFile = Environment.GetEnvironmentVariable("MAUI_PERF_RESULT_FILE");
			using var output = new StringWriter(CultureInfo.InvariantCulture);
			try
			{
				Environment.SetEnvironmentVariable("MAUI_PERF_RESULT_FILE", null);
				Console.SetOut(output);
				DevicePerformanceReporter.Write(result, result.Counters["lateMismatch"] == 0);
			}
			finally
			{
				Console.SetOut(previousOutput);
				Environment.SetEnvironmentVariable("MAUI_PERF_RESULT_FILE", previousFile);
			}

			Assert.Equal(!mismatch, result.Correctness.Passed);
			var record = output.ToString().Trim();
			Assert.StartsWith(DevicePerformanceReporter.ResultPrefix, record, StringComparison.Ordinal);
			using var json = JsonDocument.Parse(record.Substring(DevicePerformanceReporter.ResultPrefix.Length));
			Assert.Equal(!mismatch, json.RootElement.GetProperty("correctness").GetProperty("passed").GetBoolean());
			Assert.Equal(mismatch ? 1 : 0, json.RootElement.GetProperty("counters").GetProperty("lateMismatch").GetDouble());
			Assert.Equal(2, json.RootElement.GetProperty("warmupCount").GetInt32());
		}

		[Fact]
		public async Task VerificationExceptionsDoNotProduceSuccessfulMeasurements()
		{
			await Assert.ThrowsAsync<InvalidOperationException>(() =>
				DevicePerformanceMeasurement.MeasureAsync(
					"test-scenario", 0, 1,
					_ => Task.CompletedTask,
					verification: _ => Task.FromException(new InvalidOperationException("Verification failed."))));
		}

		[Theory]
		[InlineData("en-US", "d", "t")]
		[InlineData("de-DE", "dd MMM yyyy", "HH:mm")]
		public void PickerVerificationRejectsStaleNonEmptyValues(string culture, string dateFormat, string timeFormat)
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
			var date = new DateTime(2025, 1, 2);
			var time = TimeSpan.FromHours(2);
			var dateText = date.ToString(dateFormat);
			var timeText = time.ToFormattedString(timeFormat);

			Assert.True(DevicePerformanceVerification.PickerTextMatches(
				date, dateFormat, time, timeFormat, dateText, timeText));
			Assert.False(DevicePerformanceVerification.PickerTextMatches(
				date, dateFormat, time, timeFormat, date.AddDays(-1).ToString(dateFormat), timeText));
			Assert.False(DevicePerformanceVerification.PickerTextMatches(
				date, dateFormat, time, timeFormat, dateText, time.Add(TimeSpan.FromMinutes(-15)).ToFormattedString(timeFormat)));
		}

		[Fact]
		public void PickerVerificationHandlesClearedValues()
		{
			Assert.True(DevicePerformanceVerification.PickerTextMatches(
				null, "d", null, "t", string.Empty, string.Empty));
			Assert.False(DevicePerformanceVerification.PickerTextMatches(
				null, "d", null, "t", "stale date", "stale time"));
		}
	}
}
