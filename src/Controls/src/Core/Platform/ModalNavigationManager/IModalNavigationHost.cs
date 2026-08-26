#nullable enable

using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Exposes the cross-platform modal navigation state that a <see cref="IModalNavigationPlatform"/>
	/// needs in order to present and dismiss modal pages.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The framework implements this interface and supplies an instance to
	/// <see cref="IModalNavigationPlatformFactory.CreateModalNavigationPlatform(IModalNavigationHost)"/>.
	/// One host exists per <see cref="Controls.Window"/>, so an implementation must never be shared
	/// between windows.
	/// </para>
	/// <para>
	/// The framework owns the cross-platform modal stack, the page lifecycle events
	/// (<c>Appearing</c>/<c>Disappearing</c>, <c>NavigatedTo</c>/<c>NavigatedFrom</c>), the
	/// <see cref="Controls.Window.ModalPushing"/>/<see cref="Controls.Window.ModalPopped"/> events and the
	/// reconciliation loop that keeps the platform stack in sync with the requested stack. A platform
	/// implementation is only responsible for the visual presentation of a single push or pop.
	/// </para>
	/// <para>
	/// Every member except <see cref="RequestSync"/> must be read on the UI thread.
	/// </para>
	/// </remarks>
	public interface IModalNavigationHost
	{
		/// <summary>
		/// Gets the window that owns this modal navigation host.
		/// </summary>
		Window Window { get; }

		/// <summary>
		/// Gets the <see cref="IMauiContext"/> scoped to <see cref="Window"/>.
		/// </summary>
		/// <remarks>
		/// This context is tied to the window's current handler. It is not guaranteed to be usable once
		/// the window has been torn down, so do not read it from
		/// <see cref="System.IDisposable.Dispose"/>. Capture whatever cleanup needs while the platform
		/// instance is still live.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// Thrown when the window does not currently have a handler and therefore has no context.
		/// </exception>
		IMauiContext MauiContext { get; }

		/// <summary>
		/// Gets the modal pages that the platform has actually presented, in push order.
		/// The last entry is the modal currently on screen.
		/// </summary>
		/// <remarks>
		/// This is maintained by the framework. It is updated before
		/// <see cref="IModalNavigationPlatform.PushModalAsync(Page, bool)"/> is awaited and before
		/// <see cref="IModalNavigationPlatform.PopModalAsync(Page, bool)"/> is awaited, so during those
		/// calls it already reflects the requested end state.
		/// </remarks>
		IReadOnlyList<Page> PlatformModalStack { get; }

		/// <summary>
		/// Gets the page the user is expected to be looking at once the cross-platform modal stack is
		/// fully applied. This is the topmost requested modal, or the window's page when no modal is
		/// requested, with <see cref="Shell"/> unwrapped to its current page.
		/// </summary>
		Page? CurrentPage { get; }

		/// <summary>
		/// Gets the page that is currently hosting content on the platform: the topmost entry of
		/// <see cref="PlatformModalStack"/>, or the window's page when no modal has been presented.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">
		/// Thrown when there is no presented modal and the window has no page.
		/// </exception>
		Page CurrentPlatformPage { get; }

		/// <summary>
		/// Gets a value indicating whether the framework side of the window is ready for modal
		/// presentation: the window and its page both have handlers.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This intentionally does <b>not</b> include
		/// <see cref="IModalNavigationPlatform.IsReady"/>. Implementations are expected to combine it
		/// with their own conditions, for example
		/// <c>public bool IsReady =&gt; _host.IsWindowReady &amp;&amp; _nativeWindowIsRealized;</c>.
		/// Because the framework folds <see cref="IModalNavigationPlatform.IsReady"/> into its own
		/// overall readiness separately, a host property that already included it would recurse into an
		/// uncatchable <see cref="System.StackOverflowException"/>.
		/// </para>
		/// <para>
		/// Consulting this from <see cref="IModalNavigationPlatform.IsReady"/> is optional. The framework
		/// never calls <see cref="IModalNavigationPlatform.PushModalAsync(Page, bool)"/> or
		/// <see cref="IModalNavigationPlatform.PopModalAsync(Page, bool)"/> unless this is already
		/// <see langword="true"/>.
		/// </para>
		/// </remarks>
		bool IsWindowReady { get; }

		/// <summary>
		/// Gets a hint indicating that several modals are being dismissed as a single batch, for example
		/// during a <see cref="Shell"/> pop-to-root.
		/// </summary>
		/// <remarks>
		/// This is an optional optimization hint, not a contract. Implementations can use it to dismiss
		/// without animation so the modals in between do not flash on screen. It is only ever
		/// <see langword="true"/> while <see cref="Controls.Window.Page"/> is a <see cref="Shell"/> that
		/// is popping its modal stack; other batch dismissals report <see langword="false"/>. An
		/// implementation that ignores it stays correct.
		/// </remarks>
		bool IsBatchPopping { get; }

		/// <summary>
		/// Gets a hint indicating that several modals are being presented as a single batch, for example
		/// while a <see cref="Shell"/> applies a navigation state that contains multiple modals.
		/// </summary>
		/// <remarks>
		/// The counterpart to <see cref="IsBatchPopping"/>, with the same caveats: it is an optional
		/// optimization hint that is only ever <see langword="true"/> for <see cref="Shell"/>, and an
		/// implementation that ignores it stays correct.
		/// </remarks>
		bool IsBatchPushing { get; }

		/// <summary>
		/// Asks the framework to re-run the reconciliation loop that compares the requested modal stack
		/// with <see cref="PlatformModalStack"/> and issues any push or pop that is still outstanding.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Call this when <see cref="IModalNavigationPlatform.IsReady"/> transitions from
		/// <see langword="false"/> to <see langword="true"/>. The framework does not poll
		/// <see cref="IModalNavigationPlatform.IsReady"/>, so a platform that defers readiness must
		/// notify the framework through this method or queued modals will never be presented.
		/// </para>
		/// <para>
		/// This is safe to call from any thread: when the caller is not already on the window's UI
		/// thread the work is marshalled there and this method returns immediately. When called
		/// <b>on</b> the UI thread the reconciliation starts synchronously, which means
		/// <see cref="IModalNavigationPlatform.IsReady"/> and possibly
		/// <see cref="IModalNavigationPlatform.PushModalAsync(Page, bool)"/> or
		/// <see cref="IModalNavigationPlatform.PopModalAsync(Page, bool)"/> can be re-entered before
		/// this method returns. Do not call it while holding a lock, and finish mutating your own state
		/// before calling it.
		/// </para>
		/// <para>
		/// A marshalled request is bound to the window's current handler scope. If the window is
		/// destroyed, or its handler is replaced, before the request runs, the request is dropped rather
		/// than executed against a torn-down window or a service scope that no longer exists. Calling
		/// this after disposal is therefore harmless but has no effect.
		/// </para>
		/// </remarks>
		void RequestSync();
	}
}
