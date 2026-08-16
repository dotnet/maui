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
		public async Task ReplacingSelectedItemPreservesCurrentItem()
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

			var items = new ObservableCollection<string> { "1", "2", "3" };
			var carouselView = new CarouselView
			{
				CurrentItem = "2",
				HeightRequest = 160,
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = items,
				Loop = false,
				Position = 1
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carouselView, handler =>
			{
				handler.Controller.CollectionView.LayoutIfNeeded();

				carouselView.CurrentItem = "2b";
				items[1] = "2b";

				var actual = carouselView.CurrentItem as string ?? "(null)";
				Assert.True(
					string.Equals("2b", actual, StringComparison.Ordinal),
					$"Expected CurrentItem to remain 2b, but was {actual}.");

				return Task.CompletedTask;
			});
		}
	}
}
