using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Devices;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

#if IOS || MACCATALYST
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class ShellTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
					handlers.AddHandler(typeof(Controls.ContentView), typeof(ContentViewHandler));
					handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewHandler));
					handlers.AddHandler(typeof(CollectionView), typeof(CollectionViewHandler));
				});
			});
		}
#if !MACCATALYST
		[Fact]
		public async Task PageLayoutDoesNotExceedWindowBounds()
		{
			SetupBuilder();

			var button = new Button()
			{
				Text = "Test me"
			};

			var contentPage = new ContentPage()
			{
				Content = button
			};

			var shell = new Shell()
			{
				CurrentItem = contentPage,
				FlyoutBehavior = FlyoutBehavior.Disabled
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(contentPage);
				var pageBounds = contentPage.GetBoundingBox();
				var window = contentPage.Window;

				Assert.True(pageBounds.X >= 0);
				Assert.True(pageBounds.Y >= 0);
				Assert.True(pageBounds.Width <= window.Width);
				Assert.True(pageBounds.Height <= window.Height);
			});
		}
#endif

#if ANDROID || IOS || MACCATALYST
		[Fact]
		public async Task SearchHandlerRendersCorrectly()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });

				Shell.SetSearchHandler(shell, new SearchHandler() { SearchBoxVisibility = SearchBoxVisibility.Expanded });
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem2");

				// No crash using SearchHandler
			});
		}
#endif

		[Fact]
		public async Task FlyoutWithAsMultipleItemsRendersWithoutCrashing()
		{
			SetupBuilder();

			Shell shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new FlyoutItem()
				{
					FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
					Items =
					{
						new ShellSection()
						{
							FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
							Items =
							{
								new ShellContent()
								{
									ContentTemplate = new DataTemplate(() => new ContentPage())
								}
							}
						},
						new ShellSection()
						{
							FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
							Items =
							{
								new ShellContent()
								{
									ContentTemplate = new DataTemplate(() => new ContentPage())
								}
							}
						}
					}
				});
			});

			bool finished = false;
			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, (_) =>
			{
				finished = true;
				return Task.CompletedTask;
			});

			Assert.True(finished);
		}

		[Fact(DisplayName = "Flyout Width Does Not Crash")]
		public async Task FlyoutWidthDoesNotCrash()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
			});

			bool finished = false;
			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, (_) =>
			{
				finished = true;
				shell.FlyoutWidth = 1;
				return Task.CompletedTask;
			});

			Assert.True(finished);
		}

		[Fact(DisplayName = "Appearing Fires Before NavigatedTo")]
		public async Task AppearingFiresBeforeNavigatedTo()
		{
			SetupBuilder();
			var contentPage = new ContentPage();
			int contentPageAppearingFired = 0;
			int navigatedToFired = 0;
			int shellNavigatedToFired = 0;

			// If you fail these from the events then
			// an exception is raised in the middle of the platform
			// doing platform things which often leads to a crash
			bool navigatedPartPassed = false;
			bool navigatedToPartPassed = false;

			contentPage.Appearing += (_, _) =>
			{
				contentPageAppearingFired++;
				Assert.Equal(0, navigatedToFired);
				Assert.Equal(0, shellNavigatedToFired);
			};

			contentPage.NavigatedTo += (_, _) =>
			{
				navigatedToFired++;
				navigatedToPartPassed = (contentPageAppearingFired == 1);
			};

			Shell shell = await CreateShellAsync(shell =>
			{
				shell.Navigated += (_, _) =>
				{
					shellNavigatedToFired++;
					navigatedPartPassed = (contentPageAppearingFired == 1);

				};

				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							ContentTemplate = new DataTemplate(() => contentPage)
						}
					}
				});
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(contentPage);
			});

			Assert.True(navigatedPartPassed, "Appearing fired too many times before Navigated fired");
			Assert.True(navigatedToPartPassed, "Appearing fired too many times before NavigatedTo fired");
			Assert.Equal(1, contentPageAppearingFired);
			Assert.Equal(1, shellNavigatedToFired);
			Assert.Equal(1, navigatedToFired);
		}

		[Fact(DisplayName = "Navigating During Navigated Doesnt ReFire Appearing")]
		public async Task NavigatingDuringNavigatedDoesntReFireAppearing()
		{
			SetupBuilder();
			var contentPage = new ContentPage();
			var secondContentPage = new ContentPage();
			int contentPageAppearingFired = 0;
			int navigatedToFired = 0;

			int secondContentPageAppearingFired = 0;
			int secondNavigatedToFired = 0;

			TaskCompletionSource<object> finishedSecondNavigation = new TaskCompletionSource<object>();
			contentPage.Appearing += (_, _) =>
			{
				contentPageAppearingFired++;
			};

			secondContentPage.Appearing += (_, _) =>
			{
				secondContentPageAppearingFired++;
				Assert.Equal(0, secondNavigatedToFired);
			};

			secondContentPage.NavigatedTo += (_, _) =>
			{
				secondNavigatedToFired++;
			};

			Shell shell = null;
			contentPage.NavigatedTo += async (_, _) =>
			{
				navigatedToFired++;
				await shell.Navigation.PushAsync(secondContentPage);
				finishedSecondNavigation.SetResult(true);
			};

			shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							ContentTemplate = new DataTemplate(() => contentPage)
						}
					}
				});
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				await finishedSecondNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));

				Assert.Equal(1, contentPageAppearingFired);
				Assert.Equal(1, navigatedToFired);
				Assert.Equal(1, secondContentPageAppearingFired);
				Assert.Equal(1, secondNavigatedToFired);
			});
		}

