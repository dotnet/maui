using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.DeviceTests.TestCases;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Toolbar)]
	public partial class ToolbarTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.Label), typeof(LabelHandler));
					handlers.AddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Controls.Window, WindowHandlerStub>();
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));

					SetupShellHandlers(handlers);
				});
			});
		}


#if !IOS && !MACCATALYST
		[Fact(DisplayName = "Toolbar Items Map Correctly")]
		public async Task ToolbarItemsMapCorrectly()
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem() { Text = "Toolbar Item 1" };
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					toolbarItem
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				ToolbarItemsMatch(handler, toolbarItem);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Items Order Updates Correctly After Navigation")]
		public async Task ToolbarItemsOrderUpdatesCorrectlyAfterNavigation()
		{
			SetupBuilder();
			var toolbarItemFirstPage = new ToolbarItem() { Text = "Toolbar Item 1" };
			var toolbarItemSecondPage = new ToolbarItem() { Text = "Toolbar Item Second Page", Order = ToolbarItemOrder.Secondary };

			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					toolbarItemFirstPage
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(new ContentPage()
				{
					ToolbarItems =
					{
						toolbarItemSecondPage
					}
				});

				ToolbarItemsMatch(handler, GetExpectedToolbarItems(navPage));
				ToolbarItemsMatch(handler, toolbarItemSecondPage);
				await navPage.PopAsync();
				ToolbarItemsMatch(handler, GetExpectedToolbarItems(navPage));
				ToolbarItemsMatch(handler, toolbarItemFirstPage);
			});
		}
#endif

		[Fact(DisplayName = "Toolbar Title")]
		public async Task ToolbarTitle()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				string title = GetToolbarTitle(handler);
				Assert.Equal("Page Title", title);
				return Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(ContentPage)}, {nameof(FlyoutPage)}WithNavigationPage")]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(FlyoutPage)}, {nameof(FlyoutPage)}WithNavigationPage")]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(NavigationPage)}, {nameof(FlyoutPage)}WithNavigationPage")]
		[InlineData($"{nameof(Shell)}, {nameof(ContentPage)}, {nameof(Shell)}")]
		[InlineData($"FlyoutPageWithNavigationPage, NavigationPageWithFlyoutPage, FlyoutPageWithNavigationPage")]
		public async Task ToolbarUpdatesCorrectlyWhenSwappingMainPageWithAlreadyUsedPage(string pages)
		{
			string[] pageSet = pages.Split(',');

			SetupBuilder();
			Dictionary<ControlsPageTypesTestCase, Page> createdPages
				= new Dictionary<ControlsPageTypesTestCase, Page>();

			var nextPage = GetPage(pageSet[0]);
			var window = new Window(nextPage);


			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(window.Page);

				for (int i = 1; i < pageSet.Length; i++)
				{
					nextPage = GetPage(pageSet[i]);
					window.Page = nextPage;

					var currentPage = window.Page;

					currentPage = Controls.Platform.PageExtensions.GetCurrentPage(currentPage);

					await OnLoadedAsync(currentPage);

					var shouldHaveToolbar =
						pageSet[i].Contains("NavigationPage", StringComparison.OrdinalIgnoreCase) ||
						pageSet[i].Contains("Shell", StringComparison.OrdinalIgnoreCase);

					await AssertionExtensions.Wait(() => shouldHaveToolbar == IsNavigationBarVisible(currentPage.Handler));
					Assert.Equal(shouldHaveToolbar, IsNavigationBarVisible(currentPage.Handler));
				}
			});

			Page GetPage(string name)
			{
				var result = (ControlsPageTypesTestCase)Enum.Parse(typeof(ControlsPageTypesTestCase), name);

				if (!createdPages.ContainsKey(result))
					createdPages[result] = ControlsPageTypesTestCases.CreatePageType(result, new ContentPage()
					{
						Title = "Page Title",
						Content = new VerticalStackLayout()
						{
							new Label()
							{
								Text = "ToolbarUpdatesCorrectlyWhenSwappingMainPageWithAlreadyUsedPage"
							}
						}
					});

				return createdPages[result];
			}
		}

#if !IOS && !MACCATALYST
		[Theory(DisplayName = "Toolbar Recreates With New MauiContext")]
		[InlineData(nameof(FlyoutPage))]
		[InlineData(nameof(NavigationPage))]
		[InlineData(nameof(TabbedPage))]
		[InlineData(nameof(Shell))]
		public async Task ToolbarRecreatesWithNewMauiContext(string type)
		{
			SetupBuilder();
			Page page = null;

			if (type == nameof(FlyoutPage))
			{
				page = new FlyoutPage()
				{
					Detail = new NavigationPage(new ContentPage() { Title = "Detail" }),
					Flyout = new ContentPage() { Title = "Flyout" }
				};
			}
			else if (type == nameof(NavigationPage))
			{
				page = new NavigationPage(new ContentPage() { Title = "Nav Page" });
			}
			else if (type == nameof(TabbedPage))
			{
				page = new TabbedPage()
				{
					Children =
					{
						new NavigationPage(new ContentPage() { Title = "Tab Page 1" }),
						new NavigationPage(new ContentPage() { Title = "Tab Page 2" })
					}
				};
			}
			else if (type == nameof(Shell))
			{
				page = new Shell() { CurrentItem = new ContentPage() { Title = "Shell Page" } };
			}


			var window = new Window(page);

			var context1 = ContextStub.CreateNew(MauiContext);
			var context2 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, (handler) =>
			{
				var toolbar = GetToolbar(handler);
				Assert.NotNull(toolbar);
				Assert.True(IsNavigationBarVisible(handler));
				return Task.CompletedTask;
			}, context1);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, (handler) =>
			{
				var toolbar = GetToolbar(handler);
				Assert.NotNull(toolbar);
				Assert.True(IsNavigationBarVisible(handler));
				return Task.CompletedTask;
			}, context2);
		}
#endif

		ToolbarItem[] GetExpectedToolbarItems(NavigationPage navPage)
		{
			return ((Toolbar)(navPage.Window as IToolbarElement).Toolbar).ToolbarItems.ToArray();
		}
	}
}
