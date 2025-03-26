using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Devices;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using System.Diagnostics.CodeAnalysis;
using Xunit;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;

#endif

#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Compatibility;
#endif

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.Window)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class WindowTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);

#if ANDROID || WINDOWS
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
#else
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					handlers.AddHandler(typeof(FlyoutPage), typeof(PhoneFlyoutPageRenderer));
#endif

					handlers.AddHandler<IContentView, ContentViewHandler>();

					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Editor, EditorHandler>();
					handlers.AddHandler<SearchBar, SearchBarHandler>();
				});
			});
		}

#if !IOS
		[Theory]
		[ClassData(typeof(ChangingToNewMauiContextDoesntCrashTestCases))]
		public async Task ChangingToNewMauiContextDoesntCrash(bool useAppMainPage, [DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type rootPageType)
		{
			SetupBuilder();
			IWindow window;
			var rootPage = (Page)Activator.CreateInstance(rootPageType);

			if (useAppMainPage)
			{
				var app = ApplicationServices.GetService<IApplication>() as ApplicationStub;

#pragma warning disable CS0618 // Type or member is obsolete
				await InvokeOnMainThreadAsync(() => app.MainPage = rootPage);
#pragma warning restore CS0618 // Type or member is obsolete
				window = await InvokeOnMainThreadAsync(() => (app as IApplication).CreateWindow(null));

			}
			else
				window = await InvokeOnMainThreadAsync(() => new Window(rootPage));

			var mauiContextStub1 = new ContextStub(ApplicationServices);
#if ANDROID
			var activity = mauiContextStub1.GetActivity();
			mauiContextStub1.Context = new global::Android.Views.ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);
#endif
			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				if (rootPage is IPageContainer<Page> pc)
				{
					await OnLoadedAsync(pc.CurrentPage);
					await OnNavigatedToAsync(pc.CurrentPage);
				}

				await Task.Delay(100);

			}, mauiContextStub1);

			var mauiContextStub2 = new ContextStub(ApplicationServices);

#if ANDROID
			mauiContextStub2.Context = new global::Android.Views.ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);
#endif
			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				if (rootPage is IPageContainer<Page> pc)
				{
					await OnLoadedAsync(pc.CurrentPage);
					await OnNavigatedToAsync(pc.CurrentPage);
				}

				await Task.Delay(100);

			}, mauiContextStub2);
		}
#endif


		[Theory]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task NavigatedEventsFireOnEachIncomingAndOutGoingPage(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new Window(firstRootPage);

			bool hasNavigatedToFired = false;
			bool hasNavigatedFromFired = false;
			bool hasNavigatingFromFired = false;
			Page previousPage = null;

			EventHandler<NavigatedToEventArgs> NavigatedToHandler = null;			
			NavigatedToHandler = (s, args) =>
			{
				((Page)s).NavigatedTo -= NavigatedToHandler;
				hasNavigatedToFired = true;
				Assert.Equal(window.Page, s);
				Assert.Equal(previousPage, args.PreviousPage);
			};

			EventHandler<NavigatedFromEventArgs> NavigatedFromHandler = null;			
			NavigatedFromHandler = (s, args) =>
			{
				((Page)s).NavigatedFrom -= NavigatedFromHandler;
				hasNavigatedFromFired = true;

				// if the destination page is null that means this page is being unloaded
				if (args.DestinationPage is null)
				{
					Assert.Equal(window.Page, s);
				}
				else
				{
					Assert.NotEqual(window.Page, s);
					Assert.Equal(window.Page, args.DestinationPage);
				}
			};

			EventHandler<NavigatingFromEventArgs> NavigatingFromHandler = null;			
			NavigatingFromHandler = (s, args) =>
			{
				Assert.False(hasNavigatedToFired);
				((Page)s).NavigatingFrom -= NavigatingFromHandler;
				hasNavigatingFromFired = true;
				Assert.Equal(window.Page, s);
			};

			firstRootPage.NavigatedTo += NavigatedToHandler;
			firstRootPage.NavigatedFrom += NavigatedFromHandler;
			firstRootPage.NavigatingFrom += NavigatingFromHandler;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(swapOrder.Page);

				// Validate initial has Navigated To has fired
				Assert.True(hasNavigatedToFired);
				Assert.False(hasNavigatedFromFired);
				hasNavigatedToFired = false;

				while (!swapOrder.IsFinished())
				{
					var previousRootPage = window.Page?.GetType();
					var nextRootPage = swapOrder.GetNextPageType();
					previousPage = window.Page;

					// Wire up the incoming page to navigation events
					nextRootPage.NavigatedTo += NavigatedToHandler;
					nextRootPage.NavigatedFrom += NavigatedFromHandler;
					nextRootPage.NavigatingFrom += NavigatingFromHandler;

					window.Page = nextRootPage;

					try
					{
						await OnLoadedAsync(swapOrder.Page);
						Assert.True(hasNavigatedToFired);
						Assert.True(hasNavigatedFromFired);
						Assert.True(hasNavigatingFromFired);
						hasNavigatedToFired = false;
						hasNavigatedFromFired = false;
						hasNavigatingFromFired = false;
						
						// Disconnect the previous page from navigation events
						previousPage.NavigatedTo -= NavigatedToHandler;
						previousPage.NavigatedFrom -= NavigatedFromHandler;
						previousPage.NavigatingFrom -= NavigatingFromHandler;
					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage} from {previousRootPage}", exc);
					}
				}
			});
		}

		[Theory]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task MainPageSwapTests(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new Window(firstRootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(swapOrder.Page);
				while (!swapOrder.IsFinished())
				{
					var previousRootPage = window.Page?.GetType();
					var nextRootPage = swapOrder.GetNextPageType();
					window.Page = nextRootPage;

					try
					{
						await OnLoadedAsync(swapOrder.Page);

#if !IOS && !MACCATALYST

						var toolbar = GetToolbar(handler);

						// Shell currently doesn't create the handler on the xplat toolbar with Android
						// Because Android has lots of toolbars spread out between the viewpagers that
						var platformToolBar = GetPlatformToolbar(handler);
						Assert.Equal(platformToolBar != null, toolbar != null);

						if (platformToolBar != null)
						{
							if (DeviceInfo.Current.Platform == DevicePlatform.WinUI ||
								window.Page is not Shell)
							{
								Assert.Equal(toolbar?.Handler?.PlatformView, platformToolBar);
							}

							Assert.True(IsNavigationBarVisible(handler));
						}
#endif

					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage} from {previousRootPage}", exc);
					}
				}
			});
		}