#if !IOS && !MACCATALYST
		[Fact(DisplayName = "Swap Shell Root Page for NavigationPage")]
		public async Task SwapShellRootPageForNavigationPage()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				var newPage = new ContentPage();
				(handler.VirtualView as Window).Page = new NavigationPage(newPage);
				await OnNavigatedToAsync(newPage);
				await OnFrameSetToNotEmpty(newPage);
				Assert.True(newPage.Frame.Height > 0);
			});
		}
#endif

		[Fact(DisplayName = "PopToRootAsync correctly navigates to root page")]
		public async Task PopToRootAsyncCorrectlyNavigationsBackToRootPage()
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = rootPage;
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				await shell.Navigation.PushAsync(new ContentPage());
				await shell.Navigation.PushAsync(new ContentPage());
				await shell.Navigation.PushAsync(new ContentPage());
				await shell.Navigation.PushAsync(new ContentPage());
				await shell.Navigation.PopToRootAsync();
				await OnLoadedAsync(rootPage);
			});
		}

		[Fact(DisplayName = "FlyoutContent Renderers When FlyoutBehavior Starts As Locked")]
		public async Task FlyoutContentRenderersWhenFlyoutBehaviorStartsAsLocked()
		{
			SetupBuilder();
			var flyoutContent = new VerticalStackLayout() { Children = { new Label() { Text = "Rendered" } } };
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() } };
				shell.FlyoutContent = flyoutContent;
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(flyoutContent);

				Assert.NotNull(flyoutContent.Handler);
				Assert.True(flyoutContent.Frame.Width > 0);
				Assert.True(flyoutContent.Frame.Height > 0);
			});
		}

#if !IOS && !MACCATALYST
		[Fact(DisplayName = "Flyout Starts as Open correctly")]
		public async Task FlyoutIsPresented()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() } };
				shell.FlyoutIsPresented = true;
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await CheckFlyoutState(handler, true);
				shell.FlyoutIsPresented = false;
				await CheckFlyoutState(handler, false);
			});
		}
