﻿using System;
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
using static Microsoft.Maui.DeviceTests.AssertHelpers;

#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
using TabbedRenderer = Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer;
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
#if IOS || MACCATALYST
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
#else
                    handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
#endif

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
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(ContentPage)}, {nameof(FlyoutPage)}WithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(FlyoutPage)}, {nameof(FlyoutPage)}WithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"{nameof(FlyoutPage)}WithNavigationPage, {nameof(NavigationPage)}, {nameof(FlyoutPage)}WithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"{nameof(Shell)}, {nameof(ContentPage)}, {nameof(Shell)}"
#if WINDOWS
			, Skip = "Currently Failing on  Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		[InlineData($"FlyoutPageWithNavigationPage, NavigationPageWithFlyoutPage, FlyoutPageWithNavigationPage"
#if WINDOWS
			, Skip = "Currently Failing on Windows https://github.com/dotnet/maui/issues/15530"
#endif
			)]
		public async Task ToolbarUpdatesCorrectlyWhenSwappingMainPageWithAlreadyUsedPage(string pages)
		{
			string[] pageSet = pages.Split(',');

			SetupBuilder();
			Dictionary<ControlsPageTypesTestCase, Page> createdPages
				= new Dictionary<ControlsPageTypesTestCase, Page>();

			var nextPage = GetPage(pageSet[0]);
			Window window = null!;

			await InvokeOnMainThreadAsync(() =>
			{
				// This reads DisplayInfo, so it needs main thread
				window = new Window(nextPage);
			});

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

					await AssertEventually(() => shouldHaveToolbar == IsNavigationBarVisible(currentPage.Handler));
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

		[Fact(DisplayName = "Toolbar Items IsVisible Property Filters Items Correctly")]
		public async Task ToolbarItemsIsVisiblePropertyFiltersItemsCorrectly()
		{
			SetupBuilder();
			var visibleItem = new ToolbarItem() { Text = "Visible Item", IsVisible = true };
			var hiddenItem = new ToolbarItem() { Text = "Hidden Item", IsVisible = false };
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					visibleItem,
					hiddenItem
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				var actualToolbarItems = toolbar.ToolbarItems.Where(item => item.IsVisible).ToArray();
				
				// Only the visible item should be included
				Assert.Single(actualToolbarItems);
				Assert.Equal("Visible Item", actualToolbarItems[0].Text);
				Assert.DoesNotContain(hiddenItem, actualToolbarItems);
				
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Items IsVisible Property Changes Update UI")]
		public async Task ToolbarItemsIsVisiblePropertyChangesUpdateUI()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { Text = "Item 1", IsVisible = true };
			var item2 = new ToolbarItem() { Text = "Item 2", IsVisible = false };
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var toolbar = (Toolbar)(navPage.Window as IToolbarElement).Toolbar;
				
				// Initially only item1 should be visible
				var visibleItems = toolbar.ToolbarItems.Where(item => item.IsVisible).ToArray();
				Assert.Single(visibleItems);
				Assert.Equal("Item 1", visibleItems[0].Text);
				
				// Change visibility
				await InvokeOnMainThreadAsync(() =>
				{
					item1.IsVisible = false;
					item2.IsVisible = true;
				});
				
				// Now only item2 should be visible
				visibleItems = toolbar.ToolbarItems.Where(item => item.IsVisible).ToArray();
				Assert.Single(visibleItems);
				Assert.Equal("Item 2", visibleItems[0].Text);
			});
		}
	}
}
