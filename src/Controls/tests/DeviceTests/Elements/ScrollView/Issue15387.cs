#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue15387 : ControlsHandlerTestBase
	{
		const string IssueNumber = "15387";

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
		public async Task ScrollToAsyncCompletesDuringOnAppearing()
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
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var itemsLayout = new VerticalStackLayout();
			BindableLayout.SetItemTemplate(itemsLayout, new DataTemplate(() => new Label()));
			BindableLayout.SetItemsSource(itemsLayout, new[]
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5",
			});

			var scrollView = new ScrollView
			{
				Content = itemsLayout,
			};
			var page = new ReproductionPage
			{
				Content = scrollView,
				ScrollView = scrollView,
				ScrollCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously),
			};
			bool completed = false;

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				try
				{
					await page.ScrollCompleted.Task.WaitAsync(TimeSpan.FromSeconds(3));
					completed = true;
				}
				catch (TimeoutException)
				{
					completed = false;
				}
			});

			Assert.True(completed, "ScrollToAsync should complete when invoked during OnAppearing.");
		}

		sealed class ReproductionPage : ContentPage
		{
			public required ScrollView ScrollView { get; init; }

			public required TaskCompletionSource ScrollCompleted { get; init; }

			protected override async void OnAppearing()
			{
				base.OnAppearing();

				await ScrollView.ScrollToAsync(0, 0, false);
				ScrollCompleted.TrySetResult();
			}
		}
	}
}
