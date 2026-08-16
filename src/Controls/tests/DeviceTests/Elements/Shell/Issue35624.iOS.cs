#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35624 : ControlsHandlerTestBase
	{
		const string IssueNumber = "35624";

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
		public async Task SearchHandlerAppliesCharacterSpacing()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
				});
			});

			var page = new ContentPage();
			var searchHandler = new SearchHandler
			{
				Query = "SPACING",
				CharacterSpacing = 12,
				SearchBoxVisibility = SearchBoxVisibility.Expanded
			};

			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items =
				{
					new ShellContent
					{
						Content = page
					}
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnLoadedAsync(page);

				UISearchBar? nativeSearchBar = null;
				await AssertEventually(() =>
				{
					nativeSearchBar = handler.ViewController.View.FindDescendantView<UISearchBar>();
					return nativeSearchBar != null;
				});

				Assert.NotNull(nativeSearchBar);
				var actual = nativeSearchBar.SearchTextField.AttributedText.GetCharacterSpacing();

				Assert.True(
					Math.Abs(actual - searchHandler.CharacterSpacing) < 0.01,
					$"SearchHandler native text should apply CharacterSpacing=12. Expected: {searchHandler.CharacterSpacing}; Actual: {actual}");
			});
		}
	}
}