#endif

		[Fact(DisplayName = "Back Button Visibility Changes with push/pop")]
		public async Task BackButtonVisibilityChangesWithPushPop()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				Assert.False(IsBackButtonVisible(handler));
				await shell.Navigation.PushAsync(new ContentPage());
				Assert.True(IsBackButtonVisible(handler));
				await shell.Navigation.PopAsync();
				Assert.False(IsBackButtonVisible(handler));
			});
		}


		[Fact(DisplayName = "Pushing the Same Page Disconnects Previous Toolbar Items")]
		public async Task PushingTheSamePageUpdatesToolbar()
		{
			SetupBuilder();
			bool canExecute = false;
			var command = new Command(() => { }, () => canExecute);
			var pushedPage = new ContentPage()
			{
				ToolbarItems =
				{
					new ToolbarItem()
					{
						Command = command
					}
				}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await shell.Navigation.PushAsync(pushedPage);
				await shell.Navigation.PopAsync();
				canExecute = true;
				await shell.Navigation.PushAsync(pushedPage);
				command.ChangeCanExecute();
			});
		}

		[Fact(DisplayName = "Set Has Back Button")]
		public async Task SetHasBackButton()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				Assert.False(IsBackButtonVisible(shell.Handler));
				await shell.Navigation.PushAsync(new ContentPage());
				Assert.True(IsBackButtonVisible(shell.Handler));

				BackButtonBehavior behavior = new BackButtonBehavior()
				{
					IsVisible = false
				};

				Shell.SetBackButtonBehavior(shell.CurrentPage, behavior);
				Assert.False(IsBackButtonVisible(shell.Handler));
				behavior.IsVisible = true;
				NavigationPage.SetHasBackButton(shell.CurrentPage, true);
			});
		}

#if !IOS && !MACCATALYST
		[Fact(DisplayName = "Correctly Adjust to Making Currently Visible Shell Page Invisible")]
		public async Task CorrectlyAdjustToMakingCurrentlyVisibleShellPageInvisible()
		{
			SetupBuilder();

			var page1 = new ContentPage()
			{ Content = new Label() { Text = "Page 1" }, Title = "Page 1" };
			var page2 = new ContentPage()
			{ Content = new Label() { Text = "Page 2" }, Title = "Page 2" };

			var shell = await CreateShellAsync((shell) =>
			{
				var tabBar = new TabBar()
				{
					Items =
					{
						new ShellContent(){ Content = page1 },
						new ShellContent(){ Content = page2 },
					}
				};

				shell.Items.Add(tabBar);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnNavigatedToAsync(page1);
				shell.CurrentItem = page2;
				await OnNavigatedToAsync(page2);
				page2.IsVisible = false;
				await OnNavigatedToAsync(page1);
				Assert.Equal(shell.CurrentPage, page1);
				page2.IsVisible = true;
				shell.CurrentItem = page2;
				await OnNavigatedToAsync(page2);
				Assert.Equal(shell.CurrentPage, page2);
			});
		}
#endif

		[Fact(DisplayName = "Empty Shell")]
		public async Task DetailsViewUpdates()
		{
			SetupBuilder();

			var shell = await InvokeOnMainThreadAsync<Shell>(() =>
			{
				return new Shell()
				{
					Items = { new ContentPage() }
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				// TODO MAUI Fix this 
				await Task.Delay(100);
				Assert.NotNull(shell.Handler);
			});
		}

		[Fact(DisplayName = "TitleView Updates to Currently Visible Page")]
		public async Task TitleViewUpdateToCurrentlyVisiblePage()
		{
			SetupBuilder();

			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var page3 = new ContentPage();

			var titleView1 = new VerticalStackLayout();
			var titleView2 = new Label();
			var shellTitleView = new Editor();

			Shell.SetTitleView(page1, titleView1);
			Shell.SetTitleView(page2, titleView2);

			var shell = await CreateShellAsync((shell) =>
			{
				Shell.SetTitleView(shell, shellTitleView);
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							Route = "Item1",
							Content = page1
						},
						new ShellContent()
						{
							Route = "Item2",
							Content = page2
						},
						new ShellContent()
						{
							Route = "Item3",
							Content = page3
						},
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(page1);
				Assert.Equal(titleView1.ToPlatform(), GetTitleView(handler));

				bool viewIsTitleView(IView view)
				{
					return view.Handler != null && view.ToPlatform() == GetTitleView(handler);
				}

				await shell.GoToAsync("//Item2");
				await AssertEventually(() => viewIsTitleView(titleView2));

				await shell.GoToAsync("//Item1");
				await AssertEventually(() => viewIsTitleView(titleView1));

				await shell.GoToAsync("//Item2");
				await AssertEventually(() => viewIsTitleView(titleView2));

				await shell.GoToAsync("//Item3");
				await AssertEventually(() => viewIsTitleView(shellTitleView));
			});
		}

		[Fact(DisplayName = "Handlers not recreated when changing tabs")]
		public async Task HandlersNotRecreatedWhenChangingTabs()
		{
			SetupBuilder();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							Route = "Item1",
							Content = page1
						},
						new ShellContent()
						{
							Route = "Item2",
							Content = page2
						},
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var initialHandler = page1.Handler;
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("//Item1");
				Assert.Equal(initialHandler, page1.Handler);
			});
		}

		[Fact(DisplayName = "Navigation Routes Correctly After Switching Flyout Items")]
		public async Task NavigatedFiresAfterSwitchingFlyoutItems()
		{
			SetupBuilder();

			var shellContent1 = new ShellContent() { Content = new ContentPage() };
			var shellContent2 = new ShellContent() { Content = new ContentPage() };

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(shellContent1);
				shell.Items.Add(shellContent2);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				IShellController shellController = shell;
				var currentItem = shell.CurrentItem;
				// For now on iOS/Android we're just making sure nothing crashes
#if WINDOWS
				Assert.NotNull(currentItem.Handler);
#endif

				await shellController.OnFlyoutItemSelectedAsync(shellContent2);
				await shell.Navigation.PushAsync(new ContentPage());
				await shell.GoToAsync("..");

#if WINDOWS
				Assert.NotNull(shell.Handler);
				Assert.NotNull(shell.CurrentItem.Handler);
				Assert.NotNull(shell.CurrentItem.CurrentItem.Handler);
				Assert.Null(currentItem.Handler);
				Assert.Null(currentItem.CurrentItem.Handler);
#endif

			});
		}

