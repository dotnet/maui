#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Manages a UINavigationController for NavigationPage's handler architecture on iOS.
    /// Owns a UINavigationController and provides push/pop/insert/remove operations,
    /// completion tracking, interactive pop gesture handling, and nav bar visibility.
    ///
    /// Currently used by <see cref="Microsoft.Maui.Handlers.NavigationViewHandler"/>.
    /// </summary>
    /// <remarks>
    /// Consumer-specific behavior is injected via <see cref="INavigationManagerDelegate"/>,
    /// making this class reusable. In the future, Shell's handler could also adopt this
    /// manager to share the same UINavigationController infrastructure.
    /// </remarks>
    internal class NavigationControllerManager : IDisposable
    {
        readonly INavigationManagerDelegate _delegate;
        readonly UINavigationController _navigationController;
        readonly Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks = new();

        TaskCompletionSource<bool>? _popCompletionTask;
        UIViewController[]? _pendingViewControllers;
        bool _firstLayoutCompleted;
        bool _disposed;

        /// <summary>
        /// The managed UINavigationController.
        /// </summary>
        public UINavigationController NavigationController => _navigationController;

        /// <summary>
        /// Creates a new NavigationControllerManager with the specified navbar class.
        /// </summary>
        /// <param name="navBarType">The UINavigationBar subclass to use (e.g., MauiNavigationBar).</param>
        /// <param name="managerDelegate">Consumer-specific behavior delegate.</param>
        public NavigationControllerManager(Type navBarType, INavigationManagerDelegate managerDelegate)
        {
            _delegate = managerDelegate ?? throw new ArgumentNullException(nameof(managerDelegate));
            _navigationController = new MauiNavigationController(navBarType, this);
            _navigationController.Delegate = new NavDelegate(this);
        }

        /// <summary>
        /// Sets up the interactive pop gesture recognizer with the managed back-button policy.
        /// Call after the navigation controller's view is loaded.
        /// </summary>
        public void SetupInteractivePopGesture()
        {
            if (_navigationController.InteractivePopGestureRecognizer is not null)
            {
                _navigationController.InteractivePopGestureRecognizer.Delegate =
                    new GestureDelegate(_navigationController, this);
            }
        }

        #region Push / Pop

        /// <summary>
        /// Pushes a view controller onto the navigation stack.
        /// Returns a TaskCompletionSource that completes when the push animation finishes.
        /// </summary>
        public TaskCompletionSource<bool> PushViewController(UIViewController viewController, bool animated)
        {
            var completionSource = new TaskCompletionSource<bool>();
            _pendingViewControllers = null;
            _completionTasks[viewController] = completionSource;
            _navigationController.PushViewController(viewController, animated);
            return completionSource;
        }

        /// <summary>
        /// Pops the top view controller from the navigation stack.
        /// Returns a Task that completes when the pop animation finishes.
        /// </summary>
        public Task<bool> PopViewController(bool animated)
        {
            _pendingViewControllers = null;
            _popCompletionTask = new TaskCompletionSource<bool>();
            _navigationController.PopViewController(animated);
            return _popCompletionTask.Task;
        }

        /// <summary>
        /// Pops all view controllers except the root.
        /// Returns a Task that completes when the animation finishes.
        /// </summary>
        public Task<bool> PopToRootViewController(UIViewController rootViewController, bool animated)
        {
            _pendingViewControllers = null;
            var completionSource = new TaskCompletionSource<bool>();
            _completionTasks[rootViewController] = completionSource;
            _navigationController.PopToRootViewController(animated);
            return completionSource.Task;
        }

        /// <summary>
        /// Inserts a view controller at the specified index in the navigation stack.
        /// </summary>
        public void InsertViewController(int index, UIViewController viewController)
        {
            _pendingViewControllers ??= _navigationController.ViewControllers;
            if (_pendingViewControllers is not null)
            {
                _pendingViewControllers = _pendingViewControllers.Insert(index, viewController);
                _navigationController.ViewControllers = _pendingViewControllers;
            }
        }

        /// <summary>
        /// Removes a view controller from the navigation stack.
        /// </summary>
        public void RemoveViewController(UIViewController viewController)
        {
            _pendingViewControllers ??= _navigationController.ViewControllers;
            if (_pendingViewControllers is not null && _pendingViewControllers.Contains(viewController))
            {
                _pendingViewControllers = _pendingViewControllers.Remove(viewController);
            }
            if (_pendingViewControllers is not null)
            {
                _navigationController.ViewControllers = _pendingViewControllers;
            }
        }

        /// <summary>
        /// Returns the current VC stack, accounting for pending changes.
        /// </summary>
        public UIViewController[] ActiveViewControllers() =>
            _pendingViewControllers ?? _navigationController.ViewControllers ?? Array.Empty<UIViewController>();

        /// <summary>
        /// Clears the pending view controller state (e.g., on navigation events).
        /// </summary>
        public void ClearPendingViewControllers() => _pendingViewControllers = null;

        /// <summary>
        /// Completes a push completion task for a view controller that was
        /// not in the visible tab (no DidShowViewController callback).
        /// </summary>
        public void CompletePushImmediately(UIViewController viewController)
        {
            if (_completionTasks.TryGetValue(viewController, out var source))
            {
                source.TrySetResult(true);
                _completionTasks.Remove(viewController);
            }
        }

        /// <summary>
        /// Completes a pending pop task immediately.
        /// </summary>
        public void CompletePopImmediately()
        {
            _popCompletionTask?.TrySetResult(true);
            _popCompletionTask = null;
        }

        #endregion

        #region ShouldPopItem

        /// <summary>
        /// Called by <see cref="MauiNavigationController"/> when the navigation bar back button is tapped.
        /// </summary>
        internal bool HandleShouldPopItem()
        {
            return _delegate.ShouldPop();
        }

        #endregion

        #region MauiNavigationController

        /// <summary>
        /// UINavigationController subclass that intercepts the native back button via
        /// the <c>navigationBar:shouldPopItem:</c> Objective-C selector.
        /// UIKit calls this selector on the navigation controller (which is the
        /// default delegate of its own navigation bar). A plain UINavigationController
        /// always returns true; this subclass delegates to <see cref="NavigationControllerManager"/>.
        /// </summary>
        sealed class MauiNavigationController : UINavigationController
        {
            readonly WeakReference<NavigationControllerManager> _managerRef;

            public MauiNavigationController(Type navigationBarType, NavigationControllerManager manager)
                : base(navigationBarType, null!)
            {
                _managerRef = new WeakReference<NavigationControllerManager>(manager);
            }

            [Export("navigationBar:shouldPopItem:")]
            bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
            {
                if (_managerRef.TryGetTarget(out var manager))
                {
                    return manager.HandleShouldPopItem();
                }

                return true;
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Cancel all pending completion tasks
            foreach (var kvp in _completionTasks)
            {
                kvp.Value.TrySetCanceled();
            }

            _completionTasks.Clear();

            _popCompletionTask?.TrySetCanceled();
            _popCompletionTask = null;

            _pendingViewControllers = null;
        }

        #endregion

        #region NavDelegate

        /// <summary>
        /// UINavigationController delegate that resolves push/pop completion tasks
        /// and manages nav bar visibility.
        /// </summary>
        sealed class NavDelegate : UINavigationControllerDelegate
        {
            readonly WeakReference<NavigationControllerManager> _managerRef;

            public NavDelegate(NavigationControllerManager manager)
            {
                _managerRef = new WeakReference<NavigationControllerManager>(manager);
            }

            public override void DidShowViewController(
                UINavigationController navigationController,
                [Transient] UIViewController viewController,
                bool animated)
            {
                if (!_managerRef.TryGetTarget(out var manager))
                {
                    return;
                }

                // Resolve completion tasks
                if (manager._completionTasks.TryGetValue(viewController, out var source))
                {
                    source.TrySetResult(true);
                    manager._completionTasks.Remove(viewController);
                }
                else
                {
                    // No push task — this is a pop completion
                    manager._popCompletionTask?.TrySetResult(true);
                    manager._popCompletionTask = null;
                }

                // First layout guard
                if (!manager._firstLayoutCompleted)
                {
                    manager._firstLayoutCompleted = true;
                }

                // Notify consumer
                manager._delegate.OnNavigationComplete(navigationController, viewController);
            }

            public override void WillShowViewController(
                UINavigationController navigationController,
                [Transient] UIViewController viewController,
                bool animated)
            {
                if (!_managerRef.TryGetTarget(out var manager))
                {
                    return;
                }

                // Set nav bar visibility
                var (isHidden, shouldAnimate) = manager._delegate.GetNavigationBarVisibility(viewController);
                navigationController.SetNavigationBarHidden(isHidden, shouldAnimate && animated);

                // Handle interactive pop gesture
                var coordinator = viewController.GetTransitionCoordinator();
                if (coordinator is not null && coordinator.IsInteractive)
                {
                    coordinator.NotifyWhenInteractionChanges(manager.OnInteractionChanged);
                }

                // Notify consumer for early toolbar setup
                manager._delegate.OnWillShowViewController(navigationController, viewController, animated);
            }
        }

        #endregion

        #region Interactive Pop

        void OnInteractionChanged(IUIViewControllerTransitionCoordinatorContext context)
        {
            if (!context.IsCancelled)
            {
                _popCompletionTask = new TaskCompletionSource<bool>();
                _delegate.OnInteractivePopCompleted();
            }
        }

        #endregion

        #region GestureDelegate

        /// <summary>
        /// Gesture recognizer delegate for the interactive pop gesture.
        /// Controls whether the swipe-back gesture should begin.
        /// </summary>
        sealed class GestureDelegate : UIGestureRecognizerDelegate
        {
            readonly WeakReference<UINavigationController> _navigationControllerRef;
            readonly WeakReference<NavigationControllerManager> _managerRef;

            public GestureDelegate(UINavigationController navController, NavigationControllerManager manager)
            {
                _navigationControllerRef = new WeakReference<UINavigationController>(navController);
                _managerRef = new WeakReference<NavigationControllerManager>(manager);
            }

            public override bool ShouldBegin(UIGestureRecognizer recognizer)
            {
                if (!_navigationControllerRef.TryGetTarget(out var navController))
                {
                    return false;
                }

                // Only allow interactive pop if there's more than the root VC
                if ((navController.ViewControllers?.Length ?? 0) <= 1)
                {
                    return false;
                }

                if (!_managerRef.TryGetTarget(out var manager))
                {
                    return false;
                }

                return manager._delegate.ShouldPop();
            }
        }

        #endregion
    }
}
