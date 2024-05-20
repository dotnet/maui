using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
#if !MACCATALYST
		[Fact(DisplayName = "Page Adjust When Top Tabs Are Present")]
		public async Task PageAdjustsWhenTopTabsArePresent()
		{
			SetupBuilder();
			var pageWithTopTabs = new ContentPage() { Content = new Label() { Text = "Page With Top Tabs" } };
			var pageWithoutTopTabs = new ContentPage() { Content = new Label() { Text = "Page With Bottom Tabs" } };

			pageWithTopTabs.On<iOS>().SetUseSafeArea(true);
			pageWithoutTopTabs.On<iOS>().SetUseSafeArea(true);

			var mainTab1 = new Tab()
			{
				Items =
				{
					new ShellContent() { Content = pageWithTopTabs, Title = "tab 1" },
					new ShellContent() { Content = new ContentPage(), Title = "tab 2" }
				}
			};

			var mainTab2 = new Tab()
			{
				Items =
				{
					new ShellContent() { Content = pageWithoutTopTabs, Title = "tab 3"  }
				}
			};

			var shell = new Shell()
			{
				Items = { mainTab1, mainTab2 }
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnFrameSetToNotEmpty(pageWithTopTabs.Content);
				var boundsWithTopTabs = pageWithTopTabs.Content.GetPlatformViewBounds();
				shell.CurrentItem = mainTab2;
				await OnFrameSetToNotEmpty(pageWithoutTopTabs.Content);
				var boundsWithoutTopTabs = pageWithoutTopTabs.Content.GetPlatformViewBounds();
				Assert.Equal(ShellSectionRootRenderer.HeaderHeight, (boundsWithTopTabs.Top - boundsWithoutTopTabs.Top), 1);

				shell.CurrentItem = mainTab1;
				await OnFrameSetToNotEmpty(pageWithTopTabs.Content);

				var boundsWithTopTabsNavigatedBack = pageWithTopTabs.Content.GetPlatformViewBounds();
				Assert.Equal(boundsWithTopTabsNavigatedBack, boundsWithTopTabs);
			});
		}

		[Fact(DisplayName = "Swiping Away Modal Propagates to Shell")]
		public async Task SwipingAwayModalPropagatesToShell()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var modalPage = new ContentPage();
				modalPage.On<iOS>().SetModalPresentationStyle(Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.FormSheet);
				var platformWindow = MauiContext.GetPlatformWindow().RootViewController;

				await shell.Navigation.PushModalAsync(modalPage);

				var modalVC = GetModalWrapper(modalPage);
				int navigatedFired = 0;
				ShellNavigationSource? shellNavigationSource = null;
				var finishedNavigation = new TaskCompletionSource<bool>();
				shell.Navigated += ShellNavigated;

				modalVC.DidDismiss(null);
				await finishedNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));
				Assert.Equal(1, navigatedFired);
				Assert.Equal(ShellNavigationSource.PopToRoot, shellNavigationSource.Value);

				Assert.Empty(shell.Navigation.ModalStack);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
					shellNavigationSource = e.Source;
					finishedNavigation.SetResult(true);
				}
			});
		}

		[Fact(DisplayName = "Swiping Away Modal Removes Entire Navigation Page")]
		public async Task SwipingAwayModalRemovesEntireNavigationPage()
		{
			Routing.RegisterRoute(nameof(SwipingAwayModalRemovesEntireNavigationPage), typeof(ModalShellPage));

			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var modalPage = new Controls.NavigationPage(new ContentPage());
				modalPage.On<iOS>().SetModalPresentationStyle(Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.FormSheet);
				var platformWindow = MauiContext.GetPlatformWindow().RootViewController;

				await shell.Navigation.PushModalAsync(modalPage);
				await shell.GoToAsync(nameof(SwipingAwayModalRemovesEntireNavigationPage));
				await shell.GoToAsync(nameof(SwipingAwayModalRemovesEntireNavigationPage));
				await shell.GoToAsync(nameof(SwipingAwayModalRemovesEntireNavigationPage));

				var modalVC = GetModalWrapper(modalPage);
				int navigatedFired = 0;
				ShellNavigationSource? shellNavigationSource = null;
				var finishedNavigation = new TaskCompletionSource<bool>();
				shell.Navigated += ShellNavigated;

				modalVC.DidDismiss(null);
				await finishedNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));
				Assert.Equal(1, navigatedFired);
				Assert.Equal(ShellNavigationSource.PopToRoot, shellNavigationSource.Value);
				Assert.Empty(shell.Navigation.ModalStack);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
					shellNavigationSource = e.Source;
					finishedNavigation.SetResult(true);
				}
			});
		}

		[Fact(DisplayName = "Clicking BackButton Fires Correct Navigation Events")]
		public async Task ShellWithFlyoutDisabledDoesntRenderFlyout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});


			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var secondPage = new ContentPage();
				await shell.Navigation.PushAsync(new ContentPage())
					.WaitAsync(TimeSpan.FromSeconds(2));

				IShellContext shellContext = handler;
				var sectionRenderer = (shellContext.CurrentShellItemRenderer as ShellItemRenderer)
					.CurrentRenderer as ShellSectionRenderer;

				int navigatingFired = 0;
				int navigatedFired = 0;
				var finishedNavigation = new TaskCompletionSource<bool>();
				ShellNavigationSource? shellNavigationSource = null;

				shell.Navigating += ShellNavigating;
				shell.Navigated += ShellNavigated;
				sectionRenderer.SendPop();
				await finishedNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));
				Assert.Equal(1, navigatingFired);
				Assert.Equal(1, navigatedFired);
				Assert.Equal(ShellNavigationSource.PopToRoot, shellNavigationSource.Value);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
					shellNavigationSource = e.Source;
					finishedNavigation.SetResult(true);
				}

				void ShellNavigating(object sender, ShellNavigatingEventArgs e)
				{
					navigatingFired++;
				}
			});
		}

		[Fact(DisplayName = "Cancel BackButton Navigation")]
		public async Task CancelBackButtonNavigation()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});


			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var secondPage = new ContentPage();
				await shell.Navigation.PushAsync(new ContentPage())
					.WaitAsync(TimeSpan.FromSeconds(2));

				IShellContext shellContext = handler;
				var sectionRenderer = (shellContext.CurrentShellItemRenderer as ShellItemRenderer)
					.CurrentRenderer as ShellSectionRenderer;

				int navigatingFired = 0;
				int navigatedFired = 0;
				ShellNavigationSource? shellNavigationSource = null;

				shell.Navigating += ShellNavigating;
				shell.Navigated += ShellNavigated;
				var finishedNavigation = new TaskCompletionSource<bool>();
				sectionRenderer.SendPop();

				// Give Navigated time to fire just in case
				await Task.Delay(100);
				Assert.Equal(1, navigatingFired);
				Assert.Equal(0, navigatedFired);
				Assert.False(shellNavigationSource.HasValue);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
				}

				void ShellNavigating(object sender, ShellNavigatingEventArgs e)
				{
					navigatingFired++;
					e.Cancel();
				}
			});
		}

		[Fact(DisplayName = "TitleView renders correctly")]
		public async Task TitleViewRendersCorrectly()
		{
			SetupBuilder();

			var expected = Colors.Red;

			var shellTitleView = new VerticalStackLayout { BackgroundColor = expected };
			var titleViewContent = new Label { Text = "TitleView" };
			shellTitleView.Children.Add(titleViewContent);

			var shell = await CreateShellAsync(shell =>
			{
				Shell.SetTitleView(shell, shellTitleView);

				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(100);
				Assert.NotNull(shell.Handler);
				var platformShellTitleView = shellTitleView.ToPlatform();
				Assert.Equal(platformShellTitleView, GetTitleView(handler));
				Assert.NotEqual(platformShellTitleView.Frame, CGRect.Empty);
				Assert.Equal(platformShellTitleView.BackgroundColor.ToColor(), expected);
			});
		}

		[Fact(DisplayName = "TitleView can use constraints to expand area")]
		public async Task TitleViewConstraints()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
					handlers.AddHandler(typeof(Shell), typeof(CustomShellHandler));
				});
			});

			var shellTitleView = new VerticalStackLayout { BackgroundColor = Colors.Green };
			var titleViewContent = new Label { Text = "Full Height TitleView" };
			shellTitleView.Children.Add(titleViewContent);

			var label = new Label { Text = "Page Content", Background = Colors.LightBlue };

			var shell = await CreateShellAsync(shell =>
			{
				Shell.SetTitleView(shell, shellTitleView);

				shell.CurrentItem = new ContentPage()
				{
					Content = new VerticalStackLayout
					{
						Background = Colors.Gray,
						Children = { label }
					}
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await Task.Delay(50);
				var titleView = GetTitleView(handler) as UIView;
				var originalFrame = (handler as CustomShellHandler).PreviousFrame;
				var expandedFrame = titleView.Frame;
				var relativeTitleViewFrame = titleView.ConvertRectToView(titleView.Bounds, null);
				var uiLabel = label.ToPlatform();
				var relativeLabelFrame = uiLabel.ConvertRectToView(uiLabel.Bounds, null);

				// Make sure the titleView is touching the label. Happens by default on iPhone but not on iPad
				Assert.True(relativeTitleViewFrame.Bottom == relativeLabelFrame.Top);
				Assert.True(expandedFrame.Width > originalFrame.Width);
			});
		}

		class CustomShellHandler : ShellRenderer
		{
			protected override IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker() => new CustomShellNavBarAppearanceTracker(this, base.CreateNavBarAppearanceTracker()) { handler = this };

			public CGRect PreviousFrame { get; set; }
		}

		class CustomShellNavBarAppearanceTracker : IShellNavBarAppearanceTracker
		{
			readonly IShellContext _context;
			readonly IShellNavBarAppearanceTracker _baseTracker;

			public CGRect previousFrame { get; set; } = CGRect.Empty;

			public CustomShellHandler handler { get; set; }

			public CustomShellNavBarAppearanceTracker(IShellContext context, IShellNavBarAppearanceTracker baseTracker)
			{
				_context = context;
				_baseTracker = baseTracker;
			}

			public void Dispose() => _baseTracker.Dispose();

			public void ResetAppearance(UINavigationController controller) => _baseTracker.ResetAppearance(controller);

			public void SetAppearance(UINavigationController controller, ShellAppearance appearance) => _baseTracker.SetAppearance(controller, appearance);

			public void SetHasShadow(UINavigationController controller, bool hasShadow) => _baseTracker.SetHasShadow(controller, hasShadow);

			public void UpdateLayout(UINavigationController controller)
			{
				UIView titleView = Shell.GetTitleView(_context.Shell.CurrentPage)?.Handler?.PlatformView as UIView ?? Shell.GetTitleView(_context.Shell)?.Handler?.PlatformView as UIView;

				UIView parentView = GetParentByType(titleView, typeof(UIKit.UIControl));
				handler.PreviousFrame = parentView.Frame;

				if (parentView != null)
				{
					// height constraint
					NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Bottom, 1.0f, 0.0f).Active = true;
					NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Top, 1.0f, 0.0f).Active = true;

					// width constraint
					NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Leading, 1.0f, 0.0f).Active = true;
					NSLayoutConstraint.Create(parentView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, parentView.Superview, NSLayoutAttribute.Trailing, 1.0f, 0.0f).Active = true;
				}
				_baseTracker.UpdateLayout(controller);
			}

			static UIView GetParentByType(UIView view, Type type)
			{
				UIView currentView = view;

				while (currentView != null)
				{
					if (currentView.GetType().UnderlyingSystemType == type)
					{
						break;
					}

					currentView = currentView.Superview;
				}

				return currentView;
			}
		}

		protected async Task OpenFlyout(ShellRenderer shellRenderer, TimeSpan? timeOut = null)
		{
			var flyoutView = GetFlyoutPlatformView(shellRenderer);
			shellRenderer.Shell.FlyoutIsPresented = true;

			await AssertEventually(() => flyoutView.Frame.X == 0, timeOut?.Milliseconds ?? 1000);

			return;
		}

		internal Graphics.Rect GetFrameRelativeToFlyout(ShellRenderer shellRenderer, IView view)
		{
			var platformView = (view.Handler as IPlatformViewHandler).PlatformView;
			return platformView.GetFrameRelativeTo(GetFlyoutPlatformView(shellRenderer));
		}

		protected Task CheckFlyoutState(ShellRenderer renderer, bool result)
		{
			var platformView = GetFlyoutPlatformView(renderer);
			Assert.Equal(result, platformView.Frame.X == 0);
			return Task.CompletedTask;
		}

		protected UIView GetFlyoutPlatformView(ShellRenderer shellRenderer)
		{
			var vcs = shellRenderer.ViewController;
			var flyoutContent = vcs.ChildViewControllers.OfType<ShellFlyoutContentRenderer>().First();
			return flyoutContent.View;
		}

		internal Graphics.Rect GetFlyoutFrame(ShellRenderer shellRenderer)
		{
			var boundingbox = GetFlyoutPlatformView(shellRenderer).GetBoundingBox();

			return new Graphics.Rect(
				0,
				0,
				boundingbox.Width,
				boundingbox.Height);
		}


		protected async Task<double> ScrollFlyoutToBottom(ShellHandler shellHandler)
		{
			var platformView = GetFlyoutPlatformView(shellHandler);
			var scrollView = platformView.FindDescendantView<UIScrollView>();
			var bottomOffset = new CGPoint(0, scrollView.ContentSize.Height - scrollView.Bounds.Height + scrollView.ContentInset.Bottom);

			scrollView.SetContentOffset(bottomOffset, false);

			await Task.Delay(10);

			return bottomOffset.Y;
		}
