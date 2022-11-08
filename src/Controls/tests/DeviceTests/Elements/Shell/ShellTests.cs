using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
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
				});
			});
		}

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

			var page1 = new ContentPage();
			var page2 = new ContentPage();

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


		[Fact(DisplayName = "TitleView Causes a Crash When Switching Tabs")]
		public async Task TitleViewCausesACrashWhenSwitchingTabs()
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
				// GotoAsync which switching tabs/flyout items currently
				// doesn't resolve after navigated has finished which is why we have the
				// delays
				// https://github.com/dotnet/maui/issues/6193
				Assert.Equal(titleView1.ToPlatform(), GetTitleView(handler));
				await shell.GoToAsync("//Item2");
				await Task.Delay(200);
				await OnLoadedAsync(page2);
				Assert.Equal(titleView2.ToPlatform(), GetTitleView(handler));
				await shell.GoToAsync("//Item1");
				await Task.Delay(200);
				await OnLoadedAsync(page1);
				Assert.Equal(titleView1.ToPlatform(), GetTitleView(handler));
				await shell.GoToAsync("//Item2");
				await Task.Delay(200);
				await OnLoadedAsync(page2);
				Assert.Equal(titleView2.ToPlatform(), GetTitleView(handler));
				await shell.GoToAsync("//Item3");
				await Task.Delay(200);
				await OnLoadedAsync(page3);
				Assert.Equal(shellTitleView.ToPlatform(), GetTitleView(handler));
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

		protected Task<Shell> CreateShellAsync(Action<Shell> action) =>
			InvokeOnMainThreadAsync(() =>
			{
				var value = new Shell();
				action?.Invoke(value);
				return value;
			});
	}
}