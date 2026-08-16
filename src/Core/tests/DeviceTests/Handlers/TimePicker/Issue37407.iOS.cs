#nullable enable annotations
#if !MACCATALYST
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public class Issue37407 : CoreHandlerTestBase<TimePickerHandler, TimePickerStub>
	{
		const string IssueNumber = "37407";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task DefaultFormatRespectsCurrentCulture()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var testCulture = new CultureInfo("fr-FR");
			var previousCulture = CultureInfo.CurrentCulture;
			var previousUICulture = CultureInfo.CurrentUICulture;

			try
			{
				CultureInfo.CurrentCulture = testCulture;
				CultureInfo.CurrentUICulture = testCulture;

				var time = new TimeSpan(7, 30, 0);
				var timePicker = new TimePickerStub
				{
					Format = "t",
					Time = time
				};

				var actual = await GetValueAsync(timePicker, handler => handler.PlatformView.Text);
				var expected = new DateTime(1, 1, 1).Add(time).ToString("t", testCulture);

				Assert.True(
					string.Equals(expected, actual, StringComparison.Ordinal),
					"Default TimePicker format should respect the fr-FR short time pattern.");
			}
			finally
			{
				CultureInfo.CurrentCulture = previousCulture;
				CultureInfo.CurrentUICulture = previousUICulture;
			}
		}
	}
}
#endif