#if !IOS && !MACCATALYST
		[Fact]
		public async Task ChangingToNewMauiContextDoesntCrash()
		{
			SetupBuilder();

			var shell = new Shell();
			shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
			shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });


			var window = new Controls.Window(shell);
			var mauiContextStub1 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub1);

			var mauiContextStub2 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem1");
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub2);
		}
#endif

		[Theory]
		[ClassData(typeof(ShellBasicNavigationTestCases))]
		public async Task BasicShellNavigationStructurePermutations(ShellItem[] shellItems)
		{
			SetupBuilder();
			var shell = await InvokeOnMainThreadAsync<Shell>(() =>
			{
				var value = new Shell();
				foreach (var item in shellItems)
					value.Items.Add(item);

				return value;
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				// TODO MAUI Fix this 
				await Task.Delay(100);
				await shell.GoToAsync("//page2");
				await Task.Delay(100);
			});
		}

		[Fact(DisplayName = "Navigate to Root with BackButtonBehavior no Crash")]
		public async Task NavigateToRootWithBackButtonBehaviorNoCrash()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				Assert.False(IsBackButtonVisible(shell.Handler));
				var secondPage = new ContentPage();
				await shell.Navigation.PushAsync(secondPage);
				Assert.True(IsBackButtonVisible(shell.Handler));

				bool canExecute = true;
				Command command = new Command(() => { }, () => canExecute);
				BackButtonBehavior behavior = new BackButtonBehavior()
				{
					IsVisible = false,
					Command = command
				};

				Shell.SetBackButtonBehavior(secondPage, behavior);
				await AssertEventually(() => !IsBackButtonVisible(shell.Handler));
				Assert.False(IsBackButtonVisible(shell.Handler));
				behavior.IsVisible = true;
				NavigationPage.SetHasBackButton(shell.CurrentPage, true);

				await shell.Navigation.PopToRootAsync(false);
				await Task.Delay(100);
				canExecute = false;

				command.ChangeCanExecute();
				Assert.NotNull(shell.Handler);
			});
		}

		[Fact(DisplayName = "LifeCycleEvents Fire When Navigating Top Tabs")]
		public async Task LifeCycleEventsFireWhenNavigatingTopTabs()
		{
			SetupBuilder();

			var page1 = new LifeCycleTrackingPage() { Content = new Label() { Padding = 40, Text = "Page 1", Background = SolidColorBrush.Purple } };
			var page2 = new LifeCycleTrackingPage() { Content = new Label() { Padding = 40, Text = "Page 2", Background = SolidColorBrush.Green } };

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new Tab()
				{
					Items =
					{
						new ShellContent()
						{
							Title = "Tab 1",
							Content = page1
						},
						new ShellContent()
						{
							Title = "Tab 2",
							Content = page2
						},
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnNavigatedToAsync(page1);

				Assert.Equal(0, page2.OnNavigatedToCount);
				await TapToSelect(page2);
				Assert.Equal(page2.Parent, shell.CurrentItem.CurrentItem.CurrentItem);
				page2.AssertLifeCycleCounts();
			});
		}

		[Fact(DisplayName = "Toolbar Title")]
		public async Task ToolbarTitle()
		{
			SetupBuilder();
			var navPage = new Shell()
			{
				CurrentItem = new ContentPage()
				{
					Title = "Page Title"
				}
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				string title = GetToolbarTitle(handler);
				Assert.Equal("Page Title", title);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Title View Updates")]
		public async Task ToolbarTitleViewUpdates()
		{
			SetupBuilder();
			var page1 = new ContentPage()
			{
				Title = "Page 1"
			};

			var page2 = new ContentPage()
			{
				Title = "Page 2"
			};

			var navPage = new Shell()
			{
				CurrentItem = page1
			};

			var titleView1 = new VerticalStackLayout();
			var titleView2 = new Label();

			Shell.SetTitleView(page2, titleView1);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				bool viewIsTitleView(IView view)
				{
					return view.Handler != null && view.ToPlatform() == GetTitleView(handler);
				}

				await navPage.Navigation.PushAsync(page2);
				await AssertEventually(() => viewIsTitleView(titleView1));
				Shell.SetTitleView(page2, titleView2);
				await AssertEventually(() => viewIsTitleView(titleView2));
			});
		}

		[Fact(DisplayName = "Toolbar Title Updates")]
		public async Task ToolbarTitleUpdates()
		{
			SetupBuilder();
			var page1 = new ContentPage()
			{
				Title = "Page 1"
			};

			var page2 = new ContentPage()
			{
				Title = "Page 2"
			};

			var navPage = new Shell()
			{
				CurrentItem = page1
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.Navigation.PushAsync(page2);
				await AssertEventually(() => "Page 2" == GetToolbarTitle(handler));
				page2.Title = "New Title";
				page1.Title = "Previous Page Title"; // Ensuring this doesn't influence title
				await AssertEventually(() => "New Title" == GetToolbarTitle(handler));
			});
		}

