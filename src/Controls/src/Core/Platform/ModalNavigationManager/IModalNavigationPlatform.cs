#nullable enable

using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Presents and dismisses modal pages for a single <see cref="Window"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Register an <see cref="IModalNavigationPlatformFactory"/> in the application's service collection
	/// to supply an implementation. This is the extensibility point that lets an alternative platform
	/// backend render modal navigation without forking the framework: the framework keeps ownership of
	/// the cross-platform modal stack, page lifecycle events and the push/pop reconciliation loop, and
	/// only the visual presentation is delegated here.
	/// </para>
	/// <para>
	/// The framework creates one instance per window and disposes it when the window is destroyed or
	/// when the window's handler changes. Implementations must therefore tolerate multiple
	/// create/dispose cycles and must not be shared between windows.
	/// </para>
	/// <para>
	/// All members are invoked on the UI thread.
	/// </para>
	/// <para>
	/// <b>Dismissals that start on the platform.</b> When the user dismisses a modal natively — an
	/// interactive swipe-to-dismiss, a hardware back press, a native close button — the framework does
	/// not observe it. The implementation must route it back through
	/// <c>host.Window.Navigation.PopModalAsync()</c> so the cross-platform modal stack and the page
	/// lifecycle events stay in sync. The framework then calls
	/// <see cref="PopModalAsync(Page, bool)"/> for that page as part of applying the pop, so
	/// <see cref="PopModalAsync(Page, bool)"/> must be idempotent and complete successfully when the
	/// native modal is already gone.
	/// </para>
	/// <para>
	/// <b>Disposal.</b> <see cref="IDisposable.Dispose"/> is called when the window is destroyed and
	/// when the window's handler changes. The framework does <b>not</b> call
	/// <see cref="PopModalAsync(Page, bool)"/> for modals that are still presented at that point, so
	/// <see cref="IDisposable.Dispose"/> is solely responsible for dismissing and releasing every
	/// native modal the backend still owns. Do not assume
	/// <see cref="IModalNavigationHost.MauiContext"/>, the window handler, or any platform view
	/// obtained through them is still usable there — teardown may already have disposed the window's
	/// service scope. Capture whatever cleanup needs while the instance is live and treat disposal as
	/// best effort. A disposed instance is never reused; a new one is created from the factory if the
	/// window gets a new handler.
	/// </para>
	/// </remarks>
	public interface IModalNavigationPlatform : IDisposable
	{
		/// <summary>
		/// Gets a value indicating whether the backend can present or dismiss a modal right now.
		/// </summary>
		/// <remarks>
		/// <para>
		/// While this returns <see langword="false"/> the framework still records pushes and pops on the
		/// cross-platform modal stack and still raises the corresponding page lifecycle events, but it
		/// does not call <see cref="PushModalAsync(Page, bool)"/> or <see cref="PopModalAsync(Page, bool)"/>.
		/// When the backend becomes ready it must call
		/// <see cref="IModalNavigationHost.RequestSync"/> so the queued operations are applied.
		/// Return <see langword="true"/> unconditionally if the backend has no readiness requirement.
		/// </para>
		/// <para>
		/// Deferring has an observable consequence for callers: <c>Navigation.PushModalAsync</c> and
		/// <c>Navigation.PopModalAsync</c> complete as soon as the framework has updated its own state,
		/// so they do not wait for the deferred presentation and cannot report a failure in it. See the
		/// remarks on <see cref="PushModalAsync(Page, bool)"/> for how deferred faults are reported.
		/// </para>
		/// <para>
		/// Implementations may consult <see cref="IModalNavigationHost.IsWindowReady"/> here. Do not try
		/// to derive this from the framework's overall modal readiness, which already folds this
		/// property in.
		/// </para>
		/// </remarks>
		bool IsReady { get; }

		/// <summary>
		/// Presents <paramref name="modal"/> on top of
		/// <see cref="IModalNavigationHost.CurrentPlatformPage"/>.
		/// </summary>
		/// <param name="modal">The page to present. It has already been added to
		/// <see cref="IModalNavigationHost.PlatformModalStack"/> when this method is called.</param>
		/// <param name="animated"><see langword="true"/> to animate the transition.</param>
		/// <returns>
		/// A task that completes once the modal is on screen and safe to dismiss. The framework awaits
		/// this task before applying any further modal operation, so completing early can allow a
		/// subsequent pop to race the presentation.
		/// </returns>
		/// <remarks>
		/// <para>
		/// If this task faults the framework removes <paramref name="modal"/> from
		/// <see cref="IModalNavigationHost.PlatformModalStack"/> again, because the modal is not on
		/// screen. The cross-platform modal stack is left untouched, so
		/// <c>Navigation.ModalStack</c> still contains the page and the next reconciliation pass retries
		/// the presentation.
		/// </para>
		/// <para>
		/// Where the fault surfaces depends on when the operation is applied. When
		/// <see cref="IsReady"/> was <see langword="true"/> at request time the push is applied inline
		/// and the fault is rethrown to the caller of <c>Navigation.PushModalAsync</c>. When the push was
		/// deferred because <see cref="IsReady"/> was <see langword="false"/>, that caller has already
		/// completed, so the fault is instead logged through the window's
		/// <see cref="Microsoft.Extensions.Logging.ILogger"/> and cannot be observed by navigation code.
		/// Backends that need deferred failures to be actionable should surface them themselves.
		/// </para>
		/// </remarks>
		Task PushModalAsync(Page modal, bool animated);

		/// <summary>
		/// Dismisses <paramref name="modal"/>, revealing
		/// <see cref="IModalNavigationHost.CurrentPlatformPage"/>.
		/// </summary>
		/// <param name="modal">The page to dismiss. It has already been removed from
		/// <see cref="IModalNavigationHost.PlatformModalStack"/> when this method is called, so
		/// <see cref="IModalNavigationHost.CurrentPlatformPage"/> already refers to the page that is
		/// about to be revealed.</param>
		/// <param name="animated"><see langword="true"/> to animate the transition. The framework preserves
		/// the value supplied to <c>Navigation.PopModalAsync</c>, including when the dismissal has to be
		/// deferred until <see cref="IsReady"/> becomes <see langword="true"/>. For a pop that comes from
		/// reconciling a stack the app changed wholesale (for example a <see cref="Shell"/> pop-to-root),
		/// the value recorded for the matching push is used.</param>
		/// <returns>A task that completes once the modal is off screen.</returns>
		/// <remarks>
		/// <para>
		/// Implementations are responsible for releasing the platform views created for
		/// <paramref name="modal"/>. The framework detaches the page from its parent after this task
		/// completes.
		/// </para>
		/// <para>
		/// This must be idempotent. It is called for a page the backend already dismissed natively when
		/// that dismissal is routed back through <c>Navigation.PopModalAsync</c>, and it must complete
		/// successfully rather than throw in that case.
		/// </para>
		/// <para>
		/// If this task faults the framework puts <paramref name="modal"/> back on
		/// <see cref="IModalNavigationHost.PlatformModalStack"/>, because a failed dismissal means the
		/// modal is presumed to still be on screen and dropping it would leave a visible modal that no
		/// stack knows about. The cross-platform modal stack is left without the page, so the next
		/// reconciliation pass retries the dismissal. As with
		/// <see cref="PushModalAsync(Page, bool)"/>, the fault is rethrown to the caller of
		/// <c>Navigation.PopModalAsync</c> only when the pop was applied inline; a deferred pop that
		/// faults is logged instead.
		/// </para>
		/// </remarks>
		Task PopModalAsync(Page modal, bool animated);

		/// <summary>
		/// Called when the window's page gets a handler, including when the page is replaced.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to attach platform hooks that depend on the window's content being realized, such as
		/// a hardware back button handler. This may be called multiple times for the same window. It is
		/// also called once immediately after the instance is created when the window's page already has
		/// a handler, so a backend that resolves late does not miss the notification.
		/// </para>
		/// <para>
		/// The framework guarantees it is never delivered twice for the same page-handler attachment.
		/// </para>
		/// </remarks>
		void PageAttached();
	}
}
