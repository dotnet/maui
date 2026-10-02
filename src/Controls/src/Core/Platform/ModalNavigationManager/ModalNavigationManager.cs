using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager : IModalNavigationHost
	{
		Window _window;
		public IReadOnlyList<Page> ModalStack => _modalPages.Pages;
		IMauiContext WindowMauiContext => _window.MauiContext;

		List<Page> _platformModalPages = new List<Page>();
		IReadOnlyList<Page> _readOnlyPlatformModalPages;
		NavigatingStepRequestList _modalPages = new NavigatingStepRequestList();

		// Animation flag for pops that were requested while the platform couldn't service them. The
		// request is removed from _modalPages as soon as the pop is requested, so without this the
		// deferred reconciliation pass has nothing left to read the caller's intent from.
		Dictionary<Page, bool> _pendingPopAnimations = new Dictionary<Page, bool>();

		Page? _currentPage;

		Page CurrentPlatformPage =>
			_platformModalPages.Count > 0 ? _platformModalPages[_platformModalPages.Count - 1] : (_window.Page ?? throw new InvalidOperationException("Current Window isn't loaded"));

		Page CurrentPlatformModalPage =>
			_platformModalPages.Count > 0 ? _platformModalPages[_platformModalPages.Count - 1] : throw new InvalidOperationException("Modal Stack is Empty");

		Page? CurrentPage
		{
			get
			{
				var currentPage = _modalPages.Count > 0 ? _modalPages[_modalPages.Count - 1].Page : _window.Page;

				if (currentPage is Shell shell)
					currentPage = shell.CurrentPage;

				return currentPage;
			}
		}

		// Shell takes care of firing its own Modal life cycle events
		// With shell you cam remove / add multiple modals at once
		bool FireLifeCycleEvents => _window?.Page is not Shell;

		partial void InitializePlatform();

		public ModalNavigationManager(Window window)
		{
			_window = window;
			_readOnlyPlatformModalPages = _platformModalPages.AsReadOnly();
			_window.PropertyChanged += (_, args) =>
			{
				if (args.Is(Window.PageProperty))
					SettingNewPage();
			};

			InitializePlatform();

			_window.HandlerChanging += OnWindowHandlerChanging;
			_window.HandlerChanged += OnWindowHandlerChanged;
			_window.Destroying += (_, _) =>
			{
				ClearModalPages(platform: true);

				// Teardown is terminal for the override: it must not come back to life on the next lazy
				// access while the window is being torn down. Only attaching a new handler (which brings
				// a new service scope, e.g. an Android activity recreation) re-enables resolution.
				var platformOverride = _platformOverride;
				_destroyed = true;
				InvalidateWindowScope();
				DisposePlatformOverride(platformOverride);
			};
		}

		IModalNavigationPlatform? _platformOverride;
		ILogger<ModalNavigationManager>? _platformOverrideLogger;

		// Latched once resolution has been attempted, so the factory is consulted exactly once per
		// window scope. It is also set (with a null override) to *block* resolution while the window is
		// between handlers, because during that window _window.Handler still points at the outgoing
		// handler and resolving would latch the scope that is going away.
		bool _platformOverrideResolved;

		// True only when resolution completed for the current scope without an override. A null
		// _platformOverride while this is false means routing is unavailable, not that the built-in
		// platform should be used.
		bool _useBuiltInPlatform;

		// Terminal after Window.Destroying. Cleared only when a new handler is installed.
		bool _destroyed;

		// Bumped whenever the window's service scope changes or goes away. Work queued against an older
		// generation is stale and must be dropped rather than run against a disconnected scope.
		int _scopeGeneration;
		int ScopeGeneration => Volatile.Read(ref _scopeGeneration);

		// The page handler the current override (or the built-in platform) has been notified about.
		IElementHandler? _pageAttachedNotifiedForHandler;

		void InvalidateWindowScope()
		{
			Interlocked.Increment(ref _scopeGeneration);

			// Block resolution until a new handler is actually installed.
			_platformOverride = null;
			_platformOverrideResolved = true;
			_useBuiltInPlatform = false;
			_pageAttachedNotifiedForHandler = null;
		}

		// Resolved lazily because a Window is constructed long before it has a handler, and therefore
		// long before it has a service scope to resolve from. Resolution latches only once the window
		// scope is available so that a factory registered by an external backend is always seen.
		IModalNavigationPlatform? PlatformOverride
		{
			get
			{
				if (_platformOverrideResolved || _destroyed)
					return _platformOverride;

				// The window handler's IMauiContext is the per-window service scope, so the platform
				// instance created here is inherently isolated to this window.
				var services = _window.Handler?.MauiContext?.Services;
				if (services is null)
					return null;

				try
				{
					_platformOverrideLogger = services.CreateLogger<ModalNavigationManager>();
					_platformOverride = services
						.GetService<IModalNavigationPlatformFactory>()?
						.CreateModalNavigationPlatform(this);
				}
				catch (Exception exception)
				{
					// A throwing factory must not surface from whatever arbitrary call site happened to
					// touch this property first (a property getter reached during a sync, a lifecycle
					// event, ...). Log it and fall back to the built-in platform permanently: retrying on
					// every access would turn a broken registration into a storm of exceptions.
					_platformOverrideLogger?
						.LogError(exception, "IModalNavigationPlatformFactory threw while creating the modal navigation platform. Falling back to the built-in platform for this window.");

					_platformOverride = null;
				}
				finally
				{
					_platformOverrideResolved = true;
					_useBuiltInPlatform = _platformOverride is null;
				}

				// The page handler may already have been attached before the window had a service scope
				// to resolve from, in which case PageAttached was never delivered.
				DeliverPageAttached();

				return _platformOverride;
			}
		}

		// Delivers PageAttached at most once per page handler: to the override when one is registered,
		// and to the built-in platform otherwise.
		//
		// The built-in notification is deliberately withheld until resolution has completed. Before the
		// window has a service scope we cannot yet tell whether a factory is registered, and running the
		// built-in hook then would leave an external backend with the built-in platform's hook installed
		// (on Tizen, the back-button handler) in addition to its own. Resolution re-delivers.
		void DeliverPageAttached()
		{
			var pageHandler = _window.Page?.Handler;

			if (pageHandler is null || ReferenceEquals(_pageAttachedNotifiedForHandler, pageHandler))
				return;

			// Resolving calls back into this method, so re-check afterwards to avoid delivering twice.
			var platformOverride = PlatformOverride;

			if (ReferenceEquals(_pageAttachedNotifiedForHandler, pageHandler))
				return;

			if (platformOverride is null && !_useBuiltInPlatform)
				return;

			_pageAttachedNotifiedForHandler = pageHandler;

			if (platformOverride is not null)
			{
				try
				{
					platformOverride.PageAttached();
				}
				catch (Exception exception)
				{
					_platformOverrideLogger?
						.LogError(exception, "IModalNavigationPlatform threw while handling page attachment.");
				}
			}
			else
				OnPageAttachedHandler();
		}

		void DisposePlatformOverride(IModalNavigationPlatform? platformOverride)
		{
			_platformOverride = null;

			// A replacement instance has to be told about the current page handler itself.
			_pageAttachedNotifiedForHandler = null;

			try
			{
				platformOverride?.Dispose();
			}
			catch (Exception exception)
			{
				_platformOverrideLogger?
					.LogError(exception, "IModalNavigationPlatform threw while being disposed.");
			}
			finally
			{
				_platformOverrideLogger = null;
			}
		}

		Window IModalNavigationHost.Window => _window;

		IMauiContext IModalNavigationHost.MauiContext => WindowMauiContext;

		IReadOnlyList<Page> IModalNavigationHost.PlatformModalStack => _readOnlyPlatformModalPages;

		Page? IModalNavigationHost.CurrentPage => CurrentPage;

		Page IModalNavigationHost.CurrentPlatformPage => CurrentPlatformPage;

		// Deliberately does NOT include IModalNavigationPlatform.IsReady. Implementations are expected
		// to consult this from their own IsReady, so folding the override back in here would recurse
		// into an uncatchable StackOverflowException.
		bool IModalNavigationHost.IsWindowReady => IsWindowReady;

		bool IModalNavigationHost.IsBatchPopping =>
			_window.Page is Shell shell && shell.CurrentItem?.CurrentItem?.IsPoppingModalStack == true;

		bool IModalNavigationHost.IsBatchPushing =>
			_window.Page is Shell shell && shell.CurrentItem?.CurrentItem?.IsPushingModalStack == true;

		void IModalNavigationHost.RequestSync()
		{
			// Snapshot the scope this request belongs to. A request that is queued and then overtaken by
			// teardown or a handler swap must not run against the replaced or disconnected scope.
			var generation = ScopeGeneration;

			// Documented as callable from any thread. The whole reconciliation entry — readiness checks,
			// page lifecycle events and the platform push/pop calls — must run on the UI thread, so
			// marshal the entire entry point when the caller isn't already there.
			var dispatcher = TryGetWindowDispatcher();

			if (dispatcher is not null && dispatcher.IsDispatchRequired)
			{
				dispatcher.Dispatch(() => SyncModalStackFromPlatformRequest(generation));
				return;
			}

			SyncModalStackFromPlatformRequest(generation);
		}

		// Window.Dispatcher is independent of the window handler, so dispatch remains available during
		// handler teardown. A window created without any dispatcher is the documented best-effort case.
		IDispatcher? TryGetWindowDispatcher()
		{
			try
			{
				return _window.Dispatcher;
			}
			catch (InvalidOperationException)
			{
				// The window was created on a thread without a dispatcher and there is no application
				// dispatcher to fall back to. Run inline rather than dropping the request.
				return null;
			}
		}

		void SyncModalStackFromPlatformRequest(int generation)
		{
			// Drop callbacks that were queued before the window was destroyed, or before its handler was
			// replaced. Running them would repopulate modal state on a torn-down window, or drive
			// presentation through resources belonging to a scope that no longer exists.
			if (_destroyed || generation != ScopeGeneration)
				return;

			SyncModalStackWhenPlatformIsReady();
		}

		void OnWindowHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			// If the window handler is changing the activity is being recreated
			// the window activated/resumed event will take care of syncing the platform modals
			if (e.OldHandler is not null)
			{
				ClearModalPages(platform: true);

				// The override was created against the old handler's service scope and most likely holds
				// platform views owned by it, so drop it.
				var platformOverride = _platformOverride;

				// Invalidate before calling external disposal code. A backend may call RequestSync
				// reentrantly from Dispose, and that request must not route through the built-in platform.
				InvalidateWindowScope();
				DisposePlatformOverride(platformOverride);
				return;
			}

			// _window.Handler is still the OUTGOING handler at this point, so resolution stays blocked
			// until OnWindowHandlerChanged confirms the new one is installed. Without this a reentrant
			// sync could latch an override built from the scope that is going away.
			InvalidateWindowScope();
		}

		void OnWindowHandlerChanged(object? sender, EventArgs e)
		{
			Interlocked.Increment(ref _scopeGeneration);

			if (_window.Handler is null)
				return;

			// A new handler brings a new service scope. This is the only path that lifts the blocked
			// state set by OnWindowHandlerChanging, and the terminal state set by teardown.
			_destroyed = false;
			_platformOverride = null;
			_platformOverrideLogger = null;
			_platformOverrideResolved = false;
			_useBuiltInPlatform = false;
			_pageAttachedNotifiedForHandler = null;
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
		int syncingGeneration = -1;
		int queuedSyncGeneration = -1;

		// Framework-side readiness only: the window and its page both have handlers. Deliberately kept
		// free of any IModalNavigationPlatform.IsReady contribution so it is safe for an override to
		// consult through IModalNavigationHost.IsWindowReady from its own IsReady.
		bool IsWindowReady =>
			_window?.Page?.Handler is not null &&
			_window.Handler is not null;

		bool IsModalReady => IsWindowReady && IsModalPlatformReady;

		// Routing layer between the cross-platform modal orchestration above and the actual
		// presentation below. When an IModalNavigationPlatformFactory is registered the resolved
		// override handles presentation; otherwise the built-in platform partials run unchanged.
		bool IsModalPlatformReady
		{
			get
			{
				var platformOverride = PlatformOverride;
				return platformOverride?.IsReady ?? (_useBuiltInPlatform && IsModalPlatformReadyCore);
			}
		}

		Task SyncModalStackWhenPlatformIsReadyAsync()
		{
			var platformOverride = PlatformOverride;
			if (platformOverride is null)
				return _useBuiltInPlatform ? SyncModalStackWhenPlatformIsReadyCoreAsync() : Task.CompletedTask;

			// An override that isn't ready is expected to call IModalNavigationHost.RequestSync when it
			// becomes ready, so there's nothing to wait on here.
			return platformOverride.IsReady ? SyncPlatformModalStackAsync() : Task.CompletedTask;
		}

		Task<Page> PopModalPlatformAsync(bool animated)
		{
			var platformOverride = PlatformOverride;
			if (platformOverride is null)
			{
				return _useBuiltInPlatform
					? PopModalPlatformCoreAsync(animated)
					: Task.FromException<Page>(new InvalidOperationException("Modal presentation is unavailable for the current window scope."));
			}

			return PopModalWithOverrideAsync(platformOverride, animated);
		}

		async Task<Page> PopModalWithOverrideAsync(IModalNavigationPlatform platformOverride, bool animated)
		{
			var modal = CurrentPlatformModalPage;
			var generation = ScopeGeneration;

			// Removed before dismissing so that CurrentPlatformPage already refers to the page being
			// revealed while the override runs. This matches the ordering of the built-in platforms.
			_platformModalPages.Remove(modal);

			try
			{
				await platformOverride.PopModalAsync(modal, animated);
			}
			catch
			{
				// The dismissal didn't take effect, so the modal is almost certainly still on screen.
				// Put it back on the platform stack: the platform stack tracks what is presented, and
				// dropping the entry here would leave a visible modal that is absent from both stacks
				// and therefore impossible to reach again. It was the top of the stack, so appending
				// restores its position and the next reconciliation pass retries the dismissal.
				if (IsCurrentPlatformScope(platformOverride, generation) &&
					!_platformModalPages.Contains(modal))
					_platformModalPages.Add(modal);

				throw;
			}

			return modal;
		}

		Task PushModalPlatformAsync(Page modal, bool animated)
		{
			var platformOverride = PlatformOverride;
			if (platformOverride is null)
			{
				return _useBuiltInPlatform
					? PushModalPlatformCoreAsync(modal, animated)
					: Task.FromException(new InvalidOperationException("Modal presentation is unavailable for the current window scope."));
			}

			return PushModalWithOverrideAsync(platformOverride, modal, animated);
		}

		async Task PushModalWithOverrideAsync(IModalNavigationPlatform platformOverride, Page modal, bool animated)
		{
			var generation = ScopeGeneration;
			_platformModalPages.Add(modal);

			try
			{
				await platformOverride.PushModalAsync(modal, animated);
			}
			catch
			{
				// The presentation didn't take effect, so the modal is not on screen. The platform stack
				// tracks what is presented, so drop the entry again; the requested stack still contains
				// the modal, and the next reconciliation pass retries the presentation.
				if (IsCurrentPlatformScope(platformOverride, generation))
					_platformModalPages.Remove(modal);

				throw;
			}
		}

		bool IsCurrentPlatformScope(IModalNavigationPlatform platformOverride, int generation) =>
			generation == ScopeGeneration &&
			ReferenceEquals(platformOverride, _platformOverride);

		void SyncPlatformModalStack([CallerMemberName] string? callerName = null)
		{
			var logger = _window.FindMauiContext(true)?.Services?.CreateLogger<ModalNavigationManager>();
			SyncPlatformModalStackAsync().FireAndForget(logger, callerName);
		}

		void SyncModalStackWhenPlatformIsReady([CallerMemberName] string? callerName = null)
		{
			var logger = _window.FindMauiContext(true)?.Services?.CreateLogger<ModalNavigationManager>();
			SyncModalStackWhenPlatformIsReadyAsync().FireAndForget(logger, callerName);
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
			if (!IsModalReady)
				return;

			if (syncing)
			{
				// A handler replacement can request reconciliation while an operation from the outgoing
				// scope is still awaiting. Remember requests from the newer scope so the old operation
				// hands off even if it faults and therefore cannot set syncAgain. Same-scope requests
				// retain the existing behavior and do not turn repeated platform failures into a retry loop.
				if (ScopeGeneration != syncingGeneration)
					queuedSyncGeneration = ScopeGeneration;

				return;
			}

			bool syncAgain = false;
			var generation = ScopeGeneration;

			try
			{
				syncing = true;
				syncingGeneration = generation;

				int popTo;

				for (popTo = 0; popTo < _platformModalPages.Count && popTo < _modalPages.Count; popTo++)
				{
					if (_platformModalPages[popTo] != _modalPages[popTo].Page)
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
					var platformModal = CurrentPlatformModalPage;

					bool animated = false;
					if (_modalPages.TryGetValue(platformModal, out var request))
					{
						_modalPages.Remove(platformModal);
						animated = request.IsAnimated;
					}
					else if (_pendingPopAnimations.TryGetValue(platformModal, out var pendingAnimated))
					{
						// The pop was requested while the platform couldn't service it, so the request was
						// already taken off the logical stack. Without this the caller's `animated` value
						// would be lost and every deferred dismissal would be unanimated.
						animated = pendingAnimated;
					}

					// Record the intent for the duration of the attempt. It is only cleared once the
					// dismissal actually succeeds, so a failed pop that gets retried by a later pass still
					// animates the way the caller asked.
					_pendingPopAnimations[platformModal] = animated;

					var page = await PopModalPlatformAsync(animated);
					_pendingPopAnimations.Remove(page);
					page.Parent?.RemoveLogicalChild(page);
					syncAgain = true;
				}

				if (!syncAgain)
				{
					//push any modals that need to be synced
					var i = _platformModalPages.Count;
					if (i < _modalPages.Count && IsModalReady)
					{
						var nextRequest = _modalPages[i];
						var nextPage = nextRequest.Page;
						bool animated = nextRequest.IsAnimated;

						await PushModalPlatformAsync(nextPage, animated);
						syncAgain = true;
					}
				}
			}
			finally
			{
				var syncNewScope = queuedSyncGeneration >= 0 &&
					queuedSyncGeneration != generation;
				queuedSyncGeneration = -1;

				// Code has multiple exit points during the sync operation.
				// So we're using a try/finally to ensure that syncing always 
				// gets transitioned to false. If more exit points are added at a later point  
				// we don't have to always worry about the exit point setting syncing to false.
				syncing = false;
				syncingGeneration = -1;

				// syncAgain is only set after a successful operation so we won't hit a case here
				// where we hit an infinite loop of syncing. A newer scope's queued request is also safe
				// to run because it reconciles a different platform instance.
				if (syncAgain || syncNewScope)
				{
					await SyncModalStackWhenPlatformIsReadyAsync().ConfigureAwait(false);
				}
			}
		}

		Task _waitForModalToFinishTask = Task.CompletedTask;

		public async Task<Page?> PopModalAsync(bool animated)
		{
			if (_modalPages.Count <= 0)
				throw new InvalidOperationException("PopModalAsync failed because modal stack is currently empty.");

			await _waitForModalToFinishTask;

			Page modal = _modalPages[_modalPages.Count - 1].Page;

			if (_window.OnModalPopping(modal))
			{
				_window.OnPopCanceled();
				return null;
			}

			_modalPages.Remove(modal);

			if (FireLifeCycleEvents)
			{
				modal.SendNavigatingFrom(new NavigatingFromEventArgs(CurrentPage, NavigationType.Pop));
			}

			modal.SendDisappearing();

			// With shell we want to make sure to only fire the appearing event
			// on the final page that will be visible after the pop has completed
			if (_window.Page is Shell shell)
			{
				if (!shell.CurrentItem.CurrentItem.IsPoppingModalStack)
				{
					CurrentPage?.SendAppearing();
				}
			}
			else
			{
				CurrentPage?.SendAppearing();
			}

			bool isPlatformReady = IsModalReady;
			bool isPresented = _platformModalPages.Contains(modal);
			bool applyNow = isPlatformReady && !syncing && isPresented;

			// The request has already been taken off the logical stack above, so reconciliation would
			// otherwise have no way to recover how the caller wanted this dismissal animated. Keep it
			// only when the platform has actually presented the modal, and clear it once the platform
			// dismisses the modal, so both a deferred pop and a retry after a failed pop keep the caller's
			// intent without retaining pages that never reached the platform.
			if (isPresented)
				_pendingPopAnimations[modal] = animated;

			Task popTask = applyNow ? PopModalPlatformAsync(animated) : Task.CompletedTask;

			await popTask;

			if (applyNow)
				_pendingPopAnimations.Remove(modal);

			modal.Parent?.RemoveLogicalChild(modal);
			_window.OnModalPopped(modal);

			if (FireLifeCycleEvents)
			{
				modal.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage, NavigationType.Pop));
				CurrentPage?.SendNavigatedTo(new NavigatedToEventArgs(modal, NavigationType.Pop));
			}

			if (!applyNow)
				SyncModalStackWhenPlatformIsReady();

			return modal;
		}

		public async Task PushModalAsync(Page modal, bool animated)
		{
			await _waitForModalToFinishTask;

			_window.OnModalPushing(modal);

			var previousPage = CurrentPage;
			_modalPages.Add(new NavigationStepRequest(modal, true, animated));
			_window.AddLogicalChild(modal);

			if (FireLifeCycleEvents)
			{
				previousPage?.SendNavigatingFrom(new NavigatingFromEventArgs(CurrentPage, NavigationType.Push));
			}

			if (_window.Page is Shell shell)
			{
				// With shell we want to make sure to only fire the appearing event
				// on the final page that will be visible after the pop has completed
				if (!shell.CurrentItem.CurrentItem.IsPushingModalStack)
				{
					previousPage?.SendDisappearing();
					CurrentPage?.SendAppearing();
				}
			}
			else
			{
				previousPage?.SendDisappearing();
				CurrentPage?.SendAppearing();
			}

			bool isPlatformReady = IsModalReady;
			if (isPlatformReady && !syncing)
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
				previousPage?.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage, NavigationType.Push));
				CurrentPage?.SendNavigatedTo(new NavigatedToEventArgs(previousPage, NavigationType.Push));
			}

			_window.OnModalPushed(modal);

			if (!isPlatformReady)
				SyncModalStackWhenPlatformIsReady();
		}

		void SettingNewPage()
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
				{
					previousPage.HandlerChanged -= OnCurrentPageHandlerChanged;
					ClearModalPages(xplat: true);
				}

				if (_currentPage is not null)
				{
					if (_currentPage.Handler is null)
					{
						_currentPage.HandlerChanged += OnCurrentPageHandlerChanged;
					}
					else
					{
						SyncModalStackWhenPlatformIsReady();
					}
				}
			}
		}

		void OnCurrentPageHandlerChanged(object? sender, EventArgs e)
		{
			if (_currentPage is not null)
			{
				_currentPage.HandlerChanged -= OnCurrentPageHandlerChanged;
				SyncModalStackWhenPlatformIsReady();
			}
		}

		partial void OnPageAttachedHandler();

		public void PageAttachedHandler() => DeliverPageAttached();

		void ClearModalPages(bool xplat = false, bool platform = false)
		{
			if (xplat)
				_modalPages.Clear();

			if (platform)
			{
				_platformModalPages.Clear();

				// Nothing is presented any more, so there are no deferred dismissals left to describe.
				_pendingPopAnimations.Clear();
			}
		}

		// Windows and Android have basically the same requirement that
		// we need to wait for the current page to finish loading before
		// satisfying Modal requests.
		// This will most likely change once we switch Android to using dialog fragments		
