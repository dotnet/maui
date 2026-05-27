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

		public IStackNavigationView NavigationView => ((IStackNavigationView)VirtualView);

		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		// Maps IView → UIViewController for the navigation stack
		readonly Dictionary<IView, UIViewController> _viewControllerMap = new();

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
			if (_navManager is null)
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
						_navManager.NavigationController.SetViewControllers(new[] { vc }, false);
					}
					else
					{
						var tcs = _navManager.PushViewController(vc, animated && index == newStack.Count - 1);
						if (!animated || index < newStack.Count - 1)
						{
							_navManager.CompletePushImmediately(vc);
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

				// Push the new top page
				var topView = newStack[newStack.Count - 1];
				var vc = GetOrCreateViewController(topView);
				var tcs = _navManager.PushViewController(vc, animated);

				tcs.Task.ContinueWith(_ =>
				{
					NavigationStack = new List<IView>(newStack);
					NavigationView.NavigationFinished(NavigationStack);
				}, TaskScheduler.FromCurrentSynchronizationContext());

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
						// Pop to root
						var rootVC = GetOrCreateViewController(newStack[0]);
						var task = _navManager.PopToRootViewController(rootVC, animated);
						task.ContinueWith(_ =>
						{
							CleanupRemovedPages(oldStack, newStack);
							NavigationStack = new List<IView>(newStack);
							NavigationView.NavigationFinished(NavigationStack);
						}, TaskScheduler.FromCurrentSynchronizationContext());

						return;
					}
					else
					{
						var task = _navManager.PopViewController(animated);
						task.ContinueWith(_ =>
						{
							CleanupRemovedPages(oldStack, newStack);
							NavigationStack = new List<IView>(newStack);
							NavigationView.NavigationFinished(NavigationStack);
						}, TaskScheduler.FromCurrentSynchronizationContext());

						return;
					}
				}
				else
				{
					// Top page same — just mid-stack removals
					SyncMiddleOfStack(newStack);
					CleanupRemovedPages(oldStack, newStack);
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

		void CleanupRemovedPages(IReadOnlyList<IView> oldStack, IReadOnlyList<IView> newStack)
		{
			var newSet = new HashSet<IView>(newStack);

			foreach (var view in oldStack)
			{
				if (!newSet.Contains(view) && _viewControllerMap.TryGetValue(view, out var vc))
				{
					_viewControllerMap.Remove(view);
					// Don't dispose — the IView still owns the platform view lifecycle
				}
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

				// Route the back button press through MAUI's navigation pipeline.
				// IWindow.BackButtonClicked() calls Page.SendBackButtonPressed() which
				// invokes NavigationPage.OnBackButtonPressed() — that method either:
				//   1. Lets the current page intercept the press, or
				//   2. Calls PopAsync() to navigate back programmatically.
				// In both cases, UIKit should NOT perform its own pop (return false).
				var window = handler.MauiContext?.GetPlatformWindow()?.GetWindow();

				if (window?.BackButtonClicked() == true)
				{
					return false;
				}

				// Not handled (e.g. root page) — allow UIKit pop (harmless at root).
				return true;
			}

			public void OnNavigationComplete(UINavigationController navigationController, UIViewController viewController)
			{
				if (!_handlerRef.TryGetTarget(out var handler))
				{
					return;
				}

				// Check if the UIKit stack shrank compared to our tracked stack.
				// This happens when the native back button is tapped — UIKit pops the VC
				// directly without going through RequestNavigation.
				var uikitVCs = navigationController.ViewControllers;

				if (uikitVCs is not null && uikitVCs.Length < handler.NavigationStack.Count)
				{
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

			/// <summary>
			/// Rebuilds the MAUI NavigationStack from the UIKit VC stack and notifies NavigationFinished.
			/// Used after native back button pops and interactive pop gestures.
			/// </summary>
			static void SyncNativeStackToMaui(NavigationViewHandler handler)
			{
				var navController = handler._navManager?.NavigationController;
				if (navController?.ViewControllers is UIViewController[] vcs)
				{
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

					handler.NavigationStack = newStack;
					handler.NavigationView.NavigationFinished(newStack);
				}
			}
		}
	}
}
