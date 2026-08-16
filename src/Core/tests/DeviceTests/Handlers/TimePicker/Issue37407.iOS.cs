#if !MACCATALYST
#nullable enable
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
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
		public async Task DefaultShortTimeFormatUsesCurrentCulture()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var actual = await InvokeOnMainThreadAsync(() =>
			{
				var originalCulture = CultureInfo.CurrentCulture;
				var originalUICulture = CultureInfo.CurrentUICulture;
				var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
				var originalDefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;

				try
				{
					var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
					CultureInfo.DefaultThreadCurrentCulture = frenchCulture;
					CultureInfo.DefaultThreadCurrentUICulture = frenchCulture;
					CultureInfo.CurrentCulture = frenchCulture;
					CultureInfo.CurrentUICulture = frenchCulture;

					var timePicker = new TimePickerStub
					{
						Format = "t",
						Time = new TimeSpan(7, 30, 0)
					};
					var handler = CreateHandler(timePicker);

					return handler.PlatformView.Text;
				}
				finally
				{
					CultureInfo.CurrentCulture = originalCulture;
					CultureInfo.CurrentUICulture = originalUICulture;
					CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
					CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUICulture;
				}
			});

			Assert.True(
				string.Equals("07:30", actual, StringComparison.Ordinal),
				"Issue 37407 expected French short time '07:30' instead of an en-US 12-hour value.");
		}
	}
}
#endif
