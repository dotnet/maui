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
					Assert.Same(rootManager.RootView, handler.PlatformViewUnderTest);
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

		[Fact(DisplayName = "Root Replacement During Fragment Execution Is Deferred")]
		public async Task RootReplacementDuringFragmentExecutionIsDeferred()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var finalPage = new ContentPage { Content = new Label { Text = "Final page" } };
			var finalRoot = new NavigationPage(finalPage);
			var window = new Window(initialPage);
			var toolbar = new Toolbar(window);
			var replacementWasDeferred = false;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var outgoingRoot = Assert.IsAssignableFrom<global::Android.Views.ViewGroup>(rootManager.RootView);
				var fragmentHost = new global::Android.Widget.FrameLayout(handler.MauiContext.Context)
				{
					Id = global::Android.Views.View.GenerateViewId()
				};
				outgoingRoot.AddView(fragmentHost);

				var fragmentManager = handler.MauiContext.Context.GetFragmentManager();
				var fragment = new ReentrantFragment(() =>
				{
					handler.ConnectContent(finalRoot);
					window.Toolbar = toolbar;
					replacementWasDeferred = ReferenceEquals(outgoingRoot, rootManager.RootView);
					Assert.True(HasRootSwapRetryInfrastructure(rootManager));
				});
				fragmentManager
					.BeginTransaction()
					.Add(fragmentHost.Id, fragment)
					.Commit();

				try
				{
					fragmentManager.ExecutePendingTransactions();

					await OnLoadedAsync(finalPage);
					Assert.True(replacementWasDeferred);
					Assert.Same(toolbar, rootManager.ToolbarElement.Toolbar);
					Assert.Same(rootManager.RootView, handler.PlatformViewUnderTest);
					AssertPageAttachedToRoot(finalPage, rootManager);
					AssertPlatformViewAttachedToRoot(toolbar.ToPlatform(handler.MauiContext), rootManager);
					Assert.False(HasRootSwapRetryInfrastructure(rootManager));
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
		}

		[Fact(DisplayName = "Outgoing Toolbar Mapping During Root Drain Is Ignored")]
		public async Task OutgoingToolbarMappingDuringRootDrainIsIgnored()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var outgoingRoot = new NavigationPage(initialPage);
			var replacementPage = new ContentPage { Content = new Label { Text = "Replacement page" } };
			var window = new Window(outgoingRoot);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var outgoingPlatformRoot = Assert.IsAssignableFrom<global::Android.Views.ViewGroup>(rootManager.RootView);
				var outgoingToolbarElement = Assert.IsAssignableFrom<IToolbarElement>(rootManager.ToolbarElement);
				var fragmentHost = new global::Android.Widget.FrameLayout(handler.MauiContext.Context)
				{
					Id = global::Android.Views.View.GenerateViewId()
				};
				outgoingPlatformRoot.AddView(fragmentHost);

				var fragmentManager = handler.MauiContext.Context.GetFragmentManager();
				var fragment = new ReentrantFragment(() => rootManager.SetToolbarElement(outgoingToolbarElement));
				fragmentManager
					.BeginTransaction()
					.Add(fragmentHost.Id, fragment)
					.Commit();

				try
				{
					window.Page = replacementPage;

					await OnLoadedAsync(replacementPage);
					Assert.Null(rootManager.ToolbarElement);
					AssertPageAttachedToRoot(replacementPage, rootManager);
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
				Assert.Same(rootManager.RootView, handler.PlatformViewUnderTest);
				AssertPageAttachedToRoot(finalPage, rootManager);
			});
		}

		[Fact(DisplayName = "Superseded Root Request Does Not Report Applied")]
		public async Task SupersededRootRequestDoesNotReportApplied()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var supersededPage = new ContentPage { Content = new Label { Text = "Superseded page" } };
			var finalPage = new ContentPage { Content = new Label { Text = "Final page" } };
			var window = new Window(initialPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				bool? supersedingRequestApplied = null;
				var submittedRequestApplied = handler.ConnectContent(
					supersededPage,
					rootPrepared: () => supersedingRequestApplied = handler.ConnectContent(finalPage));

				Assert.False(submittedRequestApplied);
				Assert.False(supersedingRequestApplied);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				handler.MauiContext.GetFragmentManager().ExecutePendingTransactions();
				Assert.Null(supersededPage.Handler);
				Assert.Same(rootManager.RootView, handler.PlatformViewUnderTest);
				var finalPlatformView = Assert.IsAssignableFrom<global::Android.Views.View>(finalPage.Handler?.PlatformView);
				Assert.True(finalPlatformView.IsAttachedToWindow);
				AssertPlatformViewAttachedToRoot(finalPlatformView, rootManager);
			});
		}

		[Fact(DisplayName = "Reentrant Disconnect Detaches Discarded Prepared Root")]
		public async Task ReentrantDisconnectDetachesDiscardedPreparedRoot()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var replacementPage = new ContentPage { Content = new Label { Text = "Replacement page" } };
			var window = new Window(initialPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var initialRoot = handler.PlatformViewUnderTest;

				Assert.False(handler.ConnectContent(replacementPage, rootPrepared: rootManager.Disconnect));

				var discardedRoot = handler.PlatformViewUnderTest;
				Assert.NotSame(initialRoot, discardedRoot);
				Assert.Null(discardedRoot.Parent);
				Assert.Null(rootManager.RootView);
				Assert.Null(replacementPage.Handler);
			});
		}

		[Fact(DisplayName = "New Toolbar Element Reusing Outgoing Toolbar Wins During Root Drain")]
		public async Task NewToolbarElementReusingOutgoingToolbarWinsDuringRootDrain()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var replacementPage = new ContentPage { Content = new Label { Text = "Replacement page" } };
			var window = new Window(initialPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var toolbar = new Toolbar(window);
				var outgoingToolbarElement = new ToolbarElementStub(toolbar);
				var incomingToolbarElement = new ToolbarElementStub(toolbar);
				rootManager.SetToolbarElement(outgoingToolbarElement);

				var outgoingPlatformRoot = Assert.IsAssignableFrom<global::Android.Views.ViewGroup>(rootManager.RootView);
				var fragmentHost = new global::Android.Widget.FrameLayout(handler.MauiContext.Context)
				{
					Id = global::Android.Views.View.GenerateViewId()
				};
				outgoingPlatformRoot.AddView(fragmentHost);

				var fragmentManager = handler.MauiContext.Context.GetFragmentManager();
				var fragment = new ReentrantFragment(() => rootManager.SetToolbarElement(incomingToolbarElement));
				fragmentManager
					.BeginTransaction()
					.Add(fragmentHost.Id, fragment)
					.Commit();

				try
				{
					window.Page = replacementPage;

					await OnLoadedAsync(replacementPage);
					Assert.Same(incomingToolbarElement, rootManager.ToolbarElement);
					AssertPageAttachedToRoot(replacementPage, rootManager);
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
		}

		[Fact(DisplayName = "Detached Root With Active Fragment Can Be Replaced")]
		public async Task DetachedRootWithActiveFragmentCanBeReplaced()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var replacementPage = new ContentPage { Content = new Label { Text = "Replacement page" } };
			var window = new Window(initialPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var outgoingRoot = rootManager.RootView;
				var activityRoot = Assert.IsType<FakeActivityRootView>(handler.PlatformViewUnderTest.Parent);
				handler.PlatformViewUnderTest.RemoveFromParent();

				var applied = handler.ConnectContent(replacementPage);

				Assert.True(applied);
				await OnLoadedAsync(replacementPage);
				Assert.NotSame(outgoingRoot, rootManager.RootView);
				Assert.Same(rootManager.RootView, handler.PlatformViewUnderTest);
				Assert.Same(activityRoot, handler.PlatformViewUnderTest.Parent);
				AssertPageAttachedToRoot(replacementPage, rootManager);
			});
		}

		[Fact(DisplayName = "Detached Root With Active Fragment Can Be Disconnected")]
		public async Task DetachedRootWithActiveFragmentCanBeDisconnected()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var window = new Window(new NavigationPage(initialPage));

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var fragmentManager = handler.MauiContext.GetFragmentManager();
				var outgoingFragment = fragmentManager.FindFragmentById(Resource.Id.navigationlayout_content);
				Assert.NotNull(outgoingFragment);
				Assert.NotNull(rootManager.ToolbarElement);
				handler.PlatformViewUnderTest.RemoveFromParent();
				NavigationRootManager.RootRequestOutcome? outcome = null;

				rootManager.Disconnect((result, _) => outcome = result);
				fragmentManager.ExecutePendingTransactions();

				Assert.Equal(NavigationRootManager.RootRequestOutcome.Applied, outcome);
				Assert.Null(rootManager.RootView);
				Assert.Null(rootManager.ToolbarElement);
				Assert.DoesNotContain(outgoingFragment!, fragmentManager.Fragments);
				Assert.Null(fragmentManager.FindFragmentById(Resource.Id.navigationlayout_content));
			});
		}

		[Fact(DisplayName = "Busy Root Replacement Cancels After Retry Limit")]
		public async Task BusyRootReplacementCancelsAfterRetryLimit()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var replacementPage = new ContentPage { Content = new Label { Text = "Replacement page" } };
			var recoveredPage = new ContentPage { Content = new Label { Text = "Recovered page" } };
			var window = new Window(initialPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var outgoingRoot = rootManager.RootView;
				NavigationRootManager.RootRequestOutcome? outcome = null;
				rootManager.TryExecutePendingTransactionsOverride = (_, _) => false;

				var applied = rootManager.Connect(
					replacementPage,
					completion: (result, _) => outcome = result);

				Assert.False(applied);
				await AssertHelpers.AssertEventually(
					() => outcome is not null,
					timeout: 5000,
					message: "The busy root request did not reach its retry limit.");
				Assert.Equal(NavigationRootManager.RootRequestOutcome.Cancelled, outcome);
				Assert.Same(outgoingRoot, rootManager.RootView);
				AssertPageAttachedToRoot(initialPage, rootManager);
				Assert.False(HasRootSwapRetryInfrastructure(rootManager));

				rootManager.TryExecutePendingTransactionsOverride = null;
				Assert.True(handler.ConnectContent(recoveredPage));
				await OnLoadedAsync(recoveredPage);
				AssertPageAttachedToRoot(recoveredPage, rootManager);
			});
		}

		[Fact(DisplayName = "Unavailable Activity Permanently Stops Deferred Root Swaps")]
		public async Task UnavailableActivityPermanentlyStopsDeferredRootSwaps()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var replacementPage = new ContentPage { Content = new Label { Text = "Replacement page" } };
			var laterPage = new ContentPage { Content = new Label { Text = "Later page" } };
			var window = new Window(initialPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(initialPage);

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var activityUnavailable = false;
				NavigationRootManager.RootRequestOutcome? deferredOutcome = null;
				rootManager.TryExecutePendingTransactionsOverride = (_, _) => false;
				rootManager.IsActivityUnavailableOverride = () => activityUnavailable;

				Assert.False(rootManager.Connect(
					replacementPage,
					completion: (result, _) => deferredOutcome = result));

				activityUnavailable = true;
				await AssertHelpers.AssertEventually(
					() => deferredOutcome is not null,
					message: "The deferred root request did not observe the unavailable activity.");
				Assert.Equal(NavigationRootManager.RootRequestOutcome.Cancelled, deferredOutcome);
				Assert.Null(rootManager.RootView);

				NavigationRootManager.RootRequestOutcome? laterOutcome = null;
				Assert.False(rootManager.Connect(
					laterPage,
					completion: (result, _) => laterOutcome = result));
				Assert.Equal(NavigationRootManager.RootRequestOutcome.Cancelled, laterOutcome);
			});
		}

		[Fact(DisplayName = "Root Replacement During Disconnect Attaches Latest Root")]
		public async Task RootReplacementDuringDisconnectAttachesLatestRoot()
		{
			SetupBuilder();

			var initialPage = new ContentPage { Content = new Label { Text = "Initial page" } };
			var finalPage = new ContentPage { Content = new Label { Text = "Final page" } };
			var window = new Window(initialPage);
			var replacedDuringDisconnect = false;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var fragmentHost = new global::Android.Widget.FrameLayout(handler.MauiContext.Context)
				{
					Id = global::Android.Views.View.GenerateViewId()
				};
				Assert.IsAssignableFrom<global::Android.Views.ViewGroup>(rootManager.RootView).AddView(fragmentHost);

				var fragmentManager = handler.MauiContext.Context.GetFragmentManager();
				var fragment = new ReentrantFragment(() =>
				{
					replacedDuringDisconnect = true;
					window.Page = finalPage;
				});
				fragmentManager
					.BeginTransaction()
					.Add(fragmentHost.Id, fragment)
					.Commit();

				try
				{
					rootManager.Disconnect();

					await OnLoadedAsync(finalPage);
					Assert.True(replacedDuringDisconnect);
					Assert.Same(rootManager.RootView, handler.PlatformViewUnderTest);
					Assert.NotNull(handler.PlatformViewUnderTest.Parent);
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
		}

		[Fact(DisplayName = "Reentrant Root Requests Yield Before Continuing")]
		public async Task ReentrantRootRequestsYieldBeforeContinuing()
		{
			SetupBuilder();

			var pages = Enumerable.Range(1, 12)
				.Select(index => new ContentPage { Content = new Label { Text = $"Page {index}" } })
				.ToArray();
			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var nextPage = 1;
				void QueueNextPage()
				{
					if (nextPage < pages.Length)
						handler.ConnectContent(pages[nextPage++], QueueNextPage);
				}

				Assert.False(handler.ConnectContent(pages[0], QueueNextPage));
				Assert.True(nextPage < pages.Length);
				Assert.True(HasRootSwapRetryInfrastructure(handler.MauiContext.GetNavigationRootManager()));

				await OnLoadedAsync(pages[^1]);
				Assert.Equal(pages.Length, nextPage);
				AssertPageAttachedToRoot(pages[^1], handler.MauiContext.GetNavigationRootManager());
				Assert.False(HasRootSwapRetryInfrastructure(handler.MauiContext.GetNavigationRootManager()));
			});
		}

		[Fact(DisplayName = "Nested Virtual Disconnects Preserve Every Completion")]
		public async Task NestedVirtualDisconnectsPreserveEveryCompletion()
		{
			SetupBuilder();

			var window = new Window(new ContentPage());
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, handler =>
			{
				var rootManager = new NestedDisconnectNavigationRootManager(handler.MauiContext);
				NavigationRootManager.RootRequestOutcome? outerOutcome = null;

				rootManager.Disconnect((outcome, _) => outerOutcome = outcome);

				Assert.Equal(NavigationRootManager.RootRequestOutcome.Applied, outerOutcome);
				Assert.Equal(NavigationRootManager.RootRequestOutcome.Applied, rootManager.NestedOutcome);
			});
		}

		sealed class ToolbarElementStub : IToolbarElement
		{
			public ToolbarElementStub(IToolbar toolbar)
			{
				Toolbar = toolbar;
			}

			public IToolbar Toolbar { get; }
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

		sealed class NestedDisconnectNavigationRootManager : NavigationRootManager
		{
			bool _nestedDisconnectStarted;

			public NestedDisconnectNavigationRootManager(IMauiContext mauiContext)
				: base(mauiContext)
			{
			}

			public RootRequestOutcome? NestedOutcome { get; private set; }

			public override void Disconnect()
			{
				if (!_nestedDisconnectStarted)
				{
					_nestedDisconnectStarted = true;
					Disconnect((outcome, _) => NestedOutcome = outcome);
				}

				base.Disconnect();
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
			var platformView = page.ToPlatform();

			Assert.NotNull(platformView);
			Assert.True(platformView.IsAttachedToWindow);
			AssertPlatformViewAttachedToRoot(platformView, rootManager);
		}

		static void AssertPlatformViewAttachedToRoot(
			global::Android.Views.View platformView,
			NavigationRootManager rootManager)
		{
			var rootView = rootManager.RootView;

			Assert.NotNull(rootView);

			for (global::Android.Views.View current = platformView; current is not null; current = current.Parent as global::Android.Views.View)
			{
				if (ReferenceEquals(current, rootView))
					return;
			}

			Assert.Fail("The page's platform view is not hosted by the navigation root.");
		}

		static bool HasRootSwapRetryInfrastructure(NavigationRootManager rootManager)
		{
			const System.Reflection.BindingFlags flags =
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic;

			var handler = typeof(NavigationRootManager).GetField("_mainHandler", flags)?.GetValue(rootManager);
			var runnable = typeof(NavigationRootManager).GetField("_retryRunnable", flags)?.GetValue(rootManager);
			return handler is not null || runnable is not null;
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
