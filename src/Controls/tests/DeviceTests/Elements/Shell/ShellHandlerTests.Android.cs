using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using MauiWindow = Microsoft.Maui.Controls.Window;
using static Microsoft.Maui.Controls.Platform.Compatibility.ShellFlyoutTemplatedContentRenderer;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using AView = Android.Views.View;

// New Android Shell Handler — uses Microsoft.Maui.Controls.Handlers.ShellHandler (not ShellRenderer)
using NativeShellHandler = Microsoft.Maui.Controls.Handlers.ShellHandler;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Handler-based equivalents for all ShellRenderer device tests.
	/// Uses the new Microsoft.Maui.Controls.Handlers.ShellHandler (MauiDrawerLayout-based).
	/// Existing ShellRenderer tests are NOT removed — this is a NEW parallel test class.
	/// All test names carry a "ShellHandler-" prefix per naming convention.
	/// </summary>
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class ShellHandlerTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					// Register new ShellHandler (replaces ShellRenderer for these tests)
					handlers.AddHandler(typeof(Shell), typeof(NativeShellHandler));
					// Sub-handlers required by new ShellHandler (ShellRenderer handled these internally)
					handlers.TryAddHandler(typeof(ShellItem), typeof(ShellItemHandler));
					handlers.TryAddHandler(typeof(ShellSection), typeof(ShellSectionHandler));
					// Common shell infrastructure handlers
					handlers.TryAddHandler<Layout, LayoutHandler>();
					handlers.TryAddHandler<Image, ImageHandler>();
					handlers.TryAddHandler<Label, LabelHandler>();
					handlers.TryAddHandler<Page, PageHandler>();
					handlers.TryAddHandler(typeof(Controls.Toolbar), typeof(ToolbarHandler));
					handlers.TryAddHandler(typeof(MenuBar), typeof(MenuBarHandler));
					handlers.TryAddHandler(typeof(MenuBarItem), typeof(MenuBarItemHandler));
					handlers.TryAddHandler(typeof(MenuFlyoutItem), typeof(MenuFlyoutItemHandler));
					handlers.TryAddHandler(typeof(MenuFlyoutSubItem), typeof(MenuFlyoutSubItemHandler));
					handlers.TryAddHandler<ScrollView, ScrollViewHandler>();
					// Page / nav handlers
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
					handlers.AddHandler(typeof(Controls.ContentView), typeof(ContentViewHandler));
					handlers.AddHandler(typeof(CollectionView), typeof(CollectionViewHandler));
				});
			});
		}

		// =========================================================
		// Tests from ShellTests.Android.cs
		// =========================================================

		[Fact(DisplayName = "ShellHandler-No crash going back using 'Shell.Current.GoToAsync(\"..\")'")]
		public async Task ShellHandler_GoingBackUsingGoToAsyncMethod()
		{
			SetupBuilder();

			var page1 = new ContentPage();

			var page2Content = new Label { Text = "Test" };
			var page2 = new ContentPage { Content = page2Content };

			var pointerGestureRecognizer = new PointerGestureRecognizer();
			pointerGestureRecognizer.PointerPressed += (sender, args) =>
			{
				Console.WriteLine("Page Content pressed");
			};

			page2Content.GestureRecognizers.Add(pointerGestureRecognizer);

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent() { Route = "Item1", Content = page1 },
						new ShellContent() { Route = "Item2", Content = page2 },
					}
				});
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(page1);
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("..");

				await shell.GoToAsync("//Item1");
				await shell.GoToAsync("//Item2");
				await shell.Navigation.PopAsync();

				await shell.GoToAsync("//Item1");
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("..");
			});
		}

		[Theory(DisplayName = "ShellHandler-CanHideNavBarShadow")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ShellHandler_CanHideNavBarShadow(bool navBarHasShadow)
		{
			SetupBuilder();

			var contentPage = new ContentPage() { Title = "Flyout Item" };
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { contentPage } };

				shell.FlyoutContent = new VerticalStackLayout()
				{
					new Label() { Text = "Flyout Content" }
				};

				Shell.SetNavBarHasShadow(contentPage, navBarHasShadow);
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);

				var platformToolbar = GetPlatformToolbar(handler);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();

				if (navBarHasShadow)
					Assert.True(appBar.Elevation > 0);
				else
					Assert.True(appBar.Elevation == 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-FlyoutItems Render When FlyoutBehavior Starts As Locked")]
		public async Task ShellHandler_FlyoutItemsRendererWhenFlyoutBehaviorStartsAsLocked()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var dl = GetDrawerLayout(handler);
				var flyoutContainer = GetFlyoutMenuReyclerView(handler);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-Shell with Flyout Disabled Doesn't Render Flyout")]
		public async Task ShellHandler_ShellWithFlyoutDisabledDoesntRenderFlyout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, (handler) =>
			{
				var dl = GetDrawerLayout(handler);
				Assert.Equal(1, dl.ChildCount);
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				Assert.Equal(2, dl.ChildCount);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "ShellHandler-FooterTemplate Measures to Set Flyout Width When Flyout Locked")]
		public async Task ShellHandler_FooterTemplateMeasuresToSetFlyoutWidth()
		{
			SetupBuilder();
			VerticalStackLayout footer = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there" }
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.FlyoutWidth = 20;
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(footer);
				Assert.True(Math.Abs(20 - footer.Frame.Width) < 1);
				Assert.True(footer.Frame.Height > 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-Flyout Footer and Default Flyout Items Render")]
		public async Task ShellHandler_FlyoutFooterRenderersWithDefaultFlyoutItems()
		{
			SetupBuilder();
			VerticalStackLayout footer = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there" }
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var dl = GetDrawerLayout(handler);
				await OpenFlyout(handler);

				var flyoutContainer = GetFlyoutMenuReyclerView(handler);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-FlyoutItemsRenderWhenFlyoutHeaderIsSet")]
		public async Task ShellHandler_FlyoutItemsRenderWhenFlyoutHeaderIsSet()
		{
			SetupBuilder();
			VerticalStackLayout header = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there" }
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = header;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var dl = GetDrawerLayout(handler);
				await OpenFlyout(handler);

				var flyoutContainer = GetFlyoutMenuReyclerView(handler);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-FlyoutHeaderRendersCorrectSizeWithFlyoutContentSet")]
		public async Task ShellHandler_FlyoutHeaderRendersCorrectSizeWithFlyoutContentSet()
		{
			SetupBuilder();
			VerticalStackLayout header = new VerticalStackLayout()
			{
				new Label() { Text = "Flyout Header" }
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = header;

				shell.FlyoutContent = new VerticalStackLayout()
				{
					new Label() { Text = "Flyout Content" }
				};

				shell.FlyoutFooter = new VerticalStackLayout()
				{
					new Label() { Text = "Flyout Footer" }
				};
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				var headerPlatformView = header.ToPlatform();
				var appBar = headerPlatformView.GetParentOfType<AppBarLayout>();
				Assert.Equal(appBar.MeasuredHeight - appBar.PaddingTop, headerPlatformView.MeasuredHeight);
			});
		}

		[Fact(DisplayName = "ShellHandler-SwappingOutAndroidContextDoesntCrash")]
		public async Task ShellHandler_SwappingOutAndroidContextDoesntCrash()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });
			});

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

		[Fact(DisplayName = "ShellHandler-ChangingBottomTabAttributesDoesntRecreateBottomTabs")]
		public async Task ShellHandler_ChangingBottomTabAttributesDoesntRecreateBottomTabs()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				var menu = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>().Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var icon1 = menuItem1.Icon;
				var icon2 = menuItem2.Icon;
				var title1 = menuItem1.TitleFormatted;
				var title2 = menuItem2.TitleFormatted;

				shell.CurrentItem.Items[0].Title = "new Title 1";
				shell.CurrentItem.Items[0].Icon = "blue.png";

				shell.CurrentItem.Items[1].Title = "new Title 2";
				shell.CurrentItem.Items[1].Icon = "blue.png";

				await AssertEventually(() => menuItem1.Icon != icon1);

				menu = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>().Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menuItem1.Icon.AssertColorAtCenter(global::Android.Graphics.Color.Blue);
				menuItem2.Icon.AssertColorAtCenter(global::Android.Graphics.Color.Blue);

				Assert.NotEqual(icon1, menuItem1.Icon);
				Assert.NotEqual(icon2, menuItem2.Icon);
				Assert.NotEqual(title1, menuItem1.TitleFormatted);
				Assert.NotEqual(title2, menuItem2.TitleFormatted);
			});
		}

		[Fact(DisplayName = "ShellHandler-RemovingBottomTabDoesntRecreateMenu")]
		public async Task ShellHandler_RemovingBottomTabDoesntRecreateMenu()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				var bottomView = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);

				shell.CurrentItem.Items.RemoveAt(2);

				await AssertEventually(() => bottomView.Menu.Size() == 2);

				menu = bottomView.Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));
			});
		}

		[Fact(DisplayName = "ShellHandler-AddingBottomTabDoesntRecreateMenu")]
		public async Task ShellHandler_AddingBottomTabDoesntRecreateMenu()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				var bottomView = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var menuItem2Icon = menuItem2.Icon;

				shell.CurrentItem.Items.Insert(1, new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "green.png" });

				await AssertEventually(() => bottomView.Menu.GetItem(1).Icon != menuItem2Icon);

				menu = bottomView.Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menu.GetItem(1).Icon.AssertColorAtCenter(global::Android.Graphics.Color.Green);
				menu.GetItem(2).Icon.AssertColorAtCenter(global::Android.Graphics.Color.Red);
			});
		}

		[Fact(DisplayName = "ShellHandler-Flyout Header Changes When Updated")]
		public async Task ShellHandler_FlyoutHeaderReactsToChanges()
		{
			SetupBuilder();

			var initialHeader = new Label() { Text = "Hello" };
			var newHeader = new Label() { Text = "Hello Part 2" };

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = initialHeader;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				var initialHeaderPlatformView = initialHeader.ToPlatform();
				Assert.NotNull(initialHeaderPlatformView);
				Assert.NotNull(initialHeader.Handler);

				shell.FlyoutHeader = newHeader;

				var newHeaderPlatformView = newHeader.ToPlatform();
				Assert.NotNull(newHeaderPlatformView);
				Assert.NotNull(newHeader.Handler);

				Assert.Null(initialHeader.Handler);

				await OpenFlyout(handler);

				var appBar = newHeaderPlatformView.GetParentOfType<AppBarLayout>();
				Assert.NotNull(appBar);
			});
		}

		[Fact(DisplayName = "ShellHandler-Ensure Default Colors are White for BottomNavigationView")]
		public async Task ShellHandler_ShellTabColorsDefaultToWhite()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1" });
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, (handler) =>
			{
				var bottomNavigationView = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>();
				Assert.NotNull(bottomNavigationView);

				var background = bottomNavigationView.Background;
				Assert.NotNull(background);

				if (background is ColorChangeRevealDrawable changeRevealDrawable)
				{
					Assert.Equal(global::Android.Graphics.Color.White, changeRevealDrawable.EndColor);
				}
			});
		}

		[Fact(DisplayName = "ShellHandler-ShellContentFragment.Destroy handles null _shellContext gracefully")]
		public async Task ShellHandler_ShellContentFragmentDestroyHandlesNullShellContext()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent() { Route = "Item1", Content = new ContentPage { Title = "Page 1" } },
						new ShellContent() { Route = "Item2", Content = new ContentPage { Title = "Page 2" } },
					}
				});
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);

				await shell.GoToAsync("//Item2");
				await OnNavigatedToAsync(shell.CurrentPage);

				await shell.GoToAsync("//Item1");
				await OnNavigatedToAsync(shell.CurrentPage);

				var exception = Record.Exception(() =>
				{
					Page page = new ContentPage();
					var fragment = new ShellContentFragment((IShellContext)null, page);
					fragment.Dispose();
				});

				Assert.Null(exception);
			});
		}

		// =========================================================
		// Tests from ShellTests.cs
		// =========================================================

		[Fact(DisplayName = "ShellHandler-SearchHandlerRendersCorrectly")]
		public async Task ShellHandler_SearchHandlerRendersCorrectly()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });

				Shell.SetSearchHandler(shell, new SearchHandler() { SearchBoxVisibility = SearchBoxVisibility.Expanded });
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem2");

				// No crash using SearchHandler
			});
		}

		[Fact(DisplayName = "ShellHandler-FlyoutContent Renderers When FlyoutBehavior Starts As Locked")]
		public async Task ShellHandler_FlyoutContentRenderersWhenFlyoutBehaviorStartsAsLocked()
		{
			SetupBuilder();
			var flyoutContent = new VerticalStackLayout() { Children = { new Label() { Text = "Rendered" } } };
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() } };
				shell.FlyoutContent = flyoutContent;
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(flyoutContent);

				Assert.NotNull(flyoutContent.Handler);
				Assert.True(flyoutContent.Frame.Width > 0);
				Assert.True(flyoutContent.Frame.Height > 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-Flyout Starts as Open correctly")]
		public async Task ShellHandler_FlyoutIsPresented()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() } };
				shell.FlyoutIsPresented = true;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await CheckFlyoutState(handler, true);
				shell.FlyoutIsPresented = false;
				await CheckFlyoutState(handler, false);
			});
		}

		[Fact(DisplayName = "ShellHandler-Back Button Visibility Changes with push/pop")]
		public async Task ShellHandler_BackButtonVisibilityChangesWithPushPop()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				Assert.False(IsBackButtonVisible(handler));
				await shell.Navigation.PushAsync(new ContentPage());
				Assert.True(IsBackButtonVisible(handler));
				await shell.Navigation.PopAsync();
				Assert.False(IsBackButtonVisible(handler));
			});
		}

		[Fact(DisplayName = "ShellHandler-Pushing the Same Page Disconnects Previous Toolbar Items")]
		public async Task ShellHandler_PushingTheSamePageUpdatesToolbar()
		{
			SetupBuilder();
			bool canExecute = false;
			var command = new Command(() => { }, () => canExecute);
			var pushedPage = new ContentPage()
			{
				ToolbarItems =
				{
					new ToolbarItem() { Command = command }
				}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await shell.Navigation.PushAsync(pushedPage);
				await shell.Navigation.PopAsync();
				canExecute = true;
				await shell.Navigation.PushAsync(pushedPage);
				command.ChangeCanExecute();
			});
		}

		[Fact(DisplayName = "ShellHandler-Set Has Back Button")]
		public async Task ShellHandler_SetHasBackButton()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
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

		[Fact(DisplayName = "ShellHandler-Correctly Adjust to Making Currently Visible Shell Page Invisible")]
		public async Task ShellHandler_CorrectlyAdjustToMakingCurrentlyVisibleShellPageInvisible()
		{
			SetupBuilder();

			var page1 = new ContentPage() { Content = new Label() { Text = "Page 1" }, Title = "Page 1" };
			var page2 = new ContentPage() { Content = new Label() { Text = "Page 2" }, Title = "Page 2" };

			var shell = await CreateShellAsync((shell) =>
			{
				var tabBar = new TabBar()
				{
					Items =
					{
						new ShellContent() { Content = page1 },
						new ShellContent() { Content = page2 },
					}
				};

				shell.Items.Add(tabBar);
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
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

		[Fact(DisplayName = "ShellHandler-Empty Shell")]
		public async Task ShellHandler_DetailsViewUpdates()
		{
			SetupBuilder();

			var shell = await InvokeOnMainThreadAsync<Shell>(() =>
			{
				return new Shell()
				{
					Items = { new ContentPage() }
				};
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				Assert.NotNull(shell.Handler);
			});
		}

		[Fact(DisplayName = "ShellHandler-TitleView Updates to Currently Visible Page")]
		public async Task ShellHandler_TitleViewUpdateToCurrentlyVisiblePage()
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
						new ShellContent() { Route = "Item1", Content = page1 },
						new ShellContent() { Route = "Item2", Content = page2 },
						new ShellContent() { Route = "Item3", Content = page3 },
					}
				});
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
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

		[Fact(DisplayName = "ShellHandler-Handlers not recreated when changing tabs")]
		public async Task ShellHandler_HandlersNotRecreatedWhenChangingTabs()
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
						new ShellContent() { Route = "Item1", Content = page1 },
						new ShellContent() { Route = "Item2", Content = page2 },
					}
				});
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				var initialHandler = page1.Handler;
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("//Item1");
				Assert.Equal(initialHandler, page1.Handler);
			});
		}

		[Fact(DisplayName = "ShellHandler-Navigation Routes Correctly After Switching Flyout Items")]
		public async Task ShellHandler_NavigatedFiresAfterSwitchingFlyoutItems()
		{
			SetupBuilder();

			var shellContent1 = new ShellContent() { Content = new ContentPage() };
			var shellContent2 = new ShellContent() { Content = new ContentPage() };

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(shellContent1);
				shell.Items.Add(shellContent2);
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				IShellController shellController = shell;
				await shellController.OnFlyoutItemSelectedAsync(shellContent2);
				await shell.Navigation.PushAsync(new ContentPage());
				await shell.GoToAsync("..");
			});
		}

		[Theory(DisplayName = "ShellHandler-BasicShellNavigationStructurePermutations")]
		[ClassData(typeof(ShellBasicNavigationTestCases))]
		public async Task ShellHandler_BasicShellNavigationStructurePermutations(ShellItem[] shellItems)
		{
			SetupBuilder();
			var shell = await InvokeOnMainThreadAsync<Shell>(() =>
			{
				var value = new Shell();
				foreach (var item in shellItems)
					value.Items.Add(item);

				return value;
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				await shell.GoToAsync("//page2");
				await Task.Delay(100);
			});
		}

		[Fact(DisplayName = "ShellHandler-Navigate to Root with BackButtonBehavior no Crash")]
		public async Task ShellHandler_NavigateToRootWithBackButtonBehaviorNoCrash()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
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

		[Fact(DisplayName = "ShellHandler-LifeCycleEvents Fire When Navigating Top Tabs")]
		public async Task ShellHandler_LifeCycleEventsFireWhenNavigatingTopTabs()
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
						new ShellContent() { Title = "Tab 1", Content = page1 },
						new ShellContent() { Title = "Tab 2", Content = page2 },
					}
				});
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnNavigatedToAsync(page1);

				Assert.Equal(0, page2.OnNavigatedToCount);
				await TapToSelect(page2);
				Assert.Equal(page2.Parent, shell.CurrentItem.CurrentItem.CurrentItem);
				page2.AssertLifeCycleCounts();
			});
		}

		[Fact(DisplayName = "ShellHandler-Pages Do Not Leak")]
		public async Task ShellHandler_PagesDoNotLeak()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage() { Title = "Page 1" };
			});

			WeakReference pageReference = null;

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);

				var page = new LeakyShellPage();
				pageReference = new WeakReference(page);

				await shell.Navigation.PushAsync(page);
				await shell.Navigation.PopAsync();
			});

			await AssertionExtensions.WaitForGC(pageReference);
		}

		[Fact(DisplayName = "ShellHandler-Shell add then remove items from selected item")]
		public async Task ShellHandler_ShellAddRemoveItems()
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

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				rootItem.IsVisible = true;

				shell.CurrentItem = rootItem.Items.Last();

				await OnLoadedAsync(shell.CurrentPage);

				for (int i = 0; i < itemCount; i++)
				{
					rootItem.Items.RemoveAt(0);
				}

				shell.CurrentItem = homeItem;
				Assert.True(shell.CurrentSection.Title == "Home");
			});
		}

		// =========================================================
		// Tests from ShellFlyoutTests.cs
		// =========================================================

		[Fact(DisplayName = "ShellHandler-FlyoutContentUpdatesAfterChange")]
		public async Task ShellHandler_FlyoutContentUpdatesAfterChange()
		{
			var flyoutContent = new VerticalStackLayout()
			{
				new Label() { Text = "Flyout Content" }
			};

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			},
			async (shell, handler) =>
			{
				Assert.False(flyoutContent.IsLoaded);

				shell.FlyoutContent = flyoutContent;
				await OnLoadedAsync(flyoutContent);

				shell.FlyoutContent = null;
				await OnUnloadedAsync(flyoutContent);
			});
		}

		[Fact(DisplayName = "ShellHandler-LogicalChildrenPropagateCorrectly")]
		public async Task ShellHandler_LogicalChildrenPropagateCorrectly()
		{
			var flyoutItemGrid = new Grid();
			var shellSectionGrid = new Grid();
			var shellContentGrid = new Grid();

			var flyoutItem = new FlyoutItem() { Items = { new ContentPage() } };
			var shellSection = new ShellSection() { Items = { new ContentPage() } };
			var shellContent = new ShellContent() { Content = new ContentPage() };

			flyoutItemGrid.BindingContextChanged += (_, _) =>
				Assert.True(flyoutItemGrid.BindingContext == flyoutItem || flyoutItem.BindingContext == null);

			shellSectionGrid.BindingContextChanged += (_, _) =>
				Assert.True(shellSectionGrid.BindingContext == shellSection || shellSection.BindingContext == null);

			shellContentGrid.BindingContextChanged += (_, _) =>
				Assert.True(shellContentGrid.BindingContext == shellContent || shellContent.BindingContext == null);

			Shell.SetItemTemplate(flyoutItem, new DataTemplate(() => flyoutItemGrid));
			Shell.SetItemTemplate(shellSection, new DataTemplate(() => shellSectionGrid));
			Shell.SetItemTemplate(shellContent, new DataTemplate(() => shellContentGrid));

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.Items.Add(flyoutItem);
				shell.Items.Add(shellSection);
				shell.Items.Add(shellContent);
			},
			async (shell, handler) =>
			{
				await OnLoadedAsync(flyoutItemGrid);
				await OnLoadedAsync(shellSectionGrid);
				await OnLoadedAsync(shellContentGrid);

				Assert.Equal(flyoutItemGrid.Parent, flyoutItem);
				Assert.Equal(shellSectionGrid.Parent, shellSection);
				Assert.Equal(shellContentGrid.Parent, shellContent);

				Assert.Contains(flyoutItemGrid, (flyoutItem as IVisualTreeElement).GetVisualChildren());
				Assert.Contains(shellSectionGrid, (shellSection as IVisualTreeElement).GetVisualChildren());
				Assert.Contains(shellContentGrid, (shellContent as IVisualTreeElement).GetVisualChildren());
			});
		}

		[Theory(DisplayName = "ShellHandler-FlyoutHeaderMinimumHeight")]
		[ClassData(typeof(ShellFlyoutHeaderBehaviorTestCases))]
		public async Task ShellHandler_FlyoutHeaderMinimumHeight(FlyoutHeaderBehavior behavior)
		{
			await RunShellTest(shell =>
			{
				var layout = new VerticalStackLayout()
				{
					new Label() { Text = "Flyout Header" }
				};

				shell.FlyoutHeader = layout;
				shell.FlyoutHeaderBehavior = behavior;
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);
				var flyoutFrame = GetFrameRelativeToFlyout(handler, shell.FlyoutHeader as IView);

				if (behavior == FlyoutHeaderBehavior.CollapseOnScroll)
				{
					AssertionExtensions.CloseEnough(56, flyoutFrame.Height);
				}
				else
				{
					Assert.True(flyoutFrame.Height < 56, $"Expected < 56 Actual: {flyoutFrame.Height}");
				}
			});
		}

		[Theory(DisplayName = "ShellHandler-FlyoutCustomContentMargin")]
		[ClassData(typeof(ShellFlyoutTemplatePartsTestCases))]
		public async Task ShellHandler_FlyoutCustomContentMargin(string testName)
		{
			Action<Shell, object> shellPart = ShellFlyoutTemplatePartsTestCases.GetTest(testName);
			var baselineContent = new VerticalStackLayout() { new Label() { Text = "Flyout Layout Part" } };
			Rect frameWithoutMargin = Rect.Zero;

			await RunShellTest(shell =>
			{
				shellPart(shell, baselineContent);
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);
				frameWithoutMargin = GetFrameRelativeToFlyout(handler, baselineContent);
			});

			var content = new VerticalStackLayout() { new Label() { Text = "Flyout Layout Part" } };
			await RunShellTest(shell =>
			{
				content.Margin = new Thickness(20, 30, 0, 30);
				shellPart(shell, content);
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);

				var frameWithMargin = GetFrameRelativeToFlyout(handler, content);
				var leftDiff = Math.Abs(Math.Abs(frameWithMargin.Left - (frameWithoutMargin.Left - baselineContent.Margin.Left)) - 20);
				double verticalDiff = Math.Abs(Math.Abs(frameWithMargin.Top - (frameWithoutMargin.Top)) - 30);

				Assert.True(leftDiff < 0.2, $"Left Margin Incorrect. Frame w/ margin: {frameWithMargin}. Frame w/o margin: {frameWithoutMargin}");
				Assert.True(verticalDiff < 0.2, $"Top Margin Incorrect. Frame w/ margin: {frameWithMargin}. Frame w/o margin: {frameWithoutMargin}");
			});
		}

		// =========================================================
		// Tests from ShellTabBarTests.cs
		// =========================================================

		[Fact(DisplayName = "ShellHandler-ForegroundColor sets icon and title color sets title")]
		public async Task ShellHandler_ForegroundColorSetsIconAndTitleColorSetsTitle()
		{
			SetupBuilder();

			var titleColor = Color.FromArgb("#FFFF0000");
			var foregroundColor = Color.FromArgb("#FF00FF00");
			await RunShellTabBarTests(shell =>
			{
				Shell.SetTabBarTitleColor(shell, titleColor);
				Shell.SetTabBarForegroundColor(shell, foregroundColor);
			},
			async (shell) =>
			{
				await ValidateTabBarIconColor(shell.CurrentSection, foregroundColor, true);
				await ValidateTabBarTextColor(shell.CurrentSection, titleColor, true);

				await ValidateTabBarIconColor(shell.Items[0].Items[1], titleColor, false);
				await ValidateTabBarTextColor(shell.Items[0].Items[1], foregroundColor, false);
			});
		}

		[Theory(DisplayName = "ShellHandler-Shell TabBar Title Color Initializes Correctly")]
		[InlineData("#FFFF0000")]
		[InlineData("#FF00FF00")]
		public async Task ShellHandler_ShellTabBarTitleColorInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarTitleColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarTextColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarIconColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarTextColor(shell.Items[0].Items[1], expectedColor, false);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], expectedColor, false);
				});
		}

		[Theory(DisplayName = "ShellHandler-Shell TabBar Foreground Initializes Correctly")]
		[InlineData("#FFFF0000")]
		[InlineData("#FF00FF00")]
		public async Task ShellHandler_ShellTabBarForegroundInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarForegroundColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarTextColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarIconColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarTextColor(shell.Items[0].Items[1], expectedColor, false);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], expectedColor, false);
				});
		}

		[Theory(DisplayName = "ShellHandler-Shell TabBar UnselectedColor Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		public async Task ShellHandler_ShellTabBarUnselectedColorInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarUnselectedColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarTextColor(shell.CurrentSection, expectedColor, false);
					await ValidateTabBarIconColor(shell.CurrentSection, expectedColor, false);
					await ValidateTabBarTextColor(shell.Items[0].Items[1], expectedColor, true);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], expectedColor, true);
				});
		}

		// =========================================================
		// Helper methods — adapted from ShellTests.Android.cs
		// (ShellRenderer → NativeShellHandler, ShellFlyoutRenderer → MauiDrawerLayout)
		// =========================================================

		protected async Task CheckFlyoutState(NativeShellHandler handler, bool desiredState)
		{
			var drawerLayout = GetDrawerLayout(handler);
			var flyout = drawerLayout.GetChildAt(1);

			if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
			{
				Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));
				return;
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();
			flyout.LayoutChange += OnLayoutChanged;

			try
			{
				await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
			}
			catch (TimeoutException) { }

			flyout.LayoutChange -= OnLayoutChanged;
			Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));

			return;

			void OnLayoutChanged(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
			{
				if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
				{
					taskCompletionSource.SetResult(true);
					flyout.LayoutChange -= OnLayoutChanged;
				}
			}
		}

		protected AView GetFlyoutPlatformView(NativeShellHandler shellHandler)
		{
			var drawerLayout = GetDrawerLayout(shellHandler);
			return drawerLayout.GetChildrenOfType<ShellFlyoutLayout>().First();
		}

		// The base IsBackButtonVisible calls base.GetPlatformToolbar which has no ShellHandler branch.
		// We shadow it here so the correct (modal-aware) toolbar is found via CoordinatorLayout walk.
		protected new bool IsBackButtonVisible(IElementHandler handler)
		{
			if (GetShellHandlerToolbar(handler)?.NavigationIcon is DrawerArrowDrawable dad)
				return dad.Progress == 1;

			return false;
		}

		MaterialToolbar GetShellHandlerToolbar(IElementHandler handler)
		{
			if (handler.VirtualView is VisualElement e)
				handler = e.Window?.Handler ?? handler;

			if (handler is IWindowHandler wh)
				handler = wh.VirtualView.Content.Handler;

			if (handler is NativeShellHandler)
			{
				var shell = handler.VirtualView as Shell;
				var currentPage = shell?.CurrentPage;

				if (currentPage?.Handler?.PlatformView is AView pagePlatformView)
				{
					// Walk up CoordinatorLayouts — handler has nested coordinators;
					// the toolbar lives in the outer one (navigationlayout.axml).
					var coordinator = pagePlatformView.GetParentOfType<CoordinatorLayout>();
					while (coordinator is not null)
					{
						var toolbar = coordinator.GetFirstChildOfType<MaterialToolbar>();
						if (toolbar is not null)
							return toolbar;

						coordinator = (coordinator.Parent as AView)?.GetParentOfType<CoordinatorLayout>();
					}
				}

				return null;
			}

			return GetPlatformToolbar(handler.MauiContext);
		}

		internal Rect GetFlyoutFrame(NativeShellHandler shellHandler)
		{
			var platformView = GetFlyoutPlatformView(shellHandler);
			var context = platformView.Context;

			return new Rect(0, 0,
				context.FromPixels(platformView.MeasuredWidth - (platformView.PaddingLeft + platformView.PaddingRight)),
				context.FromPixels(platformView.MeasuredHeight - (platformView.PaddingTop + platformView.PaddingBottom)));
		}

		internal Rect GetFrameRelativeToFlyout(NativeShellHandler shellHandler, IView view)
		{
			var platformView = (view.Handler as IPlatformViewHandler).PlatformView;
			return platformView.GetFrameRelativeTo(GetFlyoutPlatformView(shellHandler));
		}

		protected async Task OpenFlyout(NativeShellHandler shellHandler, TimeSpan? timeOut = null)
		{
			var flyoutView = GetFlyoutPlatformView(shellHandler);
			var drawerLayout = GetDrawerLayout(shellHandler);

			// New handler uses MauiDrawerLayout — no FlyoutFirstDrawPassFinished property.
			// Small unconditional delay preserves the original timing intent.
			await Task.Delay(10);

			var hamburger =
				GetPlatformToolbar((IPlatformViewHandler)shellHandler).GetChildrenOfType<AppCompatImageButton>().FirstOrDefault() ??
				throw new InvalidOperationException("Unable to find Drawer Button");

			timeOut = timeOut ?? TimeSpan.FromSeconds(2);

			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			drawerLayout.DrawerOpened += OnDrawerOpened;
			hamburger.PerformClick();

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);

			void OnDrawerOpened(object sender, DrawerLayout.DrawerOpenedEventArgs e)
			{
				drawerLayout.DrawerOpened -= OnDrawerOpened;
				taskCompletionSource.SetResult(true);
			}
		}

		protected async Task<double> ScrollFlyoutToBottom(NativeShellHandler shellHandler)
		{
			IShellContext shellContext = shellHandler;
			DrawerLayout dl = shellContext.CurrentDrawerLayout;
			var viewGroup = dl.GetChildAt(1) as ViewGroup;
			var scrollView = viewGroup?.GetChildAt(0);

			if (scrollView is RecyclerView rv)
				return await ScrollRecyclerViewToBottom(rv);
			else if (scrollView is MauiScrollView mauisv)
				return await ScrollMauiScrollViewToBottom(mauisv);
			else
				throw new Exception("RecyclerView or MauiScrollView not found");
		}

		async Task<double> ScrollRecyclerViewToBottom(RecyclerView flyoutItems)
		{
			TaskCompletionSource<object> result = new TaskCompletionSource<object>();
			flyoutItems.ScrollChange += OnFlyoutItemsScrollChange;
			flyoutItems.ScrollToPosition(flyoutItems.GetAdapter().ItemCount - 1);
			await result.Task.WaitAsync(TimeSpan.FromSeconds(2));
			await Task.Delay(10);

			void OnFlyoutItemsScrollChange(object sender, AView.ScrollChangeEventArgs e)
			{
				flyoutItems.ScrollChange -= OnFlyoutItemsScrollChange;
				result.TrySetResult(true);
			}

			var coordinatorLayout = flyoutItems.Parent.GetParentOfType<CoordinatorLayout>();
			var appbarLayout = coordinatorLayout.GetFirstChildOfType<AppBarLayout>();
			var clLayoutParams = appbarLayout.LayoutParameters as CoordinatorLayout.LayoutParams;
			var behavior = clLayoutParams.Behavior as AppBarLayout.Behavior;
			var headerContainer = appbarLayout.GetFirstChildOfType<HeaderContainer>();

			var verticalOffset = flyoutItems.ComputeVerticalScrollOffset();
			behavior.OnNestedPreScroll(coordinatorLayout, appbarLayout, flyoutItems, 0, verticalOffset, new int[2], ViewCompat.TypeTouch);
			await Task.Delay(10);

			return verticalOffset;
		}

		async Task<double> ScrollMauiScrollViewToBottom(MauiScrollView flyoutItems)
		{
			TaskCompletionSource<object> result = new TaskCompletionSource<object>();
			flyoutItems.ScrollChange += OnFlyoutItemsScrollChange;
			flyoutItems.ScrollTo(0, flyoutItems.Height);
			await result.Task.WaitAsync(TimeSpan.FromSeconds(2));
			await Task.Delay(10);

			void OnFlyoutItemsScrollChange(object sender, NestedScrollView.ScrollChangeEventArgs e)
			{
				flyoutItems.ScrollChange -= OnFlyoutItemsScrollChange;
				result.TrySetResult(true);
			}

			var coordinatorLayout = flyoutItems.Parent.GetParentOfType<CoordinatorLayout>();
			var appbarLayout = coordinatorLayout.GetFirstChildOfType<AppBarLayout>();
			var clLayoutParams = appbarLayout.LayoutParameters as CoordinatorLayout.LayoutParams;
			var behavior = clLayoutParams.Behavior as AppBarLayout.Behavior;
			var headerContainer = appbarLayout.GetFirstChildOfType<HeaderContainer>();

#pragma warning disable XAOBS001
			var verticalOffset = flyoutItems.ComputeVerticalScrollOffset();
#pragma warning restore XAOBS001
			behavior.OnNestedPreScroll(coordinatorLayout, appbarLayout, flyoutItems, 0, verticalOffset, new int[2], ViewCompat.TypeTouch);
			await Task.Delay(10);

			return verticalOffset;
		}

		MauiDrawerLayout GetDrawerLayout(NativeShellHandler shellHandler)
		{
			IShellContext shellContext = shellHandler;
			return (MauiDrawerLayout)shellContext.CurrentDrawerLayout;
		}

		RecyclerView GetFlyoutMenuReyclerView(NativeShellHandler shellHandler)
		{
			IShellContext shellContext = shellHandler;
			DrawerLayout dl = shellContext.CurrentDrawerLayout;

			if (dl.GetChildAt(1) is ViewGroup vg1 &&
				vg1.GetChildAt(0) is RecyclerView rvc)
			{
				return rvc;
			}

			throw new Exception("RecyclerView not found");
		}

		async Task TapToSelect(ContentPage page)
		{
			var shellContent = page.Parent as ShellContent;
			var shellSection = shellContent.Parent as ShellSection;
			var shellItem = shellSection.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;
			await OnNavigatedToAsync(shell.CurrentPage);

			if (shellItem != shell.CurrentItem)
				throw new NotImplementedException();

			if (shellSection != shell.CurrentItem.CurrentItem)
				throw new NotImplementedException();

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.GetParentOfType<ViewPager2>();

			pagerParent.CurrentItem = shellSection.Items.IndexOf(shellContent);
			await OnNavigatedToAsync(page);
		}

		// =========================================================
		// ShellFlyoutTests.cs RunShellTest helper (uses NativeShellHandler)
		// =========================================================

		async Task RunShellTest(Action<Shell> action, Func<Shell, NativeShellHandler, Task> testAction)
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				action(shell);
				if (shell.Items.Count == 0)
					shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() } };
			});

			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await OnNavigatedToAsync(shell.CurrentPage);
				await testAction(shell, handler);
			});
		}

		// =========================================================
		// ShellTabBarTests.cs helpers (uses NativeShellHandler)
		// =========================================================

		async Task RunShellTabBarTests(Action<Shell> setup, Func<Shell, Task> runTest)
		{
			SetupBuilder();

			var selectedContent = new ShellContent()
			{
				Route = "Tab1", Title = "Tab1",
				Content = new ContentPage(), Icon = "white_tab.png"
			};

			var unselectedContent = new ShellContent()
			{
				Route = "Tab2", Title = "Tab2",
				Content = new ContentPage(), Icon = "white_tab.png"
			};

			var unselectedContent_2 = new ShellContent()
			{
				Route = "Tab3", Title = "Tab3",
				Content = new ContentPage(), Icon = "white_tab.png"
			};

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items = { selectedContent, unselectedContent, unselectedContent_2 }
				});
			});

			setup.Invoke(shell);
			await CreateHandlerAndAddToWindow<NativeShellHandler>(shell, async (handler) =>
			{
				await runTest(shell);
			});
		}

		// ShellTabBarTests.Android.cs helper — uses new handler
		BottomNavigationView GetTab(ShellSection item)
		{
			var shell = item.FindParentOfType<Shell>();
			var handler = (NativeShellHandler)shell.Handler;
			var bottomView = GetDrawerLayout(handler).GetFirstChildOfType<BottomNavigationView>();
			var menu = bottomView.Menu;
			var index = shell.CurrentItem.Items.IndexOf(item);

			if (index < 0 || index >= menu.Size())
				Assert.Fail("ShellSection not found in Shell");

			if (index >= menu.Size())
				Assert.Fail("Menu Item has not been created for this item");

			return bottomView;
		}

		async Task ValidateTabBarIconColor(ShellSection item, Color iconColor, bool hasColor)
		{
			if (hasColor)
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTab(item), item.Title, iconColor, MauiContext);
			else
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTab(item), item.Title, iconColor, MauiContext);
		}

		async Task ValidateTabBarTextColor(ShellSection item, Color textColor, bool hasColor)
		{
			if (hasColor)
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTab(item), item.Title, textColor, MauiContext);
			else
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTab(item), item.Title, textColor, MauiContext);
		}

		// =========================================================
		// Shared helpers
		// =========================================================

		protected Task<Shell> CreateShellAsync(Action<Shell> action) =>
			InvokeOnMainThreadAsync(() =>
			{
				var value = new Shell();
				action?.Invoke(value);
				return value;
			});

		// =========================================================
		// Tests from ModalTests.cs — Shell-specific
		// =========================================================

		[Fact(DisplayName = "ShellHandler-AppearingAndDisappearingFireOnWindowAndModal (Shell)")]
		public async Task ShellHandler_AppearingAndDisappearingFireOnWindowAndModal()
		{
			SetupBuilder();
			var windowPage = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Window" }
			};

			var modalPage = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Modal" }
			};

			// Always use Shell (this is the Shell-specific variant)
			var window = new MauiWindow(new Shell() { CurrentItem = windowPage });

			int modalAppearing = 0;
			int modalDisappearing = 0;
			int windowAppearing = 0;
			int windowDisappearing = 0;

			modalPage.Appearing += (_, _) => modalAppearing++;
			modalPage.Disappearing += (_, _) => modalDisappearing++;

			windowPage.Appearing += (_, _) => windowAppearing++;
			windowPage.Disappearing += (_, _) => windowDisappearing++;

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await windowPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage.Content);
					await windowPage.Navigation.PopModalAsync();
				});

			Assert.Equal(1, modalAppearing);
			Assert.Equal(1, modalDisappearing);
			Assert.Equal(2, windowAppearing);
			Assert.Equal(2, windowDisappearing);
		}

		[Fact(DisplayName = "ShellHandler-PushingNavigationPageModallyWithShellShowsToolbarCorrectly")]
		public async Task ShellHandler_PushingNavigationPageModallyWithShellShowsToolbarCorrectly()
		{
			SetupBuilder();
			var windowPage = new LifeCycleTrackingPage()
			{
				Title = "Window Page Title"
			};

			var modalPage = new NavigationPage(new LifeCycleTrackingPage()
			{
				Content = new Label() { Text = "Modal page with navigation" }
			})
			{ Title = "modal page" };

			var window = new MauiWindow(new Shell() { CurrentItem = windowPage })
			{
				Title = "ShellHandler-PushingNavigationPageModallyWithShellShowsToolbarCorrectly Window Title"
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					var secondPage = new ContentPage() { Title = "Second Page" };
					await windowPage.Navigation.PushAsync(secondPage);
					await windowPage.Navigation.PushModalAsync(modalPage);

					// Navigation Bar is visible
					await AssertEventually(() => IsNavigationBarVisible(modalPage.Handler));
					Assert.False(IsBackButtonVisible(modalPage.Handler));

					// Verify that new navigation bar can gain a back button
					var secondModalPage = new ContentPage();
					await modalPage.Navigation.PushAsync(secondModalPage);
					await AssertEventually(() => IsBackButtonVisible(secondModalPage.Handler));
					await secondModalPage.Navigation.PopAsync();

					// Remove the modal page and validate the root window pages toolbar is still setup correctly
					await modalPage.Navigation.PopModalAsync();

					await AssertEventually(() => IsNavigationBarVisible(secondPage.Handler));
					await AssertEventually(() => IsBackButtonVisible(secondPage.Handler));
				});
		}

		[Fact(DisplayName = "ShellHandler-LifeCycleEventsFireOnModalPagesPushedBeforeWindowHasLoaded (Shell)")]
		public async Task ShellHandler_LifeCycleEventsFireOnModalPagesPushedBeforeWindowHasLoaded()
		{
			SetupBuilder();
			var windowPage = new LifeCycleTrackingPage();
			var modalPage = new LifeCycleTrackingPage()
			{
				Content = new Label()
			};

			// Always use Shell (Shell-specific variant)
			var window = new MauiWindow(new Shell() { CurrentItem = windowPage });

			await windowPage.Navigation.PushModalAsync(modalPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await OnNavigatedToAsync(modalPage);
					await OnLoadedAsync(modalPage.Content);

					Assert.Equal(0, windowPage.AppearingCount);

					Assert.Equal(1, modalPage.AppearingCount);
					Assert.Equal(1, modalPage.OnNavigatedToCount);
				});
		}

		[Fact(DisplayName = "ShellHandler-PushModalFromAppearing (Shell)")]
		public async Task ShellHandler_PushModalFromAppearing()
		{
			SetupBuilder();
			var windowPage = new ContentPage()
			{
				Content = new Label() { Text = "Root Page" }
			};

			var modalPage = new ContentPage()
			{
				Content = new Label() { Text = "last modal page" }
			};

			// Always use Shell (Shell-specific variant)
			var window = new MauiWindow(new Shell() { CurrentItem = windowPage });

			bool appearingFired = false;
			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					ContentPage contentPage = new ContentPage()
					{
						Content = new Label() { Text = "Second Page" }
					};

					contentPage.Appearing += async (_, _) =>
					{
						if (appearingFired)
							return;

						appearingFired = true;

						await windowPage.Navigation.PushModalAsync(new ContentPage()
						{
							Content = new Label() { Text = "First modal page" }
						});

						await windowPage.Navigation.PushModalAsync(modalPage);
					};

					await window.Page.Navigation.PushAsync(contentPage);
					await OnLoadedAsync(modalPage);
					await window.Navigation.PopModalAsync();
					await window.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
					await OnLoadedAsync(contentPage);
				});

			Assert.True(appearingFired);
		}

		[Fact(DisplayName = "ShellHandler-PushModalModalWithoutAwaiting (Shell)")]
		public async Task ShellHandler_PushModalModalWithoutAwaiting()
		{
			SetupBuilder();
			var windowPage = new LifeCycleTrackingPage();
			var modalPage = new LifeCycleTrackingPage()
			{
				Content = new Label() { Text = "last page" }
			};

			// Always use Shell (Shell-specific variant)
			var window = new MauiWindow(new Shell() { CurrentItem = windowPage });

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					_ = windowPage.Navigation.PushModalAsync(new ContentPage()
					{
						Content = new Label() { Text = "First page" }
					});

					_ = windowPage.Navigation.PushModalAsync(new ContentPage()
					{
						Content = new Label() { Text = "Second page" }
					});

					_ = windowPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
				});
		}

		// =========================================================
		// Tests from WindowTests.cs — Shell-specific
		// =========================================================

		[Fact(DisplayName = "ShellHandler-Toolbar Items Update when swapping out Main Page on Handler")]
		public async Task ShellHandler_ToolbarItemsUpdateWhenSwappingOutMainPageOnHandler()
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem() { Text = "Toolbar Item 1" };
			var firstPage = new ContentPage();

			var window = new MauiWindow(firstPage);

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

		[Theory(DisplayName = "ShellHandler-MainPageSwapTests")]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task ShellHandler_MainPageSwapTests(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new MauiWindow(firstRootPage);

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

						var toolbar = GetToolbar(handler);

						// Shell currently doesn't create the handler on the xplat toolbar with Android
						// Because Android has lots of toolbars spread out between the viewpagers
						var platformToolBar = GetPlatformToolbar(handler);
						Assert.Equal(platformToolBar != null, toolbar != null);

						if (platformToolBar != null)
						{
							if (window.Page is not Shell)
							{
								Assert.Equal(toolbar?.Handler?.PlatformView, platformToolBar);
							}

							Assert.True(IsNavigationBarVisible(handler));
						}
					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage} from {previousRootPage}", exc);
					}
				}
			});
		}

		[Fact(DisplayName = "ShellHandler-PageLayoutDoesNotExceedWindowBounds")]
		public async Task ShellHandler_PageLayoutDoesNotExceedWindowBounds()
		{
			SetupBuilder();

			var button = new Button() { Text = "Test me" };
			var contentPage = new ContentPage() { Content = button };
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

		[Fact(DisplayName = "ShellHandler-FlyoutWithAsMultipleItemsRendersWithoutCrashing")]
		public async Task ShellHandler_FlyoutWithAsMultipleItemsRendersWithoutCrashing()
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
							Items = { new ShellContent() { ContentTemplate = new DataTemplate(() => new ContentPage()) } }
						},
						new ShellSection()
						{
							FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
							Items = { new ShellContent() { ContentTemplate = new DataTemplate(() => new ContentPage()) } }
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

		[Fact(DisplayName = "ShellHandler-Flyout Width Does Not Crash")]
		public async Task ShellHandler_FlyoutWidthDoesNotCrash()
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

		[Fact(DisplayName = "ShellHandler-Appearing Fires Before NavigatedTo")]
		public async Task ShellHandler_AppearingFiresBeforeNavigatedTo()
		{
			SetupBuilder();
			var contentPage = new ContentPage();
			int contentPageAppearingFired = 0;
			int navigatedToFired = 0;
			int shellNavigatedToFired = 0;

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
					Items = { new ShellContent() { ContentTemplate = new DataTemplate(() => contentPage) } }
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

		[Fact(DisplayName = "ShellHandler-Navigating During Navigated Doesnt ReFire Appearing")]
		public async Task ShellHandler_NavigatingDuringNavigatedDoesntReFireAppearing()
		{
			SetupBuilder();
			var contentPage = new ContentPage();
			var secondContentPage = new ContentPage();
			int contentPageAppearingFired = 0;
			int navigatedToFired = 0;
			int secondContentPageAppearingFired = 0;
			int secondNavigatedToFired = 0;

			TaskCompletionSource<object> finishedSecondNavigation = new TaskCompletionSource<object>();

			contentPage.Appearing += (_, _) => contentPageAppearingFired++;

			secondContentPage.Appearing += (_, _) =>
			{
				secondContentPageAppearingFired++;
				Assert.Equal(0, secondNavigatedToFired);
			};

			secondContentPage.NavigatedTo += (_, _) => secondNavigatedToFired++;

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
					Items = { new ShellContent() { ContentTemplate = new DataTemplate(() => contentPage) } }
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

		[Fact(DisplayName = "ShellHandler-Swap Shell Root Page for NavigationPage")]
		public async Task ShellHandler_SwapShellRootPageForNavigationPage()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				var newPage = new ContentPage();
				(handler.VirtualView as MauiWindow).Page = new NavigationPage(newPage);
				await OnNavigatedToAsync(newPage);
				await OnFrameSetToNotEmpty(newPage);
				Assert.True(newPage.Frame.Height > 0);
			});
		}

		[Fact(DisplayName = "ShellHandler-PopToRootAsync correctly navigates to root page")]
		public async Task ShellHandler_PopToRootAsyncCorrectlyNavigationsBackToRootPage()
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

		[Fact(DisplayName = "ShellHandler-ChangingToNewMauiContextDoesntCrash")]
		public async Task ShellHandler_ChangingToNewMauiContextDoesntCrash()
		{
			SetupBuilder();

			var shell = new Shell();
			shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
			shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });

			var window = new MauiWindow(shell);
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

		[Fact(DisplayName = "ShellHandler-Toolbar Title")]
		public async Task ShellHandler_ToolbarTitle()
		{
			SetupBuilder();
			var navPage = new Shell()
			{
				CurrentItem = new ContentPage() { Title = "Page Title" }
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new MauiWindow(navPage), (handler) =>
			{
				string title = GetToolbarTitle(handler);
				Assert.Equal("Page Title", title);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "ShellHandler-Toolbar Title View Updates")]
		public async Task ShellHandler_ToolbarTitleViewUpdates()
		{
			SetupBuilder();
			var page1 = new ContentPage() { Title = "Page 1" };
			var page2 = new ContentPage() { Title = "Page 2" };
			var navPage = new Shell() { CurrentItem = page1 };

			var titleView1 = new VerticalStackLayout();
			var titleView2 = new Label();

			Shell.SetTitleView(page2, titleView1);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new MauiWindow(navPage), async (handler) =>
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

		[Fact(DisplayName = "ShellHandler-Toolbar Title Updates")]
		public async Task ShellHandler_ToolbarTitleUpdates()
		{
			SetupBuilder();
			var page1 = new ContentPage() { Title = "Page 1" };
			var page2 = new ContentPage() { Title = "Page 2" };
			var navPage = new Shell() { CurrentItem = page1 };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new MauiWindow(navPage), async (handler) =>
			{
				await navPage.Navigation.PushAsync(page2);
				await AssertEventually(() => "Page 2" == GetToolbarTitle(handler));
				page2.Title = "New Title";
				page1.Title = "Previous Page Title";
				await AssertEventually(() => "New Title" == GetToolbarTitle(handler));
			});
		}

		[Fact(DisplayName = "ShellHandler-Title View Measures")]
		public async Task ShellHandler_TitleViewMeasures()
		{
			SetupBuilder();
			var page1 = new ContentPage() { Title = "Page 1" };
			var navPage = new Shell() { CurrentItem = page1 };

			var titleView1 = new VerticalStackLayout()
			{
				new Label() { Text = "Title View" }
			};

			Shell.SetTitleView(page1, titleView1);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new MauiWindow(navPage), async (handler) =>
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

		[Fact(DisplayName = "ShellHandler-HideSoftInputOnTapped Doesn't Crash If Entry Is Still Focused After Window Is Null")]
		public async Task ShellHandler_HideSoftInputOnTappedDoesntCrashIfEntryIsStillFocusedAfterWindowIsNull()
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
					Content = new VerticalStackLayout() { entry }
				};
			}
		}

		[Fact(DisplayName = "ShellHandler-Can Reuse Pages")]
		public async Task ShellHandler_CanReusePages()
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

		[Fact(DisplayName = "ShellHandler-Can Clear ShellContent")]
		public async Task ShellHandler_CanClearShellContent()
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
				Assert.Equal(count, appearanceObservers.Count);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "ShellHandler-FlyoutIsPresented=true sets the visible status of the Shell Flyout.")]
		public async Task ShellHandler_FlyoutIsPresentedOpenDrawer()
		{
			await RunShellTest(shell =>
			{
				shell.FlyoutContent = new VerticalStackLayout() { new Label() { Text = "Flyout Content" } };
			},
			async (shell, handler) =>
			{
				shell.FlyoutIsPresented = true;

				var dl = GetDrawerLayout(handler);
				Assert.NotNull(dl);

				await AssertionExtensions.AssertEventually(() =>
				{
					var flyoutFrame = GetFlyoutFrame(handler);
					return flyoutFrame.Width > 0 && flyoutFrame.Height > 0 && dl.IsOpen;
				});
			});
		}

		// LeakyShellPage used by PagesDoNotLeak test
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

			void OnToolbarItemClicked(object sender, EventArgs e) { }
		}
	}
}
