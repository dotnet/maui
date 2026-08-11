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
#if ANDROID
					handlers.AddHandler(typeof(ReentrantFlyoutPage), typeof(ReentrantFlyoutViewHandler));
#endif
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

#if ANDROID
		[Fact(DisplayName = "Replaced FlyoutPage Root Clears Old ContainerView")]
		public async Task ReplacedFlyoutPageRootClearsOldContainerView()
		{
			SetupBuilder();

			var rootPage = CreateFlyoutRoot();
			var window = new Window(rootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var oldRootView = Assert.IsType<ContainerView>(rootManager.RootView);

				Assert.Same(rootPage, oldRootView.CurrentView);
				Assert.NotNull(oldRootView.MainView);

				var replacementPage = new ContentPage
				{
					Content = new Label { Text = "Replacement page" }
				};

				window.Page = replacementPage;
				await OnLoadedAsync(replacementPage);

				Assert.Null(oldRootView.CurrentView);
				Assert.Null(oldRootView.MainView);
				Assert.NotNull(rootManager.RootView);
				Assert.NotSame(oldRootView, rootManager.RootView);
				AssertPageAttachedToRoot(replacementPage, rootManager);
			});
		}

		[Fact(DisplayName = "Replacing Shell Root While Switching Items Does Not Crash")]
		public async Task ReplacingShellRootWhileSwitchingItemsDoesNotCrash()
		{
			SetupBuilder();

			var firstPage = new ContentPage { Content = new Label { Text = "First item" } };
			var secondPage = new ContentPage { Content = new Label { Text = "Second item" } };
			var firstItem = new FlyoutItem { Title = "First", Items = { firstPage } };
			var secondItem = new FlyoutItem { Title = "Second", Items = { secondPage } };
			var shell = new Shell { Items = { firstItem, secondItem } };
			var window = new Window(shell);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(firstPage);

				shell.CurrentItem = secondItem;
				var replacementPage = new ContentPage
				{
					Content = new Label { Text = "Replacement page" }
				};

				// This ordering reproduced the missing-container crash on both Android
				// Mono and CoreCLR before NavigationRootManager drained the transaction.
				window.Page = replacementPage;

				await OnLoadedAsync(replacementPage);
				AssertPageAttachedToRoot(replacementPage, handler.MauiContext.GetNavigationRootManager());
			});
		}

		[Fact(DisplayName = "Replacing Shell Root From Page Loaded Does Not Crash")]
		public async Task ReplacingShellRootFromPageLoadedDoesNotCrash()
		{
			SetupBuilder();

			var shellPage = new ContentPage { Content = new Label { Text = "Shell page" } };
			var replacementPage = new ContentPage
			{
				Content = new Label { Text = "Replacement page" }
			};
			var shell = new Shell { CurrentItem = shellPage };
			var window = new Window(shell);

			shellPage.Loaded += OnShellPageLoaded;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(replacementPage);
				AssertPageAttachedToRoot(replacementPage, handler.MauiContext.GetNavigationRootManager());
			});

			void OnShellPageLoaded(object sender, EventArgs e)
			{
				shellPage.Loaded -= OnShellPageLoaded;
				window.Page = replacementPage;
			}
		}

		[Fact(DisplayName = "Replacing Flyout Root From Detail Loaded Does Not Crash")]
		public async Task ReplacingFlyoutRootFromDetailLoadedDoesNotCrash()
		{
			SetupBuilder();

			var detailPage = new ContentPage
			{
				Title = "Detail",
				Content = new Label { Text = "Detail page" }
			};
			var rootPage = new FlyoutPage
			{
				Flyout = new ContentPage { Title = "Flyout" },
				Detail = detailPage
			};
			var replacementPage = new ContentPage
			{
				Content = new Label { Text = "Replacement page" }
			};
			var window = new Window(rootPage);

			detailPage.Loaded += OnDetailPageLoaded;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(replacementPage);
				AssertPageAttachedToRoot(replacementPage, handler.MauiContext.GetNavigationRootManager());
			});

			void OnDetailPageLoaded(object sender, EventArgs e)
			{
				detailPage.Loaded -= OnDetailPageLoaded;
				window.Page = replacementPage;
			}
		}

		[Fact(DisplayName = "Replacing Content Root With Shell In Same Turn Does Not Crash")]
		public async Task ReplacingContentRootWithShellInSameTurnDoesNotCrash()
		{
			SetupBuilder();

			var window = new Window(new ContentPage());
			var shellPage = new ContentPage { Content = new Label { Text = "Shell page" } };
			var shell = new Shell { CurrentItem = shellPage };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				window.Page = new ContentPage { Content = new Label { Text = "Pending page" } };
				window.Page = shell;

				await OnLoadedAsync(shellPage);
				AssertPageAttachedToRoot(shellPage, handler.MauiContext.GetNavigationRootManager());
			});
		}

		[Fact(DisplayName = "Reentrant Root Replacement Does Not Clobber New Root")]
		public async Task ReentrantRootReplacementDoesNotClobberNewRoot()
		{
			SetupBuilder();

			var window = new Window(CreateFlyoutRoot());
			var finalPage = new ContentPage
			{
				Content = new Label { Text = "Final page" }
			};
			var replacingRoot = false;
			var reenteredDuringReplacement = false;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var oldRootView = Assert.IsType<ContainerView>(rootManager.RootView);
				var fragmentHost = new global::Android.Widget.FrameLayout(handler.MauiContext.Context)
				{
					Id = global::Android.Views.View.GenerateViewId()
				};
				oldRootView.AddView(fragmentHost);

				var fragmentManager = handler.MauiContext.Context.GetFragmentManager();
				var fragment = new ReentrantFragment(OnFragmentViewCreated);
				fragmentManager
					.BeginTransaction()
					.Add(fragmentHost.Id, fragment)
					.Commit();

				try
				{
					replacingRoot = true;
					window.Page = new ContentPage { Content = new Label { Text = "Intermediate page" } };
					replacingRoot = false;

					await OnLoadedAsync(finalPage);
					Assert.True(reenteredDuringReplacement);
					Assert.Same(finalPage, window.Page);
					Assert.Null(oldRootView.CurrentView);
					Assert.Null(oldRootView.MainView);
					Assert.Null(rootManager.DrawerLayout);
					Assert.Null(rootManager.ToolbarElement);
					AssertPageAttachedToRoot(finalPage, rootManager);
				}
				finally
				{
					fragmentManager
						.BeginTransaction()
						.Remove(fragment)
						.CommitAllowingStateLoss();
					fragmentManager.ExecutePendingTransactions();
					fragmentHost.RemoveFromParent();
				}
			});

			void OnFragmentViewCreated()
			{
				reenteredDuringReplacement |= replacingRoot;
				window.Page = finalPage;
			}
		}

		[Fact(DisplayName = "Root Replacement During Flyout Construction Publishes Latest Root")]
		public async Task RootReplacementDuringFlyoutConstructionPublishesLatestRoot()
		{
			SetupBuilder();

			var finalPage = new ContentPage
			{
				Title = "Final page",
				Content = new Label { Text = "Final page content" }
			};
			var finalRoot = new NavigationPage(finalPage);
			var initialRoot = new ContentPage();
			var reentrantRoot = new ReentrantFlyoutPage
			{
				Flyout = new ContentPage { Title = "Flyout" },
				Detail = new ContentPage { Title = "Detail" }
			};
			var window = new Window(initialRoot);
			var reenteredDuringConstruction = false;

			reentrantRoot.OnCreatingPlatformView = () =>
			{
				reenteredDuringConstruction = true;
				window.Page = finalRoot;
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				window.Page = reentrantRoot;

				await OnLoadedAsync(finalPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				Assert.True(reenteredDuringConstruction);
				Assert.Same(finalRoot, window.Page);
				Assert.Null(reentrantRoot.Handler);
				Assert.NotNull(rootManager.ToolbarElement);
				AssertPageAttachedToRoot(finalPage, rootManager);
			});
		}

		public sealed class ReentrantFragment : AndroidX.Fragment.App.Fragment
		{
			readonly Action _onCreateView;

			public ReentrantFragment()
			{
				_onCreateView = () => { };
			}

			public ReentrantFragment(Action onCreateView)
			{
				_onCreateView = onCreateView;
			}

			public override global::Android.Views.View OnCreateView(
				global::Android.Views.LayoutInflater inflater,
				global::Android.Views.ViewGroup container,
				global::Android.OS.Bundle savedInstanceState)
			{
				_onCreateView();
				return new global::Android.Views.View(inflater.Context);
			}
		}

		public sealed class ReentrantFlyoutPage : FlyoutPage
		{
			public Action OnCreatingPlatformView { get; set; }
		}

		public sealed class ReentrantFlyoutViewHandler : FlyoutViewHandler
		{
			protected override global::Android.Views.View CreatePlatformView()
			{
				var platformView = base.CreatePlatformView();
				((ReentrantFlyoutPage)VirtualView).OnCreatingPlatformView?.Invoke();
				return platformView;
			}
		}

		static void AssertPageAttachedToRoot(Page page, NavigationRootManager rootManager)
		{
			var rootView = rootManager.RootView;
			var platformView = page.ToPlatform();

			Assert.NotNull(rootView);
			Assert.NotNull(platformView);
			Assert.True(platformView.IsAttachedToWindow);

			for (global::Android.Views.View current = platformView; current is not null; current = current.Parent as global::Android.Views.View)
			{
				if (ReferenceEquals(current, rootView))
					return;
			}

			Assert.Fail("The page's platform view is not hosted by the navigation root.");
		}

		static FlyoutPage CreateFlyoutRoot()
		{
			var flyoutPage = new ContentPage { Title = "Flyout" };
			var detailPage = new ContentPage
			{
				Title = "Detail",
				Content = new Label { Text = "Detail page" }
			};
			var detailNavigationPage = new NavigationPage(detailPage) { Title = "Detail" };
			return new FlyoutPage
			{
				Flyout = flyoutPage,
				Detail = detailNavigationPage
			};
		}
#endif

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
