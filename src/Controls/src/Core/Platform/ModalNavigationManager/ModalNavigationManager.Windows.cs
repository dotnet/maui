using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Windowing;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		WindowRootViewContainer Container =>
			_window.NativeWindow.Content as WindowRootViewContainer ??
			throw new InvalidOperationException("Root container Panel not found");

		bool _firstActivated;

		// Tracks the AppWindow subscription used to update the top modal's title bar
		// when the user enters or exits full-screen while a modal is visible.
		AppWindow? _appWindowForPresenterChanges;

		partial void InitializePlatform()
		{
			_window.Created += (_, _) => SyncModalStackWhenPlatformIsReady();
			_window.Destroying += (_, _) =>
			{
				_firstActivated = false;
				UnsubscribeFromPresenterChanges();
			};
			_window.Activated += OnWindowActivated;
		}

		void OnWindowActivated(object? sender, EventArgs e)
		{
			if (!_firstActivated)
			{
				_firstActivated = true;
				SyncModalStackWhenPlatformIsReady();
			}
		}

		Task<Page> PopModalPlatformAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			var poppedPage = CurrentPlatformModalPage;
			_platformModalPages.Remove(poppedPage);
			SetCurrent(CurrentPlatformPage, poppedPage, true, () => tcs.SetResult(poppedPage));
			return tcs.Task;
		}

		Task PushModalPlatformAsync(Page modal, bool animated)
		{
			_ = modal ?? throw new ArgumentNullException(nameof(modal));

			var tcs = new TaskCompletionSource<bool>();
			var currentPage = CurrentPlatformPage;
			_platformModalPages.Add(modal);
			SetCurrent(modal, currentPage, false, () => tcs.SetResult(true));
			return tcs.Task;
		}

		void RemovePage(Page page, bool popping)
		{
			if (page is null)
				return;

			var mauiContext = page.FindMauiContext() ??
				throw new InvalidOperationException("Maui Context removed from outgoing page too early");

			var windowManager = mauiContext.GetNavigationRootManager();
			Container.RemovePage(windowManager.RootView);

			if (!popping)
				return;

			page
				.FindMauiContext()
				?.GetNavigationRootManager()
				?.Disconnect();

			page.Handler?.DisconnectHandler();
		}

		IDisposable? _waitingForIncomingPage;
		void SetCurrent(
			Page newPage,
			Page previousPage,
			bool popping,
			Action? completedCallback = null)
		{
			try
			{
				_waitingForIncomingPage?.Dispose();

				if (popping)
				{
					RemovePage(previousPage, popping);
				}
				else if (newPage.BackgroundColor.IsDefault() && newPage.Background.IsEmpty)
				{
					RemovePage(previousPage, popping);
				}

				if (Container is null || newPage is null)
					return;

				// pushing modal
				if (!popping)
				{
					var modalContext =
						WindowMauiContext
							.MakeScoped(registerNewNavigationRoot: true);

					newPage.Toolbar ??= new Toolbar(newPage);
					_ = newPage.Toolbar.ToPlatform(modalContext);

					// Hide titlebar on previous page
					var previousContext = previousPage.FindMauiContext();
					if (previousContext is not null)
					{
						var navRoot = previousContext.GetNavigationRootManager();
						if (navRoot.RootView is WindowRootView wrv && wrv.AppTitleBarContainer is not null)
						{
							wrv.SetTitleBarVisibility(UI.Xaml.Visibility.Collapsed);
						}
					}

					var windowManager = modalContext.GetNavigationRootManager();
					if (windowManager is not null)
					{
						// The NavigationRootManager constructor unconditionally reserves 32px for the
						// title bar based on ExtendsContentIntoTitleBar (true by default). In full-screen
						// mode there are no OS window controls to avoid, so suppress this reservation.
						// This also clears the InputNonClientPointerSource passthrough regions, fixing
						// the unclickable top area that would otherwise remain in full-screen mode.
						if (IsWindowFullScreen())
						{
							windowManager.SetTitleBarVisibility(false);
						}

						// Set the titlebar on the new navigation root
						if (previousPage is not null &&
							previousPage.GetParentWindow() is Window window &&
							window.TitleBar is TitleBar titlebar)
						{
							windowManager.SetTitleBar(titlebar, modalContext);
						}

						var platform = newPage.ToPlatform(modalContext);
						_waitingForIncomingPage = platform.OnLoaded(() => completedCallback?.Invoke());
						windowManager.Connect(platform);
						Container.AddPage(windowManager.RootView);

						// Subscribe once (when first modal is pushed) so that full-screen
						// entry/exit while the modal is shown updates the modal's title bar.
						if (_platformModalPages.Count == 1)
						{
							SubscribeToPresenterChanges();
						}
					}
				}
				// popping modal
				else
				{
					// Unsubscribe from presenter changes once the last modal is popped.
					if (_platformModalPages.Count == 0)
					{
						UnsubscribeFromPresenterChanges();
					}

					var context = newPage.FindMauiContext();
					var windowManager = context?.GetNavigationRootManager() ??
						throw new InvalidOperationException("Previous Page Has Lost its MauiContext");

					// Toggle the titlebar visibility on the new page.
					// Use NavigationRootManager.SetTitleBarVisibility(bool) so that height
					// and margins are also reset — not just the container's visual visibility.
					var navRoot = context?.GetNavigationRootManager();
					navRoot?.SetTitleBarVisibility(!IsWindowFullScreen());

					// Restore the titlebar
					if (previousPage is not null &&
						previousPage.GetParentWindow() is Window window &&
						window.TitleBar is TitleBar titlebar)
					{
						windowManager.SetTitleBar(titlebar, context);
					}

					var platform = newPage.ToPlatform();
					_waitingForIncomingPage = platform.OnLoaded(() => completedCallback?.Invoke());
					Container.AddPage(windowManager.RootView);
				}
			}
			catch (Exception error) when (error.HResult == -2147417842)
			{
				//This exception prevents the Main Page from being changed in a child
				//window or a different thread, except on the Main thread.
				//HEX 0x8001010E 
				throw new InvalidOperationException(
					"Changing the current page is only allowed if it's being called from the same UI thread." +
					"Please ensure that the new page is in the same UI thread as the current page.", error);
			}
		}

		// Returns true when the window is currently in full-screen presentation mode.
		bool IsWindowFullScreen()
		{
			var platformWindow = WindowMauiContext?.GetPlatformWindow();
			if (platformWindow is null)
			{
				return false;
			}

			return platformWindow.GetAppWindow()?.Presenter?.Kind == AppWindowPresenterKind.FullScreen;
		}

		// Subscribes to AppWindow.Changed (once) when the first modal is pushed so that
		// entering or exiting full-screen while a modal is visible is handled correctly.
		void SubscribeToPresenterChanges()
		{
			if (_appWindowForPresenterChanges is not null)
			{
				return;
			}

			var platformWindow = WindowMauiContext?.GetPlatformWindow();
			if (platformWindow is null)
			{
				return;
			}

			var appWindow = platformWindow.GetAppWindow();
			if (appWindow is null)
			{
				return;
			}

			_appWindowForPresenterChanges = appWindow;
			_appWindowForPresenterChanges.Changed += OnAppWindowPresenterChanged;
		}

		// Removes the AppWindow.Changed subscription when the last modal is popped or
		// the window is being destroyed.
		void UnsubscribeFromPresenterChanges()
		{
			if (_appWindowForPresenterChanges is null)
			{
				return;
			}

			_appWindowForPresenterChanges.Changed -= OnAppWindowPresenterChanged;
			_appWindowForPresenterChanges = null;
		}

		// Fires whenever the window's AppWindow.Presenter changes (e.g., windowed ↔ full-screen).
		// Updates the top-most modal's NavigationRootManager so its title bar height and margins
		// match the new presenter mode.
		void OnAppWindowPresenterChanged(AppWindow sender, AppWindowChangedEventArgs args)
		{
			if (!args.DidPresenterChange || _platformModalPages.Count == 0)
			{
				return;
			}

			var topModal = _platformModalPages[_platformModalPages.Count - 1];
			var rootManager = topModal.FindMauiContext()?.GetNavigationRootManager();
			rootManager?.SetTitleBarVisibility(sender.Presenter?.Kind != AppWindowPresenterKind.FullScreen);
		}
	}
}
