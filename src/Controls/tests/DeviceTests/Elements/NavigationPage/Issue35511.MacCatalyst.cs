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
		public async Task BackButtonTitleIsVisibleWithCustomTitleView()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				"35511",
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
				});
			});

			var rootPage = new ContentPage { Title = "Root page" };
			NavigationPage.SetBackButtonTitle(rootPage, "Main");

			var pushedPage = new ContentPage();
			NavigationPage.SetTitleView(pushedPage, new Label { Text = "Custom title" });

			var navigationPage = new NavigationPage(rootPage);
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(
				new Window(navigationPage),
				async handler =>
				{
					await navigationPage.PushAsync(pushedPage);
					await OnNavigatedToAsync(pushedPage);

					var actual = GetBackButtonText(handler);
					Assert.True(
						string.Equals("Main", actual, StringComparison.Ordinal),
						$"Expected the rendered back button title to be 'Main'. Actual: '{actual ?? "<null>"}'.");
				});
		}
	}
}
