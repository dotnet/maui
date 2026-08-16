#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CarouselView)]
	public class Issue36230 : ControlsHandlerTestBase
	{
		const string IssueNumber = "36230";

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
		public async Task CurrentItemRemainsReplacementAfterReplacingSelectedItem()
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
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var items = new ObservableCollection<string>
			{
				"1",
				"2",
				"3"
			};
			const string replacement = "2b";
			var carouselView = new CarouselView
			{
				CurrentItem = items[1],
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = items,
				Loop = false
			};

			items.CollectionChanged += (_, _) => carouselView.CurrentItem = replacement;

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carouselView, async handler =>
			{
				await handler.Controller.CollectionView.PerformBatchUpdatesAsync(() => { });

				items[1] = replacement;

				await handler.Controller.CollectionView.PerformBatchUpdatesAsync(() => { });

				Assert.True(
					ReferenceEquals(replacement, carouselView.CurrentItem),
					"CarouselView.CurrentItem should remain the replacement item after replacing the selected item.");
			});
		}
	}
}
