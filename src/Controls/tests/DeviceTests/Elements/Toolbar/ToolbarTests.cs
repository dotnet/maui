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
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
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

#if !IOS && !MACCATALYST
		[Theory(DisplayName = "Toolbar Recreates With New MauiContext")]
		[InlineData(typeof(FlyoutPage))]
		[InlineData(typeof(NavigationPage))]
		[InlineData(typeof(TabbedPage))]
		[InlineData(typeof(Shell))]
		public async Task ToolbarRecreatesWithNewMauiContext(Type type)
		{
			SetupBuilder();
			Page page = null;

			if (type == typeof(FlyoutPage))
			{
				page = new FlyoutPage()
				{
					Detail = new NavigationPage(new ContentPage() { Title = "Detail" }),
					Flyout = new ContentPage() { Title = "Flyout" }
				};
			}
			else if (type == typeof(NavigationPage))
			{
				page = new NavigationPage(new ContentPage() { Title = "Nav Page" });
			}
			else if (type == typeof(TabbedPage))
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
			else if (type == typeof(Shell))
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
