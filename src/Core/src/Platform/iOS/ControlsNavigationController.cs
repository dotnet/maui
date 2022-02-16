//#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class ControlsNavigationController : UINavigationController
	{
		readonly NavigationViewHandler _handler;
		Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks =
							new Dictionary<UIViewController, TaskCompletionSource<bool>>();
		TaskCompletionSource<bool>? _popCompletionTask;

		// This holds the view controllers for each page
		readonly Dictionary<IView, IPlatformViewHandler> _trackers =
			new Dictionary<IView, IPlatformViewHandler>();

		IReadOnlyList<IView> NavigationStack => _handler.NavigationStack;

		public ControlsNavigationController(NavigationViewHandler handler) : base()
		{
			Delegate = new NavDelegate(this);
			_handler = handler;
		}

		public ControlsNavigationController(NavigationViewHandler handler, Type navigationBarType, Type toolbarType)
			: base(navigationBarType, toolbarType)
		{
			Delegate = new NavDelegate(this);
			_handler = handler;
		}

		[Export("navigationBar:shouldPopItem:")]
		public bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item) =>
			SendPop();

		internal bool SendPop()
		{
			if (ViewControllers == null)
				return false;

			// this means the pop is already done, nothing we can do
			if (ViewControllers.Length < NavigationBar.Items.Length)
				return true;

			//foreach (var tracker in _trackers)
			//{
			//	if (tracker.Value.ViewController == TopViewController)
			//	{
			//		var behavior = Shell.GetBackButtonBehavior(tracker.Value.Page);
			//		var command = behavior.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
			//		var commandParameter = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandParameterProperty, null);

			//		if (command != null)
			//		{
			//			if (command.CanExecute(commandParameter))
			//			{
			//				command.Execute(commandParameter);
			//			}

			//			return false;
			//		}

			//		break;
			//	}
			//}

			//bool allowPop = ShouldPop();
			bool allowPop = true;

			if (allowPop)
			{
				// Do not remove, wonky behavior on some versions of iOS if you dont dispatch
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
				{
					_popCompletionTask = new TaskCompletionSource<bool>();
					SendPoppedOnCompletion(_popCompletionTask.Task);
					PopViewController(true);
				});
			}
			else
			{
				for (int i = 0; i < NavigationBar.Subviews.Length; i++)
				{
					var child = NavigationBar.Subviews[i];
					if (child.Alpha != 1)
						UIView.Animate(.2f, () => child.Alpha = 1);
				}
			}

			return false;
		}


		async void SendPoppedOnCompletion(Task popTask)
		{
			if (popTask == null)
			{
				throw new ArgumentNullException(nameof(popTask));
			}

			var poppedPage = NavigationStack[NavigationStack.Count - 1];

			_handler.SendPopping(popTask);

			await popTask;

			DisposePage(poppedPage);
		}

		void DisposePage(IView page, bool calledFromDispose = false)
		{
			if (ViewControllers == null)
				return;

			if (_trackers.TryGetValue(page, out var tracker))
			{
				if (!calledFromDispose && tracker.ViewController != null && ViewControllers.Contains(tracker.ViewController) &&
					_trackers[page].ViewController != null)
				{
					ViewControllers = ViewControllers.Remove(_trackers[page].ViewController!);
				}

				_trackers.Remove(page);
			}
		}

		//internal async Task OnPopRequestedAsync(NavigationRequest e)
		//{
		//	var page = e.Page;
		//	var animated = e.Animated;

		//	_popCompletionTask = new TaskCompletionSource<bool>();
		//	e.Task = _popCompletionTask.Task;

		//	PopViewController(animated);

		//	await _popCompletionTask.Task;

		//	DisposePage(page);
		//}

		//internal void LoadPages(IMauiContext mauiContext)
		//{
		//	foreach (var page in NavigationStack)
		//		PushPage(page, false, mauiContext);
		//}

		//internal void OnPushRequested(NavigationRequest e, IMauiContext mauiContext)
		//{
		//	var page = e.Page;
		//	var animated = e.Animated;

		//	var taskSource = new TaskCompletionSource<bool>();
		//	PushPage(page, animated, mauiContext, taskSource);

		//	e.Task = taskSource.Task;
		//}


		void PushPage(IView page, bool animated, IMauiContext mauiContext, TaskCompletionSource<bool>? completionSource = null)
		{
			var viewController = page.ToUIViewController(mauiContext);
			var handler = (IPlatformViewHandler)page.Handler!;

			_trackers[page] = handler;

			if (completionSource != null)
				_completionTasks[viewController] = completionSource;

			PushViewController(viewController, animated);
		}

		IView? ElementForViewController(UIViewController viewController)
		{
			foreach (var child in _trackers)
			{
				if (child.Value.ViewController == viewController)
					return child.Key;
			}

			return null;
		}


		class NavDelegate : UINavigationControllerDelegate
		{
			readonly ControlsNavigationController _self;

			public NavDelegate(ControlsNavigationController renderer)
			{
				_self = renderer;
			}

			// This is currently working around a Mono Interpreter bug
			// if you remove this code please verify that hot restart still works
			// https://github.com/xamarin/Microsoft.Maui.Controls.Compatibility/issues/10519
			[Export("navigationController:animationControllerForOperation:fromViewController:toViewController:")]
			[Foundation.Preserve(Conditional = true)]
			public new IUIViewControllerAnimatedTransitioning? GetAnimationControllerForOperation(UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController)
			{
				return null;
			}

			public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var tasks = _self._completionTasks;
				var popTask = _self._popCompletionTask;

				if (tasks.TryGetValue(viewController, out var source))
				{
					source.TrySetResult(true);
					tasks.Remove(viewController);
				}
				else if (popTask != null)
				{
					popTask.TrySetResult(true);
				}
			}

			public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var element = _self.ElementForViewController(viewController);

				bool navBarVisible = true;

				//if (element is BindableObject bo)
				//	navBarVisible = NavigationView.GetHasNavigationBar(bo);

				navigationController.SetNavigationBarHidden(!navBarVisible, true);

				var coordinator = viewController.GetTransitionCoordinator();
				if (coordinator != null && coordinator.IsInteractive)
				{
					// handle swipe to dismiss gesture 
					coordinator.NotifyWhenInteractionChanges(OnInteractionChanged);
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
