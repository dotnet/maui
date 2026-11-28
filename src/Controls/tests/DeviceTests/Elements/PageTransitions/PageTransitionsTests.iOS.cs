using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PageTransitionsTests
	{
		// ---- Root replacement (UIKit) ----

		static UIWindow PlatformWindow(Window window) => (UIWindow)window.Handler.PlatformView;

		static UIView NativeRoot(Window window) => PlatformWindow(window).RootViewController?.View;

		static int SnapshotViews(UIWindow window) => window.Subviews.Count(v => v is not null && v.Class.Name.Contains("Replicant", StringComparison.Ordinal));

		void SetupRootBuilder() => SetupBuilder(configureHandlers: handlers => handlers.AddHandler<Window, RootWindowHandler>());

		// WindowHandlerStub presents a workspace without replacing UIWindow.RootViewController.
		sealed class RootWindowHandler : WindowHandler
		{
			public RootWindowHandler() : base(new PropertyMapper<IWindow, IWindowHandler>(WindowHandler.Mapper)
			{
				[nameof(IWindow.Content)] = (handler, window) =>
				{
					WindowHandler.MapContent(handler, window);
					handler.PlatformView.MakeKeyAndVisible();
				}
			})
			{
			}

			protected override UIWindow CreatePlatformElement()
			{
				var host = MauiContext.Services.GetRequiredService<UIWindow>();
				return new UIWindow(host.WindowScene) { Frame = host.Bounds };
			}

			protected override void DisconnectHandler(UIWindow platformView)
			{
				base.DisconnectHandler(platformView);
				platformView.Hidden = true;
				platformView.RootViewController = null;
			}
		}

		[Fact(DisplayName = "A root replacement started during another settles on the newest root (UIKit)")]
		public async Task OverlappingRootReplacementsSettleOnUIKit()
		{
			SetupRootBuilder();
			var first = CreatePage("First");
			var second = CreatePage("Second");
			var third = CreatePage("Third");
			var window = new Window(first);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (_) =>
			{
				await OnLoadedAsync(first.Content);
				var platformWindow = PlatformWindow(window);
				var snapshotsBefore = SnapshotViews(platformWindow);

				// The second replacement starts from the first one's page assignment.
				Task nested = null;
				window.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == Window.PageProperty.PropertyName && ReferenceEquals(window.Page, second) && nested is null)
					{
						nested = window.ReplacePageAsync(third, new FadeTransition { Duration = TimeSpan.FromMilliseconds(400) });
					}
				};

				var outer = window.ReplacePageAsync(second, new SlideTransition { Duration = TimeSpan.FromMilliseconds(800) });
				await outer.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(nested);
				await nested.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.Same(third, window.Page);
				Assert.Equal(snapshotsBefore, SnapshotViews(platformWindow));
				var root = NativeRoot(window);
				Assert.True(root.Transform.IsIdentity, $"Transform left at {root.Transform}");
				Assert.Equal(1, (double)root.Alpha, 3);
			});
		}

		[Fact(DisplayName = "A root replacement started from the assignment's PropertyChanging animates once its own page lands (UIKit)")]
		public async Task RootReplacementQueuedBehindAnotherAssignmentStillAnimates()
		{
			SetupRootBuilder();
			var first = CreatePage("First");
			var second = CreatePage("Second");
			var third = CreatePage("Third");
			var window = new Window(first);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (_) =>
			{
				await OnLoadedAsync(first.Content);
				var platformWindow = PlatformWindow(window);
				var snapshotsBefore = SnapshotViews(platformWindow);

				// Queued behind the assignment of second, whose change is raised first.
				Task queued = null;
				Microsoft.Maui.Controls.PropertyChangingEventHandler onChanging = null;
				onChanging = (_, e) =>
				{
					if (e.PropertyName == Window.PageProperty.PropertyName)
					{
						window.PropertyChanging -= onChanging;
						queued = window.ReplacePageAsync(third, new FadeTransition { Duration = TimeSpan.FromMilliseconds(600) });
					}
				};
				window.PropertyChanging += onChanging;

				var outer = window.ReplacePageAsync(second, new SlideTransition { Duration = TimeSpan.FromMilliseconds(400) });
				Assert.NotNull(queued);

				// The queued replacement still animates: a picture of the old root is over the window while it runs.
				var animated = false;
				var watch = Stopwatch.StartNew();
				while (!queued.IsCompleted && !animated && watch.Elapsed < TimeSpan.FromSeconds(2))
				{
					animated = SnapshotViews(platformWindow) > snapshotsBefore;
					await Task.Yield();
				}

				await outer.WaitAsync(TimeSpan.FromSeconds(10));
				await queued.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.True(animated, "The queued replacement swapped the root without animating.");
				Assert.Same(third, window.Page);
				Assert.Equal(snapshotsBefore, SnapshotViews(platformWindow));
			});
		}

		[Fact(DisplayName = "Opacity set on the incoming root during a replacement is kept (UIKit)")]
		public async Task OpacitySetDuringARootReplacementIsKept()
		{
			SetupRootBuilder();
			var first = CreatePage("First");
			var navPage = new NavigationPage(CreatePage("Root"));
			var window = new Window(first);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (_) =>
			{
				await OnLoadedAsync(first.Content);

				var replace = window.ReplacePageAsync(navPage, new FadeTransition { Duration = TimeSpan.FromMilliseconds(800) });
				await Task.Delay(200);
				navPage.Opacity = 0.25;
				navPage.TranslationX = 10;
				await replace.WaitAsync(TimeSpan.FromSeconds(10));

				var view = ((IPlatformViewHandler)navPage.Handler).ContainerView ?? ((IPlatformViewHandler)navPage.Handler).PlatformView;
				Assert.Equal(0.25, (double)view.Alpha, 3);
				Assert.Equal(10, (double)view.Layer.Transform.M41, 3);
			});
		}

		[Fact(DisplayName = "A root replacement leaves the incoming root's own animations running (UIKit)")]
		public async Task RootReplacementKeepsTheRootsOwnAnimations()
		{
			SetupRootBuilder();
			var first = CreatePage("First");
			var second = CreatePage("Second");
			var window = new Window(first);
			const string ownKey = "pagetransitions-test-own";

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (_) =>
			{
				await OnLoadedAsync(first.Content);

				var outer = window.ReplacePageAsync(second, new SlideTransition { Duration = TimeSpan.FromSeconds(1) });
				// PropertyChanged precedes the native root mapping.
				var animatedLayer = NativeRoot(window).Layer;
				animatedLayer.AddAnimation(new CABasicAnimation
				{
					KeyPath = "shadowOpacity",
					From = NSNumber.FromFloat(0),
					To = NSNumber.FromFloat(1),
					Duration = 1,
					RepeatCount = float.PositiveInfinity,
				}, ownKey);
				Assert.NotNull(animatedLayer.AnimationForKey(ownKey));
				await Task.Delay(200);

				// Finish without removing the root (UIKit itself removes animations from an outgoing root).
				RootPageTransition.FinishRunning(window);
				await outer.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.NotNull(animatedLayer.AnimationForKey(ownKey));
				animatedLayer.RemoveAnimation(ownKey);
			});
		}

		// ---- Shared-element snapshots (UIKit) ----

		sealed class TestTransitionContext : NSObject, IUIViewControllerContextTransitioning
		{
			readonly UIView _from;
			readonly UIView _to;

			public TestTransitionContext(UIView container, UIView from, UIView to, bool cancelled)
			{
				ContainerView = container;
				_from = from;
				_to = to;
				TransitionWasCancelled = cancelled;
			}

			public TaskCompletionSource<bool> Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			public UIView ContainerView { get; }

			public bool IsAnimated => true;

			public bool IsInteractive => false;

			public bool TransitionWasCancelled { get; }

			public UIModalPresentationStyle PresentationStyle => UIModalPresentationStyle.None;

			public CGAffineTransform TargetTransform => CGAffineTransform.MakeIdentity();

			public void UpdateInteractiveTransition(nfloat percentComplete)
			{
			}

			public void FinishInteractiveTransition()
			{
			}

			public void CancelInteractiveTransition()
			{
			}

			public void PauseInteractiveTransition()
			{
			}

			public void CompleteTransition(bool didComplete) => Completed.TrySetResult(didComplete);

			public UIViewController GetViewControllerForKey(NSString uiTransitionKey) => null;

			public CGRect GetInitialFrameForViewController(UIViewController vc) => ContainerView.Bounds;

			public CGRect GetFinalFrameForViewController(UIViewController vc) => ContainerView.Bounds;

			public UIView GetViewFor(NSString uiTransitionContextToOrFromKey)
				=> uiTransitionContextToOrFromKey == UITransitionContext.ToViewKey ? _to : _from;
		}

		[Theory(DisplayName = "Shared-element snapshots are removed and both elements restored when a transition completes or is cancelled")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task SharedElementSnapshotsAreRemoved(bool cancelled)
		{
			SetupBuilder();
			var source = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 80 };
			var target = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 200 };
			SharedTransition.SetName(source, "photo");
			SharedTransition.SetName(target, "photo");
			var page = new ContentPage { Content = new VerticalStackLayout { source, target } };

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(page), async (_) =>
			{
				await OnLoadedAsync(target);
				var container = (page.Handler as IPlatformViewHandler).PlatformView;
				var from = new UIView(container.Bounds);
				var to = new UIView(container.Bounds);
				container.AddSubview(from);

				var request = new NavigationTransitionRequest(NavigationTransitionKind.Pop, new ContentPage(), new ContentPage(), new FadeTransition(), isInteractive: false, isBack: true, FlowDirection.LeftToRight);
				var animator = new PageTransitionAnimator(request, new FadeTransition { Duration = TimeSpan.FromMilliseconds(300) }, new List<(VisualElement From, VisualElement To)> { (source, target) });
				var context = new TestTransitionContext(container, from, to, cancelled);
				var snapshotsBefore = PageTransitionAnimator.HeroFlight.ActiveSnapshots;

				animator.AnimateTransition(context);

				// Both real elements are hidden under the flying snapshots.
				Assert.Equal(snapshotsBefore + 2, PageTransitionAnimator.HeroFlight.ActiveSnapshots);
				Assert.False(NativeShown(source));
				Assert.False(NativeShown(target));

				var completed = await context.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

				Assert.Equal(!cancelled, completed);
				Assert.Equal(snapshotsBefore, PageTransitionAnimator.HeroFlight.ActiveSnapshots);
				Assert.True(NativeShown(source), "The source was left hidden.");
				Assert.True(NativeShown(target), "The target was left hidden.");
				Assert.Equal(cancelled, to.Superview is null);
				from.RemoveFromSuperview();
				to.RemoveFromSuperview();
			});
		}

		// ---- Shell's More tab ----

		static Shell ShellWithMoreTab(ContentPage overflowRoot)
		{
			// Six tabs: the sixth is under the system More tab on a compact width.
			var tabBar = new TabBar();
			for (int i = 0; i < 5; i++)
			{
				tabBar.Items.Add(new Tab { Title = $"Tab {i}", Items = { new ShellContent { Content = CreatePage($"Tab {i}") } } });
			}

			var overflow = new Tab { Title = "Overflow", Items = { new ShellContent { Content = overflowRoot } } };
			tabBar.Items.Add(overflow);
			var shell = new Shell { Items = { tabBar } };
			shell.CurrentItem = overflow;
			return shell;
		}

		void SetupMoreTab(bool useShellHandler) => SetupBuilder(configureHandlers: handlers =>
		{
			if (useShellHandler)
			{
				handlers.AddHandler(typeof(Shell), typeof(Microsoft.Maui.Controls.Handlers.ShellHandler));
				handlers.AddHandler(typeof(ShellItem), typeof(Microsoft.Maui.Controls.Handlers.ShellItemHandler));
				handlers.AddHandler(typeof(ShellSection), typeof(Microsoft.Maui.Controls.Handlers.ShellSectionHandler));
				handlers.AddHandler(typeof(ShellContent), typeof(Microsoft.Maui.Controls.Handlers.ShellContentHandler));
			}
			else
			{
				handlers.AddHandler(typeof(Shell), typeof(Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer));
			}
		});

		static readonly TimeSpan MoreTransitionDuration = TimeSpan.FromSeconds(1);

		[Theory(DisplayName = "Under the More tab, push, pop and pop-to-root complete after a page transition")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task MoreTabNavigationsCompleteAfterTheTransition(bool useShellHandler)
		{
			SetupMoreTab(useShellHandler);
			var root = CreatePage("Overflow root");
			var shell = ShellWithMoreTab(root);
			var first = CreatePage("First");
			var second = CreatePage("Second");
			PageTransitions.SetTransition(first, new SlideTransition { Duration = MoreTransitionDuration });
			PageTransitions.SetTransition(second, new SlideTransition { Duration = MoreTransitionDuration });
			var navigatedTo = false;
			first.NavigatedTo += (_, _) => navigatedTo = true;

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(shell), async (_) =>
			{
				await OnNavigatedToAsync(root);

				var watch = Stopwatch.StartNew();
				var push = shell.Navigation.PushAsync(first);
				await Task.WhenAny(push, Task.Delay(MoreTransitionDuration * 0.5));
				Assert.False(push.IsCompleted, $"Push completed after {watch.Elapsed}, before its transition ended.");
				Assert.False(navigatedTo, "NavigatedTo was raised before the transition ended.");
				await push.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(navigatedTo);

				watch.Restart();
				await shell.Navigation.PopAsync().WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(watch.Elapsed >= MoreTransitionDuration * 0.8, $"Pop completed after {watch.Elapsed}.");

				await shell.Navigation.PushAsync(first, false);
				await OnLoadedAsync(first.Content);
				await shell.Navigation.PushAsync(second).WaitAsync(TimeSpan.FromSeconds(10));
				await OnLoadedAsync(second.Content);
				watch.Restart();
				await shell.Navigation.PopToRootAsync().WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(watch.Elapsed >= MoreTransitionDuration * 0.8, $"Pop to root completed after {watch.Elapsed}.");
				Assert.Same(root, shell.CurrentPage);
			});
		}

		[Theory(DisplayName = "Under the More tab, UIKit's own animation still completes straight away")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task MoreTabPlatformNavigationCompletesStraightAway(bool useShellHandler)
		{
			SetupMoreTab(useShellHandler);
			var root = CreatePage("Overflow root");
			var shell = ShellWithMoreTab(root);
			var detail = CreatePage("Detail");

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(shell), async (_) =>
			{
				await OnNavigatedToAsync(root);

				var watch = Stopwatch.StartNew();
				await shell.Navigation.PushAsync(detail).WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(300), $"Push completed after {watch.Elapsed}; UIKit's animation isn't waited for under More.");
			});
		}

		[Theory(DisplayName = "Under the More tab, an unanimated push is recorded, so the pop reverses None")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task MoreTabUnanimatedPushIsRecordedForThePop(bool useShellHandler)
		{
			SetupMoreTab(useShellHandler);
			var root = CreatePage("Overflow root");
			var shell = ShellWithMoreTab(root);
			var detail = CreatePage("Detail");
			PageTransitions.SetTransition(detail, new FadeTransition { Duration = MoreTransitionDuration });

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(shell), async (_) =>
			{
				await OnNavigatedToAsync(root);
				await shell.Navigation.PushAsync(detail, PageTransition.None).WaitAsync(TimeSpan.FromSeconds(10));
				await OnNavigatedToAsync(detail);

				// Shown with None, the pop reverses None rather than the page's Fade.
				var watch = Stopwatch.StartNew();
				await shell.Navigation.PopAsync().WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(watch.Elapsed < MoreTransitionDuration * 0.5, $"Pop took {watch.Elapsed}: it ran the page's Fade.");
				Assert.Same(root, shell.CurrentPage);
			});
		}

		[Theory(DisplayName = "Under the More tab, a shared element on the overflow root flies")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task MoreTabSharedElementFliesFromTheOverflowRoot(bool useShellHandler)
		{
			SetupMoreTab(useShellHandler);
			var source = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 80 };
			var target = new BoxView { Color = Graphics.Colors.SteelBlue, HeightRequest = 200 };
			SharedTransition.SetName(source, "photo");
			SharedTransition.SetName(target, "photo");
			var root = new ContentPage { Content = new VerticalStackLayout { source } };
			var detail = new ContentPage { Content = new VerticalStackLayout { target } };
			PageTransitions.SetTransition(detail, new SlideTransition { Duration = MoreTransitionDuration });
			var shell = ShellWithMoreTab(root);

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(shell), async (_) =>
			{
				await OnLoadedAsync(source);
				var snapshotsBefore = PageTransitionAnimator.HeroFlight.ActiveSnapshots;
				var push = shell.Navigation.PushAsync(detail);

				var flew = false;
				while (!push.IsCompleted && !flew)
				{
					flew = PageTransitionAnimator.HeroFlight.ActiveSnapshots > snapshotsBefore;
					await Task.Yield();
				}

				await push.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.True(flew, "No shared-element flight ran from the overflow section's root.");
			});
		}
	}
}
