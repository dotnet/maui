using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		// We need to wait for the window to be activated the first time before
		// we push any modal views.
		// After it's been activated we can push modals if the app has been sent to
		// the background so we don't care if it becomes inactive
		bool _platformActivated;
		bool _waitForModalToFinish;

		partial void InitializePlatform()
		{
			_window.Activated += OnWindowActivated;
			_window.Resumed += (_, _) => SyncPlatformModalStack();
			_window.HandlerChanging += OnPlatformWindowHandlerChanging;
			_window.Destroying += (_, _) => _platformActivated = false;
			_window.PropertyChanging += OnWindowPropertyChanging;
			_platformActivated = _window.IsActivated;
		}

		void OnWindowPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (e.PropertyName != Window.PageProperty.PropertyName)
			{
				return;
			}

			if (_currentPage is not null &&
				_currentPage.Handler is IPlatformViewHandler pvh &&
				pvh.ViewController?.PresentedViewController is ModalWrapper &&
				_window.Page != _currentPage)
			{
				ClearModalPages(xplat: true, platform: true);

				// Dismissing the root modal will dismiss everything
				pvh.ViewController.DismissViewController(false, null);
			}
		}

		Task SyncModalStackWhenPlatformIsReadyAsync() =>
			SyncPlatformModalStackAsync();

		bool IsModalPlatformReady => _platformActivated && !_waitForModalToFinish;

		void OnPlatformWindowHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			_platformActivated = _window.IsActivated;
		}

		void OnWindowActivated(object? sender, EventArgs e)
		{
			if (!_platformActivated)
			{
				_platformActivated = true;
				SyncModalStackWhenPlatformIsReady();
			}
		}

		UIViewController? WindowViewController
		{
			get
			{
				if (_window?.Page?.Handler is IPlatformViewHandler pvh &&
					pvh.ViewController?.ViewIfLoaded?.Window is not null)
				{
					return pvh.ViewController;
				}

				return WindowMauiContext.
						GetPlatformWindow()?
						.RootViewController;
			}
		}

		async Task<Page> PopModalPlatformAsync(bool animated)
		{
			var modal = CurrentPlatformModalPage;
			_platformModalPages.Remove(modal);

			var controller = (modal.Handler as IPlatformViewHandler)?.ViewController;

			if (controller?.ParentViewController is ModalWrapper modalWrapper &&
				modalWrapper.PresentingViewController is not null)
			{
				await modalWrapper.PresentingViewController.DismissViewControllerAsync(animated);
				return modal;
			}

			// if the presenting VC is null that means the modal window was already dismissed
			// this will usually happen as a result of swapping out the content on the window
			// which is what was acting as the PresentingViewController
			return modal;
		}

		Task PushModalPlatformAsync(Page modal, bool animated)
		{
			EndEditing();

			_platformModalPages.Add(modal);

			if (_window?.Page?.Handler is not null)
			{
				return PresentModal(modal, animated && _window.IsActivated);
			}

			return Task.CompletedTask;
		}

		async Task PresentModal(Page modal, bool animated)
		{
			bool failed = false;
			TaskCompletionSource presentFinished = new TaskCompletionSource();
			try
			{
				_waitForModalToFinishTask = presentFinished.Task;
				_waitForModalToFinish = true;

				var wrapper = new ControlsModalWrapper(modal.ToHandler(WindowMauiContext));

				if (_platformModalPages.Count > 1)
				{
					var topPage = _platformModalPages[_platformModalPages.Count - 2];
					var controller = (topPage?.Handler as IPlatformViewHandler)?.ViewController;
					if (controller?.ViewIfLoaded?.Window is not null)
					{
						await controller.PresentViewControllerAsync(wrapper, animated);
						await Task.Delay(5);
						return;
					}
				}

				// One might wonder why these delays are here... well thats a great question. It turns out iOS will claim the 
				// presentation is complete before it really is. It does not however inform you when it is really done (and thus 
				// would be safe to dismiss the VC). Fortunately this is almost never an issue

				if (WindowViewController is not null)
				{
					// This is branched, because if the modal is a popover and can't display correctly for some reason, we want it to fail
					if (wrapper.ModalPresentationStyle == UIKit.UIModalPresentationStyle.Popover)
					{
						if (wrapper.PopoverPresentationController is not null && WindowViewController.View is not null)
						{
							await WindowViewController.PresentViewControllerAsync(wrapper, animated);
							await Task.Delay(5);
						}
						else
						{
							failed = true;
						}
					}
					else
					{
						await WindowViewController.PresentViewControllerAsync(wrapper, animated);
						await Task.Delay(5);
					}
				}
			}
			catch
			{
				failed = true;
				throw;
			}
			finally
			{
				_waitForModalToFinish = false;
				presentFinished.SetResult();

				if (!failed)
				{
					SyncModalStackWhenPlatformIsReady();
				}
			}

		}

		void EndEditing()
		{
			// If any text entry controls have focus, we need to end their editing session
			// so that they are not the first responder; if we don't some things (like the activity indicator
			// on pull-to-refresh) will not work correctly. 

			// The topmost modal on the stack will have the Window; we can use that to end any current
			// editing that's going on 
			if (_platformModalPages.Count > 0)
			{
				var uiViewController = (_platformModalPages[_platformModalPages.Count - 1].Handler as IPlatformViewHandler)?.ViewController;
				uiViewController?.View?.Window?.EndEditing(true);
				return;
			}

			// TODO MAUI
			// If there aren't any modals, then the platform renderer will have the Window
			_window?.NativeWindow?.EndEditing(true);
		}
	}
}
