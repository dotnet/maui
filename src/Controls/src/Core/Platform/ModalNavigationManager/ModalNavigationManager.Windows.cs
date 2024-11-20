using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		WindowRootViewContainer Container =>
			_window.NativeWindow.Content as WindowRootViewContainer ??
			throw new InvalidOperationException("Root container Panel not found");

		bool _firstActivated;

		partial void InitializePlatform()
		{
			_window.Created += (_, _) => SyncModalStackWhenPlatformIsReady();
			_window.Destroying += (_, _) => _firstActivated = false;
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

					var windowManager = modalContext.GetNavigationRootManager();

					if (windowManager.RootView is WindowRootView wrv)
					{
						wrv.SetTitleBarBackgroundToTransparent(false);
					}

					var platform = newPage.ToPlatform(modalContext);
					_waitingForIncomingPage = platform.OnLoaded(() => completedCallback?.Invoke());
					windowManager.Connect(platform);
					Container.AddPage(windowManager.RootView);
				}
				// popping modal
				else
				{
					var windowManager = newPage.FindMauiContext()?.GetNavigationRootManager() ??
						throw new InvalidOperationException("Previous Page Has Lost its MauiContext");

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
	}
}
