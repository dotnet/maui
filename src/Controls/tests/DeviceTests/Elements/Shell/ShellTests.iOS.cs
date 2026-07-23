using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
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

#pragma warning disable CS0618 // Type or member is obsolete
			pageWithTopTabs.On<iOS>().SetUseSafeArea(true);
			pageWithoutTopTabs.On<iOS>().SetUseSafeArea(true);
#pragma warning restore CS0618 // Type or member is obsolete

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

		[Theory(DisplayName = "SafeArea works with both old and new APIs")]
		[InlineData(true)] // Use new SafeArea API
		[InlineData(false)] // Use old UseSafeArea API
		public async Task SafeAreaBehaviorConsistency(bool useNewApi)
		{
			SetupBuilder();
			var pageWithTopTabs = new ContentPage() { Content = new Label() { Text = "Page With Top Tabs" } };
			var pageWithoutTopTabs = new ContentPage() { Content = new Label() { Text = "Page With Bottom Tabs" } };

			if (useNewApi)
			{
				pageWithTopTabs.SafeAreaEdges = SafeAreaEdges.Container;
				pageWithoutTopTabs.SafeAreaEdges = SafeAreaEdges.Container;
			}
			else
			{
#pragma warning disable CS0618 // Type or member is obsolete
				pageWithTopTabs.On<iOS>().SetUseSafeArea(true);
				pageWithoutTopTabs.On<iOS>().SetUseSafeArea(true);
#pragma warning restore CS0618 // Type or member is obsolete
			}

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

				// Both APIs should produce consistent results
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
			// Skip on iOS 26+ due to UINavigationBar internal API changes
			// See: https://github.com/dotnet/maui/issues/33004
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;

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

				if (parentView != null)
				{
					handler.PreviousFrame = parentView.Frame;

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
			// Skip on iOS 26+ due to UINavigationBar internal API changes
			// See: https://github.com/dotnet/maui/issues/33004
			if (OperatingSystem.IsIOSVersionAtLeast(26))
				return;

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
			// Skip on iOS 26+ due to UINavigationBar internal API changes
			// See: https://github.com/dotnet/maui/issues/33004
			if (OperatingSystem.IsIOSVersionAtLeast(26))
				return;

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
		[Fact(DisplayName = "Shell Section Delegates Do Not Retain Renderer")]
		public async Task ShellSectionDelegatesDoNotRetainRenderer()
		{
			var references = await InvokeOnMainThreadAsync(CreateShellSectionDelegateReferences);

			Assert.NotNull(references.NavigationDelegate);
			Assert.NotNull(references.GestureDelegate);
			await AssertionExtensions.WaitForGC(references.Renderer);
			GC.KeepAlive(references.NavigationDelegate);
			GC.KeepAlive(references.GestureDelegate);
		}

		[Fact(DisplayName = "Disposed Shell Section Root Ignores Late Layout")]
		public async Task DisposedShellSectionRootIgnoresLateLayout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async handler =>
			{
				await OnLoadedAsync(shell.CurrentPage);

				IShellContext shellContext = handler;
				var shellItemRenderer = Assert.IsType<ShellItemRenderer>(shellContext.CurrentShellItemRenderer);
				var sectionRenderer = Assert.IsType<ShellSectionRenderer>(shellItemRenderer.CurrentRenderer);
				var rootRenderer = Assert.IsType<ShellSectionRootRenderer>(sectionRenderer.ViewControllers[0]);

				rootRenderer.Dispose();
				rootRenderer.ViewDidLayoutSubviews();
			});
		}

		[Fact(DisplayName = "Disposed Shell Flyout Content Ignores Late Lifecycle Callbacks")]
		public async Task DisposedShellFlyoutContentIgnoresLateLifecycleCallbacks()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, handler =>
			{
				var flyoutContent = handler.ViewController
					.ChildViewControllers
					.OfType<ShellFlyoutContentRenderer>()
					.First();

				flyoutContent.Dispose();
				flyoutContent.ViewDidLoad();
				flyoutContent.ViewWillAppear(false);
				flyoutContent.ViewWillLayoutSubviews();
				flyoutContent.ViewDidLayoutSubviews();
				flyoutContent.ViewWillDisappear(false);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Disposed Shell Flyout Content Disconnects Header And Footer Handlers")]
		public async Task DisposedShellFlyoutContentDisconnectsHeaderAndFooterHandlers()
		{
			SetupBuilder();
			var header = new Label { Text = "Header" };
			var footer = new Label { Text = "Footer" };
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ContentPage());
				shell.FlyoutHeader = header;
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, handler =>
			{
				var flyoutContent = handler.ViewController
					.ChildViewControllers
					.OfType<ShellFlyoutContentRenderer>()
					.First();
				var headerPlatformView = header.ToPlatform();
				var footerPlatformView = footer.ToPlatform();

				Assert.NotNull(header.Handler);
				Assert.NotNull(footer.Handler);
				Assert.NotNull(headerPlatformView.Superview);
				Assert.NotNull(footerPlatformView.Superview);

				flyoutContent.Dispose();

				Assert.Null(header.Handler);
				Assert.Null(footer.Handler);
				Assert.Null(headerPlatformView.Superview);
				Assert.Null(footerPlatformView.Superview);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Disposed Shell Table Source Clears Scrolled Subscribers")]
		public async Task DisposedShellTableSourceClearsScrolledSubscribers()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, handler =>
			{
				var flyoutContent = handler.ViewController
					.ChildViewControllers
					.OfType<ShellFlyoutContentRenderer>()
					.First();
				var tableViewController = flyoutContent.ChildViewControllers
					.OfType<ShellTableViewController>()
					.First();
				var source = Assert.IsType<ShellTableViewSource>(tableViewController.TableView.Source);
				var scrolled = false;
				source.ScrolledEvent += OnScrolled;

				flyoutContent.Dispose();

				using var nativeScrollView = new UIScrollView();
				source.Scrolled(nativeScrollView);
				Assert.False(scrolled);

				return Task.CompletedTask;

				void OnScrolled(object sender, UIScrollView view)
				{
					scrolled = true;
				}
			});
		}

		[Fact(DisplayName = "Shell Flyout Renderer Disposal Is Idempotent After Native Teardown")]
		public Task ShellFlyoutRendererDisposalIsIdempotentAfterNativeTeardown() =>
			InvokeOnMainThreadAsync(() =>
			{
				var renderer = new TestableShellFlyoutRenderer();

				renderer.DisposeForTest(false);
				renderer.DisposeForTest(true);
			});

		[Fact(DisplayName = "Disconnect Shell During Current Item Change Does Not Recreate Renderer")]
		public async Task DisconnectShellDuringCurrentItemChangeDoesNotRecreateRenderer()
		{
			SetupBuilder(typeof(TestableShellRenderer));
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ContentPage());
				shell.Items.Add(new ContentPage());
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				using var handler = (TestableShellRenderer)shell.ToHandler(MauiContext);
				var activeTransitionField = typeof(ShellHandler).GetField("_activeTransition", BindingFlags.Instance | BindingFlags.NonPublic);
				var currentRendererField = typeof(ShellHandler).GetField("_currentShellItemRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
				var incomingRendererField = typeof(ShellHandler).GetField("_incomingRenderer", BindingFlags.Instance | BindingFlags.NonPublic);

				Assert.NotNull(activeTransitionField);
				Assert.NotNull(currentRendererField);
				Assert.NotNull(incomingRendererField);

				var transition = new TaskCompletionSource<bool>();
				activeTransitionField.SetValue(handler, transition.Task);

				shell.CurrentItem = shell.Items[1];
				var currentItemChanged = handler.CurrentItemChangedTask;
				Assert.False(currentItemChanged.IsCompleted);
				((IElementHandler)handler).DisconnectHandler();
				transition.SetResult(true);
				await currentItemChanged;

				Assert.Null(handler.Element);
				Assert.Null(shell.Handler);
				Assert.Null(currentRendererField.GetValue(handler));
				Assert.Null(incomingRendererField.GetValue(handler));
			});
		}

		[Fact(DisplayName = "Disconnect Shell Detaches Current Item Renderer Before Disposal")]
		public Task DisconnectShellDetachesCurrentItemRendererBeforeDisposal() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var handler = new TestableShellRenderer();
				using var parentViewController = new UIViewController();
				var currentRendererField = typeof(ShellHandler).GetField("_currentShellItemRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(currentRendererField);

				var renderer = new TrackedShellItemRenderer();
				parentViewController.AddChildViewController(renderer.ViewController);
				parentViewController.View.AddSubview(renderer.ViewController.View);
				currentRendererField.SetValue(handler, renderer);

				((IElementHandler)handler).DisconnectHandler();

				Assert.Null(renderer.ParentAtDispose);
				Assert.Null(renderer.SuperviewAtDispose);
				Assert.Equal(1, renderer.DisconnectCount);
				Assert.Equal(1, renderer.DisposeCount);
			});

		[Fact(DisplayName = "Disconnect Shell Cancels Active Item Transition Before Disposal")]
		public Task DisconnectShellCancelsActiveItemTransitionBeforeDisposal() =>
			InvokeOnMainThreadAsync(async () =>
			{
				using var handler = new TestableShellRenderer();
				var currentRendererField = typeof(ShellHandler).GetField("_currentShellItemRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
				var elementProperty = typeof(ShellHandler).GetProperty(nameof(ShellHandler.Element), BindingFlags.Instance | BindingFlags.Public);
				Assert.NotNull(currentRendererField);
				Assert.NotNull(elementProperty);

				var shell = new Shell();
				shell.Items.Add(new ContentPage());
				shell.Items.Add(new ContentPage());
				shell.CurrentItem = shell.Items[1];
				elementProperty.SetValue(handler, shell);

				var oldRenderer = new TrackedShellItemRenderer { ShellItem = shell.Items[0] };
				var newRenderer = new TrackedShellItemRenderer { ShellItem = shell.Items[1] };
				var transition = new ControlledShellItemTransition();
				handler.ShellItemTransition = transition;
				currentRendererField.SetValue(handler, oldRenderer);

				var setCurrentItem = handler.SetCurrentShellItemControllerForTestAsync(newRenderer);
				await transition.Started.Task.WaitAsync(TimeSpan.FromSeconds(1));
				Assert.NotEmpty(newRenderer.ViewController.View.Layer.AnimationKeys);

				((IElementHandler)handler).DisconnectHandler();

				var completedTask = await Task.WhenAny(setCurrentItem, Task.Delay(TimeSpan.FromSeconds(1)));
				transition.Complete();
				await setCurrentItem;

				Assert.Same(setCurrentItem, completedTask);
				Assert.False(newRenderer.HadAnimationsAtDispose);
				Assert.Equal(1, oldRenderer.DisconnectCount);
				Assert.Equal(1, oldRenderer.DisposeCount);
				Assert.Equal(1, newRenderer.DisconnectCount);
				Assert.Equal(1, newRenderer.DisposeCount);
			});

		[Fact(DisplayName = "Disconnect Shell Disposes All Pending Item Renderers")]
		public Task DisconnectShellDisposesAllPendingItemRenderers() =>
			InvokeOnMainThreadAsync(async () =>
			{
				using var handler = new TestableShellRenderer();
				var activeTransitionField = typeof(ShellHandler).GetField("_activeTransition", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(activeTransitionField);

				var transition = new TaskCompletionSource<bool>();
				activeTransitionField.SetValue(handler, transition.Task);

				var firstRenderer = new TrackedShellItemRenderer();
				var secondRenderer = new TrackedShellItemRenderer();
				var firstTransition = handler.SetCurrentShellItemControllerForTestAsync(firstRenderer);
				var secondTransition = handler.SetCurrentShellItemControllerForTestAsync(secondRenderer);

				((IElementHandler)handler).DisconnectHandler();
				transition.SetResult(true);
				await Task.WhenAll(firstTransition, secondTransition);

				Assert.Equal(1, firstRenderer.DisconnectCount);
				Assert.Equal(1, firstRenderer.DisposeCount);
				Assert.Equal(1, secondRenderer.DisconnectCount);
				Assert.Equal(1, secondRenderer.DisposeCount);
			});

		[Fact(DisplayName = "Stale Shell Flyout Background Image Is Disposed")]
		public async Task StaleShellFlyoutBackgroundImageIsDisposed()
		{
			SetupBuilder();
			var imageSource = new DelayedImageSource();
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ContentPage());
				shell.FlyoutBackgroundImage = imageSource;
			});
			var imageSourceServiceProvider = MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = imageSourceServiceProvider.GetRequiredImageSourceService<DelayedImageSource>();
			var delayedImageSourceService = Assert.IsType<DelayedImageSourceService>(imageSourceService);

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async _ =>
			{
				try
				{
					Assert.True(await Task.Run(() => delayedImageSourceService.Starting.WaitOne(TimeSpan.FromSeconds(5))));
					shell.FlyoutBackgroundImage = null;
				}
				finally
				{
					delayedImageSourceService.DoWork.Set();
				}

				Assert.True(await Task.Run(() => delayedImageSourceService.Disposed.WaitOne(TimeSpan.FromSeconds(5))));
			});
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static ShellSectionDelegateReferences CreateShellSectionDelegateReferences()
		{
			var renderer = new ShellSectionRenderer(new TestShellContext());
			renderer.LoadViewIfNeeded();

			var references = new ShellSectionDelegateReferences(
				new WeakReference(renderer),
				renderer.Delegate,
				renderer.InteractivePopGestureRecognizer.Delegate);

			((IDisconnectable)renderer).Disconnect();
			return references;
		}

		readonly record struct ShellSectionDelegateReferences(
			WeakReference Renderer,
			object NavigationDelegate,
			object GestureDelegate);

		sealed class TestShellContext : IShellContext
		{
			public bool AllowFlyoutGesture => true;
			public IShellItemRenderer CurrentShellItemRenderer => null;
			public Shell Shell { get; } = new();
			public IMauiContext MauiContext => null;

			public IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker() => null;
			public IShellPageRendererTracker CreatePageRendererTracker() => null;
			public IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer() => null;
			public IShellSearchResultsRenderer CreateShellSearchResultsRenderer() => null;
			public IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection) => null;
			public IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker() => null;
		}

		sealed class TestableShellRenderer : ShellHandler
		{
			public Task CurrentItemChangedTask { get; private set; } = Task.CompletedTask;
			public IShellItemTransition ShellItemTransition { get; set; }

			public Task SetCurrentShellItemControllerForTestAsync(IShellItemRenderer renderer) =>
				SetCurrentShellItemControllerAsync(renderer);

			protected override IShellItemTransition CreateShellItemTransition() =>
				ShellItemTransition ?? base.CreateShellItemTransition();

			protected override void OnCurrentItemChanged()
			{
				CurrentItemChangedTask = OnCurrentItemChangedAsync();
			}
		}

		sealed class TrackedShellItemRenderer : IShellItemRenderer, IDisconnectable
		{
			public int DisconnectCount { get; private set; }
			public int DisposeCount { get; private set; }
			public bool HadAnimationsAtDispose { get; private set; }
			public UIViewController ParentAtDispose { get; private set; }
			public UIView SuperviewAtDispose { get; private set; }
			public ShellItem ShellItem { get; set; }
			public UIViewController ViewController { get; } = new UIViewController();

			public void Disconnect() => DisconnectCount++;

			public void Dispose()
			{
				HadAnimationsAtDispose = ViewController.ViewIfLoaded?.Layer.AnimationKeys?.Length > 0;
				ParentAtDispose = ViewController.ParentViewController;
				SuperviewAtDispose = ViewController.ViewIfLoaded?.Superview;
				DisposeCount++;
				ViewController.Dispose();
			}
		}

		sealed class ControlledShellItemTransition : IShellItemTransition
		{
			readonly TaskCompletionSource<bool> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			public Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
			{
				using var animation = CABasicAnimation.FromKeyPath("opacity");
				animation.From = NSNumber.FromDouble(0);
				animation.To = NSNumber.FromDouble(1);
				animation.Duration = 60;
				newRenderer.ViewController.View.Layer.AddAnimation(animation, "active-shell-item-transition");
				Started.TrySetResult(true);
				return _completion.Task;
			}

			public void Complete() => _completion.TrySetResult(true);
		}

		sealed class TestableShellFlyoutRenderer : ShellFlyoutRenderer
		{
			public void DisposeForTest(bool disposing) => base.Dispose(disposing);
		}

		interface IDelayedImageSource : IImageSource
		{
		}

		sealed class DelayedImageSource : ImageSource, IDelayedImageSource
		{
		}

		sealed class DelayedImageSourceService : IImageSourceService<IDelayedImageSource>
		{
			public AutoResetEvent Starting { get; } = new(false);
			public AutoResetEvent DoWork { get; } = new(false);
			public AutoResetEvent Disposed { get; } = new(false);

			public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default) =>
				GetImageAsync((IDelayedImageSource)imageSource, scale, cancellationToken);

			public async Task<IImageSourceServiceResult<UIImage>> GetImageAsync(
				IDelayedImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				Starting.Set();
				await Task.Run(() => DoWork.WaitOne());
				var image = await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() => new UIImage());
				return new ImageSourceServiceResult(image, () =>
				{
					image.Dispose();
					Disposed.Set();
				});
			}
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

		[Fact(DisplayName = "Shell Flyout Table View Has ScrollsToTop Disabled")]
		public async Task ShellFlyoutTableViewScrollsToTopIsDisabled()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage() { Content = new Label() { Text = "Test Page" } });
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, (handler) =>
			{
				var flyoutContent = handler.ViewController
					.ChildViewControllers
					.OfType<ShellFlyoutContentRenderer>()
					.First();

				var tableViewController = flyoutContent.ChildViewControllers
					.OfType<ShellTableViewController>()
					.FirstOrDefault();

				Assert.NotNull(tableViewController);
				Assert.False(tableViewController.TableView.ScrollsToTop,
					"Shell flyout's table view should have ScrollsToTop disabled to avoid conflicting with content scroll views");

				return Task.CompletedTask;
			});
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
