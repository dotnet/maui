#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35511 : ControlsHandlerTestBase
	{
		const string IssueNumber = "35511";

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
		public async Task BackButtonTitleRemainsVisibleWithCustomTitleView()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			const string ExpectedBackButtonTitle = "Main";

			if (!OperatingSystem.IsMacCatalyst())
				return;

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
				});
			});

			var rootPage = new ContentPage();
			NavigationPage.SetBackButtonTitle(rootPage, ExpectedBackButtonTitle);

			var detailsPage = new ContentPage();
			NavigationPage.SetTitleView(detailsPage, new Label { Text = "Details Title" });

			var navigationPage = new NavigationPage(rootPage);
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(
				new Window(navigationPage),
				async handler =>
				{
					await navigationPage.PushAsync(detailsPage);
					await OnNavigatedToAsync(detailsPage);

					var actualBackButtonTitle = GetBackButtonText(handler);

					Assert.True(
						string.Equals(
							ExpectedBackButtonTitle,
							actualBackButtonTitle,
							StringComparison.Ordinal),
						"Expected the Mac Catalyst back button title to remain visible as 'Main'.");
				});
		}
	}
}
