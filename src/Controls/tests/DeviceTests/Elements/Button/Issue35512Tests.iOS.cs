#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

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
		public async Task ResettingBackgroundColorRestoresPlatformDefault()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var platformDefaults = await InvokeOnMainThreadAsync(() =>
			{
				using var defaultButton = new UIButton(UIButtonType.System);
				return (defaultButton.BackgroundColor, defaultButton.Configuration?.BaseBackgroundColor);
			});

			var button = new Button();
			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await InvokeOnMainThreadAsync(() => button.BackgroundColor = Colors.Red);
			var redBackgroundColor = await InvokeOnMainThreadAsync(
				() => handler.PlatformView.Configuration?.BaseBackgroundColor ?? handler.PlatformView.BackgroundColor);
			Assert.Equal(UIColor.Red, redBackgroundColor);

			await InvokeOnMainThreadAsync(() => button.BackgroundColor = null);
			var resetBackgroundColors = await InvokeOnMainThreadAsync(
				() => (handler.PlatformView.BackgroundColor, handler.PlatformView.Configuration?.BaseBackgroundColor));

			Assert.True(
				Equals(platformDefaults.BackgroundColor, resetBackgroundColors.BackgroundColor) &&
				Equals(platformDefaults.BaseBackgroundColor, resetBackgroundColors.BaseBackgroundColor),
				"Button native background should restore its platform default after BackgroundColor is reset to null.");
		}
	}
}
