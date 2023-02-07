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

		Page? CurrentPage =>
			_modalPages.Count > 0 ? _modalPages[_modalPages.Count - 1] : _window.Page;

		// Shell takes care of firing its own Modal life cycle events
		// With shell you cam remove / add multiple modals at once
		bool FireLifeCycleEvents => _window?.Page is not Shell;

		partial void InitializePlatform();

		public ModalNavigationManager(Window window)
		{
			_window = window;
			InitializePlatform();

			_window.HandlerChanging += OnWindowHandlerChanging;
			_window.Destroying += (_, _) =>
			{
				_platformModalPages.Clear();
			};
		}

		void OnWindowHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			// If the window handler is changing the activity is being recreated
			// the window activated/resumed event will take care of syncing the platform modals
			if (e.OldHandler is not null)
			{
				_platformModalPages.Clear();
			}
		}

		public Task<Page?> PopModalAsync()
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

		// This code only processes a single sync action per call.
		// It recursively calls itself until no more sync actions are left to perform.
		//
		// A lot can change during the process of pushing/popping a page
		// i.e. Users might change the root page during an appearing event.
		// So, instead of just bull dozing through the whole sync we perform one
		// sync step then recalculate the state of affairs and then perform another
		// until no more sync operations are left.
		// Typically it's always a good idea to re-evaluate after any async operation has completed
		async Task SyncPlatformModalStackAsync()
		{
			if (!IsModalReady || syncing)
				return;

			bool changed = false;

			try
			{
				syncing = true;

				await WindowReadyForModal();

				int popTo;

				for (popTo = 0; popTo < _platformModalPages.Count && popTo < _modalPages.Count; popTo++)
				{
					if (_platformModalPages[popTo] != _modalPages[popTo])
					{
						break;
					}
				}

				// This means the modal stacks are already synced so we don't have to do anything
				if (_platformModalPages.Count == _modalPages.Count && popTo == _platformModalPages.Count)
					return;

				// This ensures that appearing has fired on the final page that will be visible after 
				// the sync has finished
				CurrentPage?.SendAppearing();

				// Pop platform modal pages until we get to the point where the xplat expectation
				// matches the platform modals
				if (_platformModalPages.Count > popTo && IsModalReady)
				{
					var page = await PopModalPlatformAsync(false);
					page.Parent = null;
					changed = true;
				}

				if (!changed)
				{
					//push any modals that need to be synced
					var i = _platformModalPages.Count;
					if (i < _modalPages.Count && IsModalReady)
					{
						await PushModalPlatformAsync(_modalPages[i], false);
						changed = true;
					}
				}
			}
			finally
			{
				syncing = false;
			}

			if (changed)
			{
				await SyncPlatformModalStackAsync();
			}
		}

		public async Task<Page?> PopModalAsync(bool animated)
		{
			Page modal = _modalPages[_modalPages.Count - 1];

			if (_window.OnModalPopping(modal))
			{
				_window.OnPopCanceled();
				return null;
			}

			_modalPages.Remove(modal);

			if (FireLifeCycleEvents)
			{
				modal.SendNavigatingFrom(new NavigatingFromEventArgs());
				modal.SendDisappearing();
				CurrentPage?.SendAppearing();
			}

			Task popTask = 
				IsModalReady ? PopModalPlatformAsync(animated) : Task.CompletedTask;

			await popTask;
			modal.Parent = null;
			_window.OnModalPopped(modal);

			if (FireLifeCycleEvents)
			{
				modal.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage));
				CurrentPage?.SendNavigatedTo(new NavigatedToEventArgs(modal));
			}

			return modal;
		}

		public async Task PushModalAsync(Page modal, bool animated)
		{
			_window.OnModalPushing(modal);
			modal.Parent = _window;

			var previousPage = CurrentPage;
			_modalPages.Add(modal);

			if (FireLifeCycleEvents)
			{
				previousPage?.SendNavigatingFrom(new NavigatingFromEventArgs());
				previousPage?.SendDisappearing();
				CurrentPage?.SendAppearing();
			}

			if (IsModalReady)
			{
				if (ModalStack.Count == 0)
				{
					modal.NavigationProxy.Inner = _window.Navigation;
					await PushModalPlatformAsync(modal, animated);
				}
				else
				{
					await PushModalPlatformAsync(modal, animated);
					modal.NavigationProxy.Inner = _window.Navigation;
				}
			}

			if (FireLifeCycleEvents)
			{
				previousPage?.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage));
				CurrentPage?.SendNavigatedTo(new NavigatedToEventArgs(previousPage));
			}

			_window.OnModalPushed(modal);
		}

		internal void SettingNewPage()
		{
			if (_window.Page is null)
			{
				_currentPage = null;
				return;
			}

			if (_currentPage != _window.Page)
			{
				var previousPage = _currentPage;
				_currentPage = _window.Page;

				if (previousPage is not null)
					_modalPages.Clear();

				if (_currentPage is not null)
				{
					if (_currentPage.Handler is null)
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
			if (_currentPage is not null)
			{
				_currentPage.HandlerChanged -= OnCurrentPageHandlerChanged;
				SyncPlatformModalStack();
			}
		}

		partial void OnPageAttachedHandler();

		public void PageAttachedHandler() => OnPageAttachedHandler();
	}
}
