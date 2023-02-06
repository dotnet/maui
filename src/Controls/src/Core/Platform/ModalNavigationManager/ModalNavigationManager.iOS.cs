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
		TaskCompletionSource? _platformActivated;

		partial void InitializePlatform()
		{
			_window.Activated += OnWindowActivated;
			_window.Resumed += (_, _) => SyncPlatformModalStack();
			_window.HandlerChanging += OnPlatformWindowHandlerChanging;

			if (!_window.IsActivated)
				_platformActivated = new TaskCompletionSource();
		}

		private void OnPlatformWindowHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			if (!_window.IsActivated && _platformActivated == null)
				_platformActivated = new TaskCompletionSource();
		}

		void OnWindowActivated(object? sender, EventArgs e)
		{
			if (_platformActivated != null)
			{
				var source = _platformActivated;
				_platformActivated = null;
				source?.SetResult();
			}

			SyncPlatformModalStack();
		}

		UIViewController? WindowViewController
		{
			get
			{
				if (_window?.Page?.Handler is IPlatformViewHandler pvh &&
					pvh.ViewController?.ViewIfLoaded?.Window != null)
				{
					return pvh.ViewController;
				}

				return WindowMauiContext.
						GetPlatformWindow()?
						.RootViewController;
			}
		}

		Task WindowReadyForModal() => _platformActivated?.Task ?? Task.CompletedTask;

		async Task<Page> PopModalPlatformAsync(bool animated)
		{
			var modal = CurrentPlatformModalPage;
			_platformModalPages.Remove(modal);

			var controller = (modal.Handler as IPlatformViewHandler)?.ViewController;

			if (controller?.ParentViewController is ModalWrapper modalWrapper &&
				modalWrapper.PresentingViewController != null)
			{
				await modalWrapper.PresentingViewController.DismissViewControllerAsync(animated);
				return modal;
			}

			// if the presnting VC is null that means the modal window was already dismissed
			// this will usually happen as a result of swapping out the content on the window
			// which is what was acting as the PresentingViewController
			return modal;
		}

		Task PushModalPlatformAsync(Page modal, bool animated)
		{
			EndEditing();

			_platformModalPages.Add(modal);

			if (_window?.Page?.Handler != null)
				return PresentModal(modal, animated && _window.IsActivated);

			return Task.CompletedTask;
		}

		async Task PresentModal(Page modal, bool animated)
		{
			modal.ToPlatform(WindowMauiContext);
			var wrapper = new ModalWrapper(modal.Handler as IPlatformViewHandler);

			if (_platformModalPages.Count > 1)
			{
				var topPage = _platformModalPages[_platformModalPages.Count - 2];
				var controller = (topPage?.Handler as IPlatformViewHandler)?.ViewController;
				if (controller != null)
				{
					await controller.PresentViewControllerAsync(wrapper, animated);
					await Task.Delay(5);
					return;
				}
			}

			// One might wonder why these delays are here... well thats a great question. It turns out iOS will claim the 
			// presentation is complete before it really is. It does not however inform you when it is really done (and thus 
			// would be safe to dismiss the VC). Fortunately this is almost never an issue

			if (WindowViewController != null)
			{
				await WindowViewController.PresentViewControllerAsync(wrapper, animated);
				await Task.Delay(5);
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
