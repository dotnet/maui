using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue15387 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ScrollToAsyncCompletesWhenCalledFromInitialOnAppearing()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
					handlers.AddHandler<Page, PageHandler>();
				});
			});

			var itemsLayout = new VerticalStackLayout();
			BindableLayout.SetItemTemplate(itemsLayout, new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}));
			BindableLayout.SetItemsSource(itemsLayout, new[]
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5",
				"Item 6",
				"Item 7",
				"Item 8",
				"Item 9",
				"Item 10",
			});

			var scrollView = new ScrollView
			{
				Content = itemsLayout,
				HeightRequest = 240,
			};
			var page = new ScrollOnAppearingPage
			{
				Content = scrollView,
			};

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				await OnLoadedAsync(scrollView);
				Assert.NotNull(page.ScrollTask);

				try
				{
					await page.ScrollTask.WaitAsync(TimeSpan.FromSeconds(2));
				}
				catch (TimeoutException)
				{
					Assert.Fail("ScrollToAsync should complete after the ScrollView is loaded.");
				}
			});
		}

		sealed class ScrollOnAppearingPage : ContentPage
		{
			public Task ScrollTask { get; private set; }

			protected override async void OnAppearing()
			{
				base.OnAppearing();

				ScrollTask = ((ScrollView)Content).ScrollToAsync(0, 0, false);
				await ScrollTask;
			}
		}
	}
}
