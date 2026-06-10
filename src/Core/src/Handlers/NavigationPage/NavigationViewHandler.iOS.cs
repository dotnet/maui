#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, UIView>, IPlatformViewHandler
	{
		NavigationControllerManager? _navManager;

		/// <summary>
		/// The UINavigationBar subclass to use when creating the UINavigationController.
		/// Defaults to UINavigationBar. Controls layer can override to MauiNavigationBar.
		/// </summary>
		internal static Type NavigationBarType { get; set; } = typeof(UINavigationBar);

		/// <summary>
		/// Optional factory set by the Controls layer to create a wrapper VC for each page.
		/// When set, replaces the default handler-based VC creation with a ParentingViewController
		/// that manages toolbar items, nav bar visibility, back button, etc.
		/// </summary>
		internal static Func<IView, IMauiContext, UIViewController>? CreateViewControllerForPage { get; set; }

		/// <summary>
		/// Optional callback invoked after a UIKit-initiated pop (back button, swipe, long-press
		/// history menu) has completed and the MAUI stack is synced. The Controls layer sets this
		/// to fire page lifecycle events (OnNavigatedTo/OnNavigatedFrom) via SendNavigatedFromHandler.
		/// Parameters: (IStackNavigationView navigationView, IView poppedPage)
		/// </summary>
		internal static Action<IStackNavigationView, IView>? OnNativePopCompleted { get; set; }

		/// <summary>
		/// Optional callback invoked when the UINavigationController's ViewDidAppear fires.
		/// The Controls layer sets this to re-evaluate the Loaded state on the NavigationPage
		/// element, ensuring the Loaded event fires when hosted inside a parent container
		/// (TabbedPage, FlyoutPage) where the initial KVO-based watcher may miss the window assignment.
		/// Also fires SendAppearing on the NavigationPage element for tab switch scenarios.
		/// </summary>
		internal static Action<IStackNavigationView>? OnNavigationControllerDidAppear { get; set; }

		/// <summary>
		/// Optional callback invoked when the UINavigationController's ViewDidDisappear fires.
		/// The Controls layer sets this to fire SendDisappearing on the NavigationPage element
		/// when the user switches away from this tab or the NavigationPage is otherwise hidden.
		/// </summary>
		internal static Action<IStackNavigationView>? OnNavigationControllerDidDisappear { get; set; }

		public IStackNavigationView NavigationView => ((IStackNavigationView)VirtualView);

		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		// Maps IView → UIViewController for the navigation stack
		readonly Dictionary<IView, UIViewController> _viewControllerMap = new();

		// The MAUI stack expected after the in-flight UIKit navigation (push or pop) completes.
		// Set in RequestNavigation before calling PushViewController/PopViewController;
		// consumed in OnNavigationComplete (DidShowViewController) to call NavigationFinished.
		// When null, DidShowViewController was triggered by a native pop (back button/swipe).
		List<IView>? _pendingNavigationStack;

		/// <summary>
		/// The managed UINavigationController, exposed for Controls-layer appearance helpers.
		/// </summary>
		internal UINavigationController? NavigationController => _navManager?.NavigationController;

		UIViewController IPlatformViewHandler.ViewController => _navManager?.NavigationController ?? throw new InvalidOperationException("Handler not connected");

		protected override UIView CreatePlatformView()
		{
			_navManager = new NavigationControllerManager(NavigationBarType, new NavigationViewDelegate(this));
			return _navManager.NavigationController.View!;
		}

		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);
			_navManager!.SetupInteractivePopGesture();
		}

		private protected override void OnDisconnectHandler(UIView platformView)
		{
			_navManager?.Dispose();
			_navManager = null;

			foreach (var kvp in _viewControllerMap)
			{
				kvp.Value.View?.RemoveFromSuperview();
			}
			_viewControllerMap.Clear();
			NavigationStack = new List<IView>();

			base.OnDisconnectHandler(platformView);
		}

		void RequestNavigation(NavigationRequest args)
		{
			if (_navManager is not NavigationControllerManager navManager)
			{
				return;
			}

			var newStack = args.NavigationStack;
			var oldStack = NavigationStack;
			bool animated = args.Animated;


			if (newStack.Count == 0)
			{
				return;
			}

			// Determine what navigation operation(s) to perform
			if (oldStack.Count == 0)
			{
				// Initial load — push all pages
				for (int index = 0; index < newStack.Count; index++)
				{
					var vc = GetOrCreateViewController(newStack[index]);

					if (index == 0)
					{
						navManager.NavigationController.SetViewControllers(new[] { vc }, false);
					}
					else
					{
						var tcs = navManager.PushViewController(vc, animated && index == newStack.Count - 1);
						if (!animated || index < newStack.Count - 1)
						{
							navManager.CompletePushImmediately(vc);
						}
					}
				}
			}
			else if (newStack.Count >= oldStack.Count && newStack[newStack.Count - 1] != oldStack[oldStack.Count - 1])
			{
				// Push — top page changed and stack grew or stayed same size
				// First handle any mid-stack inserts (exclude top page — it will be pushed separately)
				if (newStack.Count > 1)
				{
					var midStack = newStack.Take(newStack.Count - 1).ToList();
					SyncMiddleOfStack(midStack);
				}

				// Push the new top page.
				// Store expected stack; OnNavigationComplete (DidShowViewController) will call
				// NavigationFinished once the push animation finishes.
				var topView = newStack[newStack.Count - 1];
				var vc = GetOrCreateViewController(topView);
				_pendingNavigationStack = new List<IView>(newStack);
				navManager.PushViewController(vc, animated);
				return;
			}
			else if (newStack.Count < oldStack.Count)
			{
				// Pop — stack shrunk

				if (newStack[newStack.Count - 1] != oldStack[oldStack.Count - 1])
				{
					// Top page changed — pop with animation.
					// Before popping, sync mid-stack changes (inserts/removals below the top).
					// We must keep the OLD top VC in the stack so PopViewController can
					// pop it with animation — otherwise SyncMiddleOfStack would remove it
					// and PopViewController would pop the WRONG page (double-pop bug).
					var oldTopView = oldStack[oldStack.Count - 1];
					var prePopStack = new List<IView>(newStack) { oldTopView };
					SyncMiddleOfStack(prePopStack);

					if (newStack.Count == 1)
					{
						// Pop to root — store expected stack; OnNavigationComplete calls NavigationFinished.
						var rootVC = GetOrCreateViewController(newStack[0]);
						_pendingNavigationStack = new List<IView>(newStack);
						navManager.PopToRootViewController(rootVC, animated);
						return;
					}
					else
					{
						// Pop — store expected stack; OnNavigationComplete calls NavigationFinished.
						_pendingNavigationStack = new List<IView>(newStack);
						navManager.PopViewController(animated);
						return;
					}
				}
				else
				{
					// Top page same — just mid-stack removals
					SyncMiddleOfStack(newStack);
				}
			}
			else
			{
				// Stack size same, top page same — mid-stack changes only
				SyncMiddleOfStack(newStack);
			}

			NavigationStack = new List<IView>(newStack);
			NavigationView.NavigationFinished(NavigationStack);
		}

		void SyncMiddleOfStack(IReadOnlyList<IView> newStack)
		{
			if (_navManager is null)
			{
				return;
			}

			var currentVCs = _navManager.ActiveViewControllers();

			// Build the expected VC array from the new stack
			var expectedVCs = new UIViewController[newStack.Count];

			for (int index = 0; index < newStack.Count; index++)
			{
				expectedVCs[index] = GetOrCreateViewController(newStack[index]);
			}

			// If VCs differ, set them directly
			if (!currentVCs.SequenceEqual(expectedVCs))
			{
				_navManager.NavigationController.SetViewControllers(expectedVCs, false);
				_navManager.ClearPendingViewControllers();
			}
		}

		UIViewController GetOrCreateViewController(IView view)
		{
			if (_viewControllerMap.TryGetValue(view, out var existing))
			{
				return existing;
			}

			UIViewController vc;

			if (CreateViewControllerForPage is not null)
			{
				// Controls layer provides a ParentingViewController wrapper
				vc = CreateViewControllerForPage(view, MauiContext!);
			}
			else
			{
				// Core-only fallback
				var platformHandler = view.ToHandler(MauiContext!);
				vc = platformHandler.ViewController
					?? new ContainerViewController(view, platformHandler);
			}

			_viewControllerMap[view] = vc;
			return vc;
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest ea)
			{
				platformHandler.RequestNavigation(ea);
			}
		}

		/// <summary>
		/// Simple container VC for views that don't have their own VC.
		/// </summary>
		internal sealed class ContainerViewController : UIViewController
		{
			WeakReference<IView>? _viewRef;
			WeakReference<IPlatformViewHandler>? _handlerRef;

			public ContainerViewController(IView view, IPlatformViewHandler handler)
			{
				_viewRef = new WeakReference<IView>(view);
				_handlerRef = new WeakReference<IPlatformViewHandler>(handler);
			}

			public override void LoadView()
			{
				if (_handlerRef?.TryGetTarget(out var handler) == true)
				{
					View = handler.ToPlatform();
				}
				else
				{
					View = new UIView();
				}
			}

			public override void ViewDidLayoutSubviews()
			{
				base.ViewDidLayoutSubviews();

				if (View is not null && _viewRef?.TryGetTarget(out var view) == true)
				{
					view.Arrange(View.Bounds.ToRectangle());
				}
			}
		}

		/// <summary>
		/// INavigationManagerDelegate implementation for NavigationPage.
		/// </summary>
		sealed class NavigationViewDelegate : INavigationManagerDelegate
		{
			readonly WeakReference<NavigationViewHandler> _handlerRef;

			public NavigationViewDelegate(NavigationViewHandler handler)
			{
				_handlerRef = new WeakReference<NavigationViewHandler>(handler);
			}

			public (bool isHidden, bool animate) GetNavigationBarVisibility(UIViewController viewController)
			{
				if (!_handlerRef.TryGetTarget(out var handler))
				{
					return (false, false);
				}

				// Find the IView for this VC
				foreach (var kvp in handler._viewControllerMap)
				{
					if (kvp.Value == viewController)
					{
						bool hasNavBar = true;

						if (kvp.Key is IToolbarElement toolbarElement)
						{
							// NavigationPage uses Toolbar.IsVisible to control nav bar visibility
							hasNavBar = toolbarElement.Toolbar?.IsVisible ?? true;
						}

						return (!hasNavBar, true);
					}
				}

				return (false, true);
			}

			public bool ShouldPop()
			{
				if (!_handlerRef.TryGetTarget(out var handler))
				{
					return true;
				}

				// Dispatch to the next run loop iteration to avoid re-entrancy in shouldPopItem:
				// which would cause UIKit to skip DidShowViewController callbacks.
				var window = handler.MauiContext?.GetPlatformWindow()?.GetWindow();

				if (window is not null)
				{
					CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						window.BackButtonClicked();
					});

					return false;
				}

				// No window — allow UIKit pop (harmless fallback).
				return true;
			}

			public void OnNavigationComplete(UINavigationController navigationController, UIViewController viewController)
			{
				if (!_handlerRef.TryGetTarget(out var handler))
				{
					return;
				}

				var uikitVCs = navigationController.ViewControllers;
				var uikitCount = uikitVCs?.Length ?? 0;
				var mauiCount = handler.NavigationStack.Count;


				if (handler._pendingNavigationStack is not null)
				{
					// A MAUI-initiated push or pop has completed (DidShowViewController fired).
					// Consume the pending stack and call NavigationFinished synchronously here
					// on the main thread. This avoids the ContinueWith double-fire issue where
					// the pop's ContinueWith fires after the second push has begun, incorrectly
					// completing the push TCS with the wrong (pop) stack.
					var pendingStack = handler._pendingNavigationStack;
					handler._pendingNavigationStack = null;

					// If this is a pop, remove the popped pages from the VC map so that
					// re-pushing the same page instance creates a fresh VC via CreateForPage.
					// This ensures page.ToPlatform() is called again, which refreshes MAUI's
					// platform-loaded state so OnLoadedAsync can complete.
					if (pendingStack.Count < handler.NavigationStack.Count)
					{
						var newSet = new HashSet<IView>(pendingStack);
						foreach (var view in handler.NavigationStack)
						{
							if (!newSet.Contains(view))
								handler._viewControllerMap.Remove(view);
						}
					}

					handler.NavigationStack = pendingStack;
					handler.NavigationView.NavigationFinished(pendingStack);
				}
				else if (uikitCount < mauiCount)
				{
					// No pending MAUI navigation — UIKit stack shrank due to a native pop
					// (back button, swipe gesture, long-press history menu).
					SyncNativeStackToMaui(handler);
				}
			}

			public void OnWillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
			{
				// No early toolbar setup needed for NavigationPage
				// (NavigationRenderer uses ParentingViewController for this, but we handle it through mappers)
			}

			public void OnInteractivePopCompleted()
			{
				if (!_handlerRef.TryGetTarget(out var handler))
				{
					return;
				}

				SyncNativeStackToMaui(handler);
			}

			public void OnNavigationControllerDidAppear()
			{
				if (!_handlerRef.TryGetTarget(out var handler) || ((IElementHandler)handler).VirtualView is null)
				{
					return;
				}

				NavigationViewHandler.OnNavigationControllerDidAppear?.Invoke(handler.NavigationView);
			}

			public void OnNavigationControllerDidDisappear()
			{
				if (!_handlerRef.TryGetTarget(out var handler) || ((IElementHandler)handler).VirtualView is null)
				{
					return;
				}

				NavigationViewHandler.OnNavigationControllerDidDisappear?.Invoke(handler.NavigationView);
			}

			public void OnViewDidLayoutSubviews(CoreGraphics.CGRect bounds)
			{
				if (!_handlerRef.TryGetTarget(out var handler) || ((IElementHandler)handler).VirtualView is null)
				{
					return;
				}

				// Propagate UIKit layout to the MAUI NavigationPage element.
				// This mirrors NavigationRenderer.ViewDidLayoutSubviews → Element.Arrange().
				// Without this, NavigationPage.Frame remains {-1,-1} under the handler because
				// no one calls Arrange on it when UIKit lays out the nav controller's view.
				handler.VirtualView?.Arrange(bounds.ToRectangle());
			}

			/// <summary>
			/// Rebuilds the MAUI NavigationStack from the UIKit VC stack and notifies NavigationFinished.
			/// Used after native back button pops and interactive pop gestures.
			/// </summary>
			static void SyncNativeStackToMaui(NavigationViewHandler handler)
			{
				var navController = handler._navManager?.NavigationController;
				if (navController?.ViewControllers is UIViewController[] vcs)
				{
					// Identify pages that were popped (in old stack but not in new UIKit stack)
					var oldStack = handler.NavigationStack;

					var newStack = new List<IView>();
					foreach (var vc in vcs)
					{
						foreach (var kvp in handler._viewControllerMap)
						{
							if (kvp.Value == vc)
							{
								newStack.Add(kvp.Key);
								break;
							}
						}
					}

					// Find the page that was on top before the pop
					IView? poppedPage = null;
					if (oldStack.Count > newStack.Count && oldStack.Count > 0)
					{
						poppedPage = oldStack[oldStack.Count - 1];
					}

					handler.NavigationStack = newStack;
					handler.NavigationView.NavigationFinished(newStack);

					// After NavigationFinished has updated CurrentPage, fire lifecycle events
					// for the popped page. This is the iOS-specific equivalent of the renderer's
					// ParentingViewController.DidMoveToParentViewController → UpdateFormsInnerNavigation
					// → SendNavigatedFromHandler.
					if (poppedPage is not null && OnNativePopCompleted is not null)
					{
						OnNativePopCompleted(handler.NavigationView, poppedPage);
					}
				}
			}
		}
	}
}
