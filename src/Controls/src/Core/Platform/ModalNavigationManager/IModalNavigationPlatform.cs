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
	/// </remarks>
	public interface IModalNavigationPlatform : IDisposable
	{
		/// <summary>
		/// Gets a value indicating whether the backend can present or dismiss a modal right now.
		/// </summary>
		/// <remarks>
		/// While this returns <see langword="false"/> the framework still records pushes and pops on the
		/// cross-platform modal stack and still raises the corresponding page lifecycle events, but it
		/// does not call <see cref="PushModalAsync(Page, bool)"/> or <see cref="PopModalAsync(Page, bool)"/>.
		/// When the backend becomes ready it must call
		/// <see cref="IModalNavigationHost.RequestSync"/> so the queued operations are applied.
		/// Return <see langword="true"/> unconditionally if the backend has no readiness requirement.
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
		/// If this task faults, the framework removes <paramref name="modal"/> from
		/// <see cref="IModalNavigationHost.PlatformModalStack"/> again and rethrows to the caller of
		/// <c>PushModalAsync</c>.
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
		/// <param name="animated"><see langword="true"/> to animate the transition. The framework passes
		/// the value that was supplied to the matching push when the pop is the result of stack
		/// reconciliation, and the value supplied to <c>PopModalAsync</c> otherwise.</param>
		/// <returns>A task that completes once the modal is off screen.</returns>
		/// <remarks>
		/// Implementations are responsible for releasing the platform views created for
		/// <paramref name="modal"/>. The framework detaches the page from its parent after this task
		/// completes.
		/// </remarks>
		Task PopModalAsync(Page modal, bool animated);

		/// <summary>
		/// Called when the window's page gets a handler, including when the page is replaced.
		/// </summary>
		/// <remarks>
		/// Use this to attach platform hooks that depend on the window's content being realized, such as
		/// a hardware back button handler. This may be called multiple times for the same window.
		/// </remarks>
		void PageAttached();
	}
}