#if IOS
		[Fact(DisplayName = "Back Button Text Has Correct Default")]
		public async Task BackButtonTextHasCorrectDefault()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage() { Title = "Page 1" };
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await shell.Navigation.PushAsync(new ContentPage() { Title = "Page 2" });
				await OnNavigatedToAsync(shell.CurrentPage);

				await AssertEventually(() => GetBackButtonText(handler) == "Page 1");
			});
		}


		[Fact(DisplayName = "Back Button Behavior Text")]
		public async Task BackButtonBehaviorText()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage() { Title = "Page 1" };
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);

				var page2 = new ContentPage() { Title = "Page 2" };
				var page3 = new ContentPage() { Title = "Page 3" };

				Shell.SetBackButtonBehavior(page3, new BackButtonBehavior() { TextOverride = "Text Override" });
				await shell.Navigation.PushAsync(page2);
				await shell.Navigation.PushAsync(page3);

				await AssertEventually(() => GetBackButtonText(handler) == "Text Override");
				await shell.Navigation.PopAsync();
				await AssertEventually(() => GetBackButtonText(handler) == "Page 1");
			});
		}
#endif
#endif
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
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			var tabController = pagerParent.NextResponder as ShellItemRenderer;

			var section = tabController.SelectedViewController as ShellSectionRenderer;

			var rootCV = section.ViewControllers[0] as
				ShellSectionRootRenderer;

			var rootHeader = rootCV.ChildViewControllers
				.OfType<ShellSectionRootHeader>()
				.First();

			var newIndex = shellSection.Items.IndexOf(shellContent);

			await Task.Delay(100);

			rootHeader.ItemSelected(rootHeader.CollectionView, NSIndexPath.FromItemSection((int)newIndex, 0));

			await OnNavigatedToAsync(page);
		}

		class ModalShellPage : ContentPage
		{
			public ModalShellPage()
			{
				Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);
			}
		}
	}
}