#if WINDOWS || ANDROID
		IDisposable? _platformPageWatchingForLoaded;

		async Task SyncModalStackWhenPlatformIsReadyCoreAsync()
		{
			DisconnectPlatformPageWatchingForLoaded();

			if (IsModalPlatformReadyCore)
			{
				await SyncPlatformModalStackAsync().ConfigureAwait(false);
			}
			else if (IsWindowReadyForModals)
			{
				if (CurrentPlatformPage.Handler is null)
				{
					CurrentPlatformPage.HandlerChanged += OnCurrentPlatformPageHandlerChanged;

					_platformPageWatchingForLoaded = new ActionDisposable(() =>
					{
						CurrentPlatformPage.HandlerChanged -= OnCurrentPlatformPageHandlerChanged;
					});
				}
				// This accounts for cases where we swap the root page out
				// We want to wait for that to finish loading before processing any modal changes
#if ANDROID
				else if (_window?.Page is not null && !_window.Page.IsLoadedOnPlatform())
				{
					var windowPage = _window.Page;
					_platformPageWatchingForLoaded =
						windowPage.OnLoaded(() => OnCurrentPlatformPageLoaded(windowPage, EventArgs.Empty));
				}
#endif

				if (!CurrentPlatformPage.IsLoadedOnPlatform() &&
						  CurrentPlatformPage.Handler is not null)
				{
					var currentPlatformPage = CurrentPlatformPage;
					_platformPageWatchingForLoaded =
						currentPlatformPage.OnLoaded(() => OnCurrentPlatformPageLoaded(currentPlatformPage, EventArgs.Empty));
				}
			}
		}

		void OnCurrentPlatformPageHandlerChanged(object? sender, EventArgs e)
		{
			DisconnectPlatformPageWatchingForLoaded();
			SyncModalStackWhenPlatformIsReady();
		}

		void DisconnectPlatformPageWatchingForLoaded()
		{
			_platformPageWatchingForLoaded?.Dispose();
		}

		void OnCurrentPlatformPageLoaded(object? sender, EventArgs e)
		{
			DisconnectPlatformPageWatchingForLoaded();
			SyncPlatformModalStack();
		}

		bool IsWindowReadyForModals =>
					_window?.Page?.Handler is not null &&
#if WINDOWS
					_firstActivated;
#else
					_window.IsActivated;
#endif

		bool IsModalPlatformReadyCore
		{
			get
			{
				bool result =
					IsWindowReadyForModals
#if ANDROID
					&& _window?.Page?.IsLoadedOnPlatform() == true
#endif
					&& CurrentPlatformPage?.Handler is not null
					&& CurrentPlatformPage.IsLoadedOnPlatform();

				if (result)
					DisconnectPlatformPageWatchingForLoaded();

				return result;
			}
		}
#endif
	}
}
