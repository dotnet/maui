#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
#if MACCATALYST
using UIKit;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class Issue35512 : ControlsHandlerTestBase
	{
		const string IssueNumber = "35512";

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
		public async Task ResettingBackgroundColorRestoresNativeDefault()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

#if MACCATALYST
			await VerifyBackgroundColorReset();
#endif
		}

#if MACCATALYST
		async Task VerifyBackgroundColorReset()
		{
			var button = new Button();
			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			var restoredNativeDefault = await InvokeOnMainThreadAsync(() =>
			{
				using var nativeDefaultButton = new UIButton(UIButtonType.System);
				var nativeDefaultBackground = (
					nativeDefaultButton.Configuration?.BaseBackgroundColor,
					nativeDefaultButton.BackgroundColor);
				button.BackgroundColor = Colors.Red;
				button.BackgroundColor = null;

				var restoredBackground = (
					handler.PlatformView.Configuration?.BaseBackgroundColor,
					handler.PlatformView.BackgroundColor);

				return Equals(nativeDefaultBackground, restoredBackground);
			});

			Assert.True(
				restoredNativeDefault,
				"Resetting BackgroundColor to null should restore the native default button background.");
		}
#endif
	}
}
