#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellSectionRenderer : UINavigationController, IShellSectionRenderer, IAppearanceObserver, IDisconnectable
	{
		#region IShellContentRenderer

		public bool IsInMoreTab { get; set; }

		public ShellSection ShellSection
		{
			get { return _shellSection; }
			set
			{
				if (_shellSection == value)
					return;
				_shellSection = value;
				LoadPages();
				OnShellSectionSet();
				_shellSection.PropertyChanged += HandlePropertyChanged;
				((IShellSectionController)_shellSection).NavigationRequested += OnNavigationRequested;
			}
		}

		IShellSectionController ShellSectionController => ShellSection;

		public UIViewController ViewController => this;

		#endregion IShellContentRenderer

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				_appearanceTracker.ResetAppearance(this);
			else
				_appearanceTracker.SetAppearance(this, appearance);
		}

		#endregion IAppearanceObserver

		IShellContext _context;

		readonly Dictionary<Element, IShellPageRendererTracker> _trackers =
			new Dictionary<Element, IShellPageRendererTracker>();

		IShellNavBarAppearanceTracker _appearanceTracker;

		Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks =
							new Dictionary<UIViewController, TaskCompletionSource<bool>>();

		Page _displayedPage;
		bool _disposed;
		bool _firstLayoutCompleted;
		TaskCompletionSource<bool> _popCompletionTask;
		IShellSectionRootRenderer _renderer;
		ShellSection _shellSection;
		bool _ignorePopCall;

		// When setting base.ViewControllers iOS doesn't modify the property right away. 
		// if you set base.ViewControllers to a new array and then retrieve base.ViewControllers
		// iOS will return the previous array until the new array has been processed
		// This means if you try to remove one VC and then try to remove a second VC before the first one is processed
		// you'll end up re-adding back the first VC
		// ViewControllers = ViewControllers.Remove(vc1)
		// ViewControllers = ViewControllers.Remove(vc2)  
		// You've now added vc1 back because the second call to ViewControllers will still return a ViewControllers list with vc1 in it
		UIViewController[] _pendingViewControllers;

		public ShellSectionRenderer(IShellContext context) : base(typeof(MauiNavigationBar), null)
		{
			Delegate = new NavDelegate(this);
			_context = context;
			_context.Shell.PropertyChanged += HandleShellPropertyChanged;
			_context.Shell.Navigated += OnNavigated;
			_context.Shell.Navigating += OnNavigating;
		}

		public ShellSectionRenderer(IShellContext context, Type navigationBarType, Type toolbarType)
			: base(navigationBarType, toolbarType)
		{
			Delegate = new NavDelegate(this);
			_context = context;
			_context.Shell.PropertyChanged += HandleShellPropertyChanged;
			_context.Shell.Navigated += OnNavigated;
			_context.Shell.Navigating += OnNavigating;
		}

		[Export("navigationBar:shouldPopItem:")]
		[Internals.Preserve(Conditional = true)]
		public bool ShouldPopItem(UINavigationBar _, UINavigationItem __)
			=> SendPop();

		[Export("navigationBar:didPopItem:")]
		[Internals.Preserve(Conditional = true)]
		bool DidPopItem(UINavigationBar _, UINavigationItem __)
		{
			// Check for null references
			if (_shellSection?.Stack is null || NavigationBar?.Items is null)
				return true;

			// Check if stacks are in sync
			if (_shellSection.Stack.Count == NavigationBar.Items.Length)
				return true;

			var pages = _shellSection.Stack.ToList();

			// Ensure we have enough pages and navigation items
			if (pages.Count == 0 || NavigationBar.Items.Length == 0)
				return true;

			// Bounds check: ensure we have a valid index for pages array
			int targetIndex = NavigationBar.Items.Length - 1;
			if (targetIndex < 0 || targetIndex >= pages.Count)
				return true;

			_shellSection.SyncStackDownTo(pages[targetIndex]);

			for (int i = pages.Count - 1; i >= NavigationBar.Items.Length; i--)
			{
				var page = pages[i];
				if (page != null)
					DisposePage(page);
			}

			return true;
		}

		internal bool SendPop()
		{
			// this means the pop is already done, nothing we can do
			if (ActiveViewControllers().Length < NavigationBar.Items.Length)
				return true;

			foreach (var tracker in _trackers)
			{
				if (tracker.Value.ViewController == TopViewController)
				{
					var behavior = Shell.GetBackButtonBehavior(tracker.Value.Page);
					var command = behavior.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
					var commandParameter = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandParameterProperty, null);

					if (command != null)
					{
						if (command.CanExecute(commandParameter))
						{
							command.Execute(commandParameter);
						}

						return false;
					}

					break;
				}
			}


			// Do not remove, wonky behavior on some versions of iOS if you dont dispatch
			// Shane: ^ not sure if this is true anymore because of how
			// we now route this through "GoToAsync"
			CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(async () =>
			{
				var navItemsCount = NavigationBar.Items.Length;

				await _context.Shell.GoToAsync("..", true);

				// This means the navigation was cancelled
				if (NavigationBar.Items.Length == navItemsCount)
				{
					for (int i = 0; i < NavigationBar.Subviews.Length; i++)
					{
						var child = NavigationBar.Subviews[i];
						if (child.Alpha != 1)
							UIView.Animate(.2f, () => child.Alpha = 1);
					}
				}
			});

			return false;
		}

		public override void ViewDidDisappear(bool animated)
		{
			// If this page is removed from the View Hierarchy we need to resolve any
			// pending navigation operations
			var sourcesToComplete = new List<TaskCompletionSource<bool>>();

			foreach (var item in _completionTasks.Values)
			{
				sourcesToComplete.Add(item);
			}

			_completionTasks.Clear();

			foreach (var source in sourcesToComplete)
				source.TrySetResult(false);

			_popCompletionTask?.TrySetResult(false);
			_popCompletionTask = null;


			base.ViewDidDisappear(animated);
		}

		public override void ViewWillAppear(bool animated)
		{
			if (_disposed)
				return;

			UpdateFlowDirection();
			base.ViewWillAppear(animated);
		}

		internal void UpdateFlowDirection()
		{
			View.UpdateFlowDirection(_context.Shell);
			NavigationBar.UpdateFlowDirection(_context.Shell);
		}

		public override void ViewDidLayoutSubviews()
		{
			if (_disposed)
				return;

			base.ViewDidLayoutSubviews();

			_appearanceTracker.UpdateLayout(this);

			if (!_firstLayoutCompleted)
			{
				UpdateShadowImages();
				_firstLayoutCompleted = true;
			}
		}

		public override void ViewDidLoad()
		{
			if (_disposed)
				return;

			base.ViewDidLoad();
			InteractivePopGestureRecognizer.Delegate = new GestureDelegate(this, ShouldPop);
			UpdateFlowDirection();
		}


		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			if (_context is ShellRenderer shellRenderer)
			{
				shellRenderer.ViewController.SetNeedsUpdateOfHomeIndicatorAutoHidden();
				shellRenderer.ViewController.SetNeedsStatusBarAppearanceUpdate();
			}
		}

		void IDisconnectable.Disconnect()
		{
			(_renderer as IDisconnectable)?.Disconnect();

			if (_displayedPage != null)
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

			if (_shellSection != null)
			{
				_shellSection.PropertyChanged -= HandlePropertyChanged;
				((IShellSectionController)ShellSection).NavigationRequested -= OnNavigationRequested;
				((IShellSectionController)ShellSection).RemoveDisplayedPageObserver(this);
			}


			if (_context.Shell != null)
			{
				_context.Shell.PropertyChanged -= HandleShellPropertyChanged;
				_context.Shell.Navigated -= OnNavigated;
				_context.Shell.Navigating -= OnNavigating;
				((IShellController)_context.Shell).RemoveAppearanceObserver(this);
			}

		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				this.RemoveFromParentViewController();
				_disposed = true;
				_renderer.Dispose();
				_appearanceTracker.Dispose();
				(this as IDisconnectable).Disconnect();

				foreach (var tracker in ShellSection.Stack)
				{
					if (tracker == null)
						continue;

					DisposePage(tracker, true);
				}
			}

			_disposed = true;
			_displayedPage = null;
			_shellSection = null;
			_appearanceTracker = null;
			_renderer = null;
			_context = null;

			base.Dispose(disposing);
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.TitleProperty.PropertyName)
				UpdateTabBarItem();
			else if (e.PropertyName == BaseShellItem.IconProperty.PropertyName)
				UpdateTabBarItem();
		}

		protected virtual IShellSectionRootRenderer CreateShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			return new ShellSectionRootRenderer(shellSection, shellContext);
		}

		protected virtual void LoadPages()
		{
			_renderer = CreateShellSectionRootRenderer(ShellSection, _context);

			PushViewController(_renderer.ViewController, false);

			var stack = ShellSection.Stack;
			for (int i = 1; i < stack.Count; i++)
			{
				PushPage(stack[i], false);
			}
		}

		protected virtual void OnDisplayedPageChanged(Page page)
		{
			if (_displayedPage == page)
				return;

			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;
			}

			_displayedPage = page;

			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;
				UpdateNavigationBarHidden();
				UpdateNavigationBarHasShadow();
			}
		}

		protected virtual void OnInsertRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var before = e.BeforePage;

			var beforeRenderer = (IPlatformViewHandler)before.Handler;

			var renderer = (IPlatformViewHandler)page.ToHandler(_shellSection.FindMauiContext());

			var tracker = _context.CreatePageRendererTracker();
			tracker.ViewController = renderer.ViewController;
			tracker.Page = page;

			_trackers[page] = tracker;

			InsertViewController(ActiveViewControllers().IndexOf(beforeRenderer.ViewController), renderer.ViewController);
		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			switch (e.RequestType)
			{
				case NavigationRequestType.Push:
					OnPushRequested(e);
					break;

				case NavigationRequestType.Pop:
					OnPopRequested(e);
					break;

				case NavigationRequestType.PopToRoot:
					OnPopToRootRequested(e);
					break;

				case NavigationRequestType.Insert:
					OnInsertRequested(e);
					break;

				case NavigationRequestType.Remove:
					OnRemoveRequested(e);
					break;
			}
		}

		protected virtual async void OnPopRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			_popCompletionTask = new TaskCompletionSource<bool>();
			e.Task = _popCompletionTask.Task;

			PopViewController(animated);

			await _popCompletionTask.Task;

			DisposePage(page);
		}

		public override UIViewController[] PopToRootViewController(bool animated)
		{
			if (!_ignorePopCall && ActiveViewControllers().Length > 1)
			{
				ProcessPopToRoot();
			}

			return base.PopToRootViewController(animated);
		}

		async void ProcessPopToRoot()
		{
			var task = new TaskCompletionSource<bool>();
			var pages = _shellSection.Stack.ToList();
			_completionTasks[_renderer.ViewController] = task;
			((IShellSectionController)ShellSection).SendPoppingToRoot(task.Task);
			await task.Task;

			for (int i = pages.Count - 1; i >= 1; i--)
			{
				var page = pages[i];
				DisposePage(page);
			}
		}

		protected virtual async void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			var animated = e.Animated;
			var task = new TaskCompletionSource<bool>();
			var pages = _shellSection.Stack.ToList();

			try
			{
				_ignorePopCall = true;
				_completionTasks[_renderer.ViewController] = task;
				e.Task = task.Task;
				PopToRootViewController(animated);
			}
			finally
			{
				_ignorePopCall = false;
			}

			await e.Task;

			for (int i = pages.Count - 1; i >= 1; i--)
			{
				var page = pages[i];
				DisposePage(page);
			}
		}

		protected virtual void OnPushRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			var taskSource = new TaskCompletionSource<bool>();
			PushPage(page, animated, taskSource);

			e.Task = taskSource.Task;
		}

		protected virtual void OnRemoveRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;

			var renderer = (IPlatformViewHandler)page.Handler;
			var viewController = renderer?.ViewController;

			if (viewController == null && _trackers.ContainsKey(page))
				viewController = _trackers[page].ViewController;

			if (viewController != null)
			{
				if (viewController == TopViewController)
				{
					e.Animated = false;
					OnPopRequested(e);
				}

				RemoveViewController(viewController);
				DisposePage(page);
			}
		}

		protected virtual void OnShellSectionSet()
		{
			_appearanceTracker = _context.CreateNavBarAppearanceTracker();
			UpdateTabBarItem();
			((IShellController)_context.Shell).AddAppearanceObserver(this, ShellSection);
			((IShellSectionController)ShellSection).AddDisplayedPageObserver(this, OnDisplayedPageChanged);
		}

		protected virtual void UpdateTabBarItem()
		{
			Title = ShellSection.Title;

			ShellSection.Icon.LoadImage(ShellSection.FindMauiContext(), icon =>
			{
				TabBarItem = new UITabBarItem(ShellSection.Title, icon?.Value, null);
				TabBarItem.AccessibilityIdentifier = ShellSection.AutomationId ?? ShellSection.Title;
			});
		}

		void DisposePage(Page page, bool calledFromDispose = false)
		{
			if (_trackers.TryGetValue(page, out var tracker))
			{
				if (!calledFromDispose && tracker.ViewController != null && ActiveViewControllers().Contains(tracker.ViewController))
				{
					System.Diagnostics.Debug.Write($"Disposing {_trackers[page].ViewController.GetHashCode()}");
					RemoveViewController(_trackers[page].ViewController);
				}

				tracker.Dispose();
				_trackers.Remove(page);
			}

			page?.DisconnectHandlers();
		}

		Element ElementForViewController(UIViewController viewController)
		{
			if (_renderer.ViewController == viewController)
				return ShellSection;

			foreach (var child in ShellSection.Stack)
			{
				if (child == null)
					continue;
				var renderer = (IPlatformViewHandler)child.Handler;
				if (viewController == renderer.ViewController)
					return child;
			}

			return null;
		}

		void OnDisplayedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				UpdateNavigationBarHidden();
			else if (e.PropertyName == Shell.NavBarHasShadowProperty.PropertyName)
				UpdateNavigationBarHasShadow();
		}

		// We only care about using pendingViewControllers when we are setting the ViewControllers array directly
		// So, once navigation starts again (or ends) we can just clear the pendingViewControllers
		void OnNavigating(object sender, ShellNavigatingEventArgs e)
		{
			_pendingViewControllers = null;
		}

		void OnNavigated(object sender, ShellNavigatedEventArgs e)
		{
			_pendingViewControllers = null;
		}

		// These are all just safety nets to ensure that _pendingViewControllers doesn't for some reason get out of sync
		// and start causing issues. In theory we could just override ViewControllers here to make sure _pendingViewControllers
		// stays in sync but I don't trust that `ViewControllers.set` is reliably called with every modification
		public override UIViewController[] ViewControllers
		{
			get => base.ViewControllers;
			set
			{
				if (_pendingViewControllers != null)
					_pendingViewControllers = value;

				base.ViewControllers = value;
			}
		}

		public override UIViewController[] PopToViewController(UIViewController viewController, bool animated)
		{
			_pendingViewControllers = null;
			return base.PopToViewController(viewController, animated);
		}

		public override void PushViewController(UIViewController viewController, bool animated)
		{
			_pendingViewControllers = null;
			if (IsInMoreTab && ParentViewController is UITabBarController tabBarController)
			{
				tabBarController.MoreNavigationController.PushViewController(viewController, animated);
			}
			else
			{
				base.PushViewController(viewController, animated);
			}
		}

		public override UIViewController PopViewController(bool animated)
		{
			_pendingViewControllers = null;
			if (IsInMoreTab && ParentViewController is UITabBarController tabBarController)
			{
				return tabBarController.MoreNavigationController.PopViewController(animated);
			}

			return base.PopViewController(animated);
		}

		UIViewController[] ActiveViewControllers() =>
			_pendingViewControllers ?? base.ViewControllers;

		void RemoveViewController(UIViewController viewController)
		{
			_pendingViewControllers = _pendingViewControllers ?? base.ViewControllers;
			if (_pendingViewControllers.Contains(viewController))
				_pendingViewControllers = _pendingViewControllers.Remove(viewController);

			ViewControllers = _pendingViewControllers;
		}

		void InsertViewController(int index, UIViewController viewController)
		{
			_pendingViewControllers = _pendingViewControllers ?? base.ViewControllers;
			_pendingViewControllers = _pendingViewControllers.Insert(index, viewController);
			ViewControllers = _pendingViewControllers;
		}

		void PushPage(Page page, bool animated, TaskCompletionSource<bool> completionSource = null)
		{
			var renderer = (IPlatformViewHandler)page.ToHandler(_shellSection.FindMauiContext());

			var tracker = _context.CreatePageRendererTracker();
			var pageViewController = renderer.ViewController!;
			tracker.ViewController = pageViewController;
			tracker.Page = page;

			_trackers[page] = tracker;

			var parentTabBar = ParentViewController as UITabBarController;
			var showsPresentation = parentTabBar == null || ReferenceEquals(parentTabBar.SelectedViewController, this);
			if (completionSource != null && showsPresentation)
				_completionTasks[pageViewController] = completionSource;

			PushViewController(pageViewController, animated);

			if (completionSource != null && !showsPresentation)
				completionSource.TrySetResult(true);
		}

		async void SendPoppedOnCompletion(Task popTask)
		{
			if (popTask == null)
			{
				throw new ArgumentNullException(nameof(popTask));
			}

			var poppedPage = _shellSection.Stack[_shellSection.Stack.Count - 1];

			// this is used to setup appearance changes based on the incoming page
			((IShellSectionController)_shellSection).SendPopping(popTask);

			await popTask;

			DisposePage(poppedPage);
		}

		bool ShouldPop()
		{
			var shellItem = _context.Shell.CurrentItem;
			var shellSection = shellItem?.CurrentItem;
			var shellContent = shellSection?.CurrentItem;
			var stack = shellSection?.Stack.ToList();

			stack?.RemoveAt(stack.Count - 1);

			return ((IShellController)_context.Shell).ProposeNavigation(ShellNavigationSource.Pop, shellItem, shellSection, shellContent, stack, true);
		}

		void UpdateNavigationBarHidden()
		{
			SetNavigationBarHidden(!Shell.GetNavBarIsVisible(_displayedPage), true);
		}

		void UpdateNavigationBarHasShadow()
		{
			_appearanceTracker.SetHasShadow(this, Shell.GetNavBarHasShadow(_displayedPage));
		}

		void UpdateShadowImages()
		{
			NavigationBar.SetValueForKey(NSObject.FromObject(true), new NSString("hidesShadow"));
		}

		class GestureDelegate : UIGestureRecognizerDelegate
		{
			readonly UINavigationController _parent;
			readonly Func<bool> _shouldPop;

			public GestureDelegate(UINavigationController parent, Func<bool> shouldPop)
			{
				_parent = parent;
				_shouldPop = shouldPop;
			}

			public override bool ShouldBegin(UIGestureRecognizer recognizer)
			{
				if ((_parent as ShellSectionRenderer).ActiveViewControllers().Length == 1)
					return false;
				return _shouldPop();
			}
		}

		class NavDelegate : UINavigationControllerDelegate
		{
			readonly ShellSectionRenderer _self;

			public NavDelegate(ShellSectionRenderer renderer)
			{
				_self = renderer;
			}

			// This is currently working around a Mono Interpreter bug
			// if you remove this code please verify that hot restart still works
			// https://github.com/xamarin/Xamarin.Forms/issues/10519
			[Export("navigationController:animationControllerForOperation:fromViewController:toViewController:")]
			[Foundation.Preserve(Conditional = true)]
			public new IUIViewControllerAnimatedTransitioning GetAnimationControllerForOperation(UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController)
			{
				return null;
			}

			public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				(navigationController.NavigationBar as MauiNavigationBar)?.RefreshIfNeeded();

				var tasks = _self._completionTasks;
				var popTask = _self._popCompletionTask;

				if (tasks.TryGetValue(viewController, out var source))
				{
					source.TrySetResult(true);
					tasks.Remove(viewController);
				}
				else
				{
					popTask?.TrySetResult(true);
				}
			}

			public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var element = _self.ElementForViewController(viewController);

				bool navBarVisible = false;

				if (element is not null)
				{
					if (element is ShellSection)
						navBarVisible = _self._renderer.ShowNavBar;
					else
						navBarVisible = Shell.GetNavBarIsVisible(element);
				}

				navigationController.SetNavigationBarHidden(!navBarVisible, true);

				var coordinator = viewController.GetTransitionCoordinator();
				if (coordinator != null && coordinator.IsInteractive)
				{
					// handle swipe to dismiss gesture 
					coordinator.NotifyWhenInteractionChanges(OnInteractionChanged);
				}

				// Because the back button title needs to be set on the previous VC
				// We want to set the BackButtonItem as early as possible so there is no flickering
				var currentPage = _self._context?.Shell?.GetCurrentShellPage();
				var trackers = _self._trackers;
				if (currentPage?.Handler is IPlatformViewHandler pvh &&
					pvh.ViewController == viewController &&
					trackers.TryGetValue(currentPage, out var tracker) &&
					tracker is ShellPageRendererTracker shellRendererTracker)
				{
					shellRendererTracker.UpdateToolbarItemsInternal(false);
				}
			}

			void OnInteractionChanged(IUIViewControllerTransitionCoordinatorContext context)
			{
				if (!context.IsCancelled)
				{
					_self._popCompletionTask = new TaskCompletionSource<bool>();
					_self.SendPoppedOnCompletion(_self._popCompletionTask.Task);
				}
			}
		}
	}
}
