using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		Window _window;
		public IReadOnlyList<Page> ModalStack => _modalPages;
		IMauiContext WindowMauiContext => _window.MauiContext;

		List<Page> _modalPages = new List<Page>();
		List<Page> _platformModalPages = new List<Page>();
		Page? _currentPage;
		Page CurrentPlatformPage =>
			_platformModalPages.Count > 0 ? _platformModalPages[_platformModalPages.Count - 1] : (_window.Page ?? throw new InvalidOperationException("Current Window isn't loaded"));

		Page CurrentPlatformModalPage =>
			_platformModalPages.Count > 0 ? _platformModalPages[_platformModalPages.Count - 1] : throw new InvalidOperationException("Modal Stack is Empty");

		public ModalNavigationManager(Window window)
		{
			_window = window;

#if WINDOWS
			_window.Created += (_, _) => SyncPlatformModalStack();
#else
			_window.Activated += (_, _) => SyncPlatformModalStack();
			_window.Resumed += (_, _) => SyncPlatformModalStack();
#endif
			_window.HandlerChanging += OnWindowHandlerChanging;
			_window.Destroying += (_, _) => _platformModalPages.Clear();
		}

		void OnWindowHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			// If the window handler is changing the activity is being recreated
			// the window activated/resumed event will take care of syncing the platform modals
			if (e.OldHandler != null)
			{
				_platformModalPages.Clear();
			}
		}

		public Task<Page> PopModalAsync()
		{
			return PopModalAsync(true);
		}

		public Task PushModalAsync(Page modal)
		{
			return PushModalAsync(modal, true);
		}

		bool syncing = false;

		bool IsModalReady
		{
			get
			{
				return
#if ANDROID
					_window.IsActivated &&
#endif
					_window?.Page?.Handler != null && _window.Handler != null;
			}
		}

		void SyncPlatformModalStack([CallerMemberName] string? callerName = null)
		{
			var logger = _window.FindMauiContext(true)?.Services?.CreateLogger<ModalNavigationManager>();
			SyncPlatformModalStackAsync().FireAndForget(logger, callerName);
		}

		async Task SyncPlatformModalStackAsync()
		{
			if (!IsModalReady || syncing)
				return;

			try
			{
				syncing = true;
				int popTo = 0;

				for (var i = 0; i < _platformModalPages.Count && i < _modalPages.Count; i++)
				{
					if (_platformModalPages[i] != _modalPages[i])
					{
						break;
					}

					popTo = i + 1;
				}

				// This means the modal stacks are already synced so we don't have to do anything
				if (_platformModalPages.Count == _modalPages.Count && popTo == _platformModalPages.Count)
					return;

				await WindowReadyForModal();

				// Pop platform modal pages until we get to the point where the xplat expectation
				// matches the platform modals
				while (_platformModalPages.Count > popTo && IsModalReady)
				{
					await PopModalPlatformAsync(false);
				}

				//push any modals that need to be synced
				for (var i = _platformModalPages.Count; i < _modalPages.Count && IsModalReady; i++)
				{
					await PushModalPlatformAsync(_modalPages[i], false);
				}
			}
			finally
			{
				syncing = false;
			}
		}

		public Task<Page> PopModalAsync(bool animated)
		{
			Page modal = _modalPages[_modalPages.Count - 1];
			_modalPages.Remove(modal);

			if (!IsModalReady)
			{
				return Task.FromResult(modal);
			}
			else
			{
				return PopModalPlatformAsync(animated);
			}
		}

		public Task PushModalAsync(Page modal, bool animated)
		{
			_modalPages.Add(modal);

			if (!IsModalReady)
			{
				return Task.CompletedTask;
			}
			else
			{
				return PushModalPlatformAsync(modal, animated);
			}
		}

		internal void SettingNewPage()
		{
			if (_window.Page == null)
			{
				_currentPage = null;
				return;
			}

			if (_currentPage != _window.Page)
			{
				var previousPage = _currentPage;
				_currentPage = _window.Page;

				if (previousPage != null)
					_modalPages.Clear();

				if (_currentPage != null)
				{
					if (_currentPage.Handler == null)
					{
						_currentPage.HandlerChanged += OnCurrentPageHandlerChanged;
					}
					else
					{
						SyncPlatformModalStack();
					}
				}
			}
		}

		void OnCurrentPageHandlerChanged(object? sender, EventArgs e)
		{
			if (_currentPage != null)
			{
				_currentPage.HandlerChanged -= OnCurrentPageHandlerChanged;
				SyncPlatformModalStack();
			}
		}

		partial void OnPageAttachedHandler();

		public void PageAttachedHandler() => OnPageAttachedHandler();
	}
}