#if !IOS && !MACCATALYST
		// Automated Shell tests are currently broken via xharness
		[Fact(DisplayName = "Toolbar Items Update when swapping out Main Page on Handler")]
		public async Task ToolbarItemsUpdateWhenSwappingOutMainPageOnHandler()
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem() { Text = "Toolbar Item 1" };
			var firstPage = new ContentPage();

			var window = new Window(firstPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var contentPage = new ContentPage()
				{
					ToolbarItems =
					{
						toolbarItem
					}
				};

				var shell = new Shell() { CurrentItem = contentPage };
				window.Page = shell;


				await OnLoadedAsync(shell);
				await OnLoadedAsync(shell.CurrentPage);

				ToolbarItemsMatch(handler, toolbarItem);
			});
		}
#endif

		[Fact(DisplayName = "Initial Dispatch from Background Thread Succeeds")]
		public async Task InitialDispatchFromBackgroundThreadSucceeds()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.Services.RemoveAll<IDispatcher>();
				builder.ConfigureDispatching();
			});

			var firstPage = new ContentPage();
			var window = new Window(firstPage);
			bool passed = true;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await Task.Run(async () =>
				{
					await firstPage.Handler.MauiContext.Services.GetRequiredService<IDispatcher>()
						.DispatchAsync(() => passed = true);
				});
			});

			Assert.True(passed);
		}

		[Fact]
		public async Task WindowIsActivedRespondToMethodsCall()
		{
			SetupBuilder();
			var page = new ContentPage();
			var window = new Window(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, (h) =>
			{
				var w = h.VirtualView;

				Assert.True(window.IsActivated);

				w.Deactivated();

				Assert.False(window.IsActivated);
			});
		}

		[Fact]
		public async Task SwitchBetweenWindowShouldTriggerIsActivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var app = ApplicationServices.GetService<IApplication>() as ApplicationStub;

			var window1 = new Window(page);
			var window2 = new Window(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window1, (h) =>
			{
				app.OpenWindow(window1);	
				Assert.True(window1.IsActivated);
				Assert.False(window2.IsActivated);
			});

			
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window2, (h) =>
			{
				app.OpenWindow(window2);

				Assert.False(window1.IsActivated);
				Assert.True(window2.IsActivated);
			});

			app.CloseWindow(window2);
			app.CloseWindow(window1);
			
			Assert.False(window1.IsActivated);
			Assert.False(window2.IsActivated);
		}
	}
}