#if !WINDOWS
		[Fact(DisplayName = "Title View Measures")]
		public async Task TitleViewMeasures()
		{
			SetupBuilder();
			var page1 = new ContentPage()
			{
				Title = "Page 1"
			};

			var navPage = new Shell()
			{
				CurrentItem = page1
			};

			var titleView1 = new VerticalStackLayout()
			{
				new Label()
				{
					Text = "Title View"
				}
			};

			Shell.SetTitleView(page1, titleView1);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnFrameSetToNotEmpty(titleView1);
				var containerSize = GetTitleViewExpectedSize(handler);
				var titleView1PlatformSize = titleView1.GetBoundingBox();
				Assert.Equal(containerSize.Width, titleView1PlatformSize.Width);
				Assert.Equal(containerSize.Height, titleView1PlatformSize.Height);
				Assert.True(containerSize.Height > 0);
				Assert.True(containerSize.Width > 0);

			});
		}
#endif

		[Fact(DisplayName = "Pages Do Not Leak")]
		public async Task PagesDoNotLeak()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage() { Title = "Page 1" };
			});

			WeakReference pageReference = null;

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);

				var page = new LeakyShellPage();
				pageReference = new WeakReference(page);

				await shell.Navigation.PushAsync(page);
				await shell.Navigation.PopAsync();
			});

			await AssertionExtensions.WaitForGC(pageReference);
		}

		class LeakyShellPage : ContentPage
		{
			public LeakyShellPage()
			{
				Title = "Page 2";
				Content = new VerticalStackLayout
				{
					new Label(),
					new Button(),
					new Controls.ContentView(),
					new ScrollView(),
					new CollectionView(),
				};

				var item = new ToolbarItem { Text = "Primary Item", Order = ToolbarItemOrder.Primary };
				item.Clicked += OnToolbarItemClicked;
				ToolbarItems.Add(item);

				item = new ToolbarItem { Text = "Secondary Item", Order = ToolbarItemOrder.Secondary };
				item.Clicked += OnToolbarItemClicked;
				ToolbarItems.Add(item);
			}

			// Needs to be an instance method
			void OnToolbarItemClicked(object sender, EventArgs e) { }
		}

		//HideSoftInputOnTapped doesn't currently do anything on windows
#if !WINDOWS
		[Fact(DisplayName = "HideSoftInputOnTapped Doesn't Crash If Entry Is Still Focused After Window Is Null")]
		public async Task HideSoftInputOnTappedDoesntCrashIfEntryIsStillFocusedAfterWindowIsNull()
		{
			SetupBuilder();

			Entry entry1;
			Entry entry2;

			var rootPage = CreatePage(out entry1);
			var nextPage = CreatePage(out entry2);

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = rootPage;
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				entry1.Focus();
				await entry1.WaitForFocused();

				await rootPage.Navigation.PushAsync(nextPage);
				entry2.Focus();
				await entry2.WaitForFocused();

				await shell.GoToAsync("..", false);
			});

			ContentPage CreatePage(out Entry entry)
			{
				entry = new Entry();

				return new ContentPage()
				{
					HideSoftInputOnTapped = true,
					Content = new VerticalStackLayout()
					{
						entry
					}
				};
			}
		}
#endif

		[Fact(DisplayName = "Can Reuse Pages")]
		public async Task CanReusePages()
		{
			SetupBuilder();
			var rootPage = new ContentPage();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = rootPage;
			});
			var reusedPage = new ContentPage
			{
				Content = new Label { Text = "HEY" }
			};
			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				await shell.Navigation.PushAsync(reusedPage);
				await shell.Navigation.PopAsync();
				await shell.Navigation.PushAsync(reusedPage);
				await OnLoadedAsync(reusedPage.Content);
			});
		}

		[Fact(DisplayName = "Shell add then remove items from selected item")]
		public async Task ShellAddRemoveItems()
		{
			FlyoutItem rootItem = null;
			ShellItem homeItem = null;
			int itemCount = 3;

			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				homeItem = new ShellContent()
				{
					Title = "Home",
					Route = "MainPage",
					Content = new ContentPage()
				};
				shell.Items.Add(homeItem);

				rootItem = new FlyoutItem()
				{
					Title = "Items",
					Route = "Items",
					FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
				};

				for (int i = 0; i < itemCount; i++)
				{
					var shellContent = new ShellContent
					{
						Content = new ContentPage(),
						Title = $"Item {i}",
						Route = $"Item{i}"
					};
					rootItem.Items.Add(shellContent);
				}
				shell.Items.Add(rootItem);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				rootItem.IsVisible = true;

				shell.CurrentItem = rootItem.Items.Last();

				// Wait for the rootItem to finish loading before we remove items.
				// If we just start removing items directly after setting the current item
				// This can cause the platform to crash since we're pulling the
				// carpet out from under it while it's trying to load.
				await OnLoadedAsync(shell.CurrentPage);

				// Remove all root child items
				for (int i = 0; i < itemCount; i++)
				{
					rootItem.Items.RemoveAt(0);
				}

				shell.CurrentItem = homeItem;
				Assert.True(shell.CurrentSection.Title == "Home");
			});
		}

		[Fact(DisplayName = "Can Clear ShellContent")]
		public async Task CanClearShellContent()
		{
			SetupBuilder();
			var page = new ContentPage();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ShellContent { Content = page };
			});

			var appearanceObserversField = shell.GetType().GetField("_appearanceObservers", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(appearanceObserversField);
			var appearanceObservers = appearanceObserversField.GetValue(shell) as IList;
			Assert.NotNull(appearanceObservers);

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, _ =>
			{
				int count = appearanceObservers.Count;
				shell.Items.Clear();
				shell.CurrentItem = new ShellContent { Content = page };
				Assert.Equal(count, appearanceObservers.Count); // Count doesn't increase
			});
		}

		protected Task<Shell> CreateShellAsync(Action<Shell> action) =>
			InvokeOnMainThreadAsync(() =>
			{
				var value = new Shell();
				action?.Invoke(value);
				return value;
			});
	}
}
