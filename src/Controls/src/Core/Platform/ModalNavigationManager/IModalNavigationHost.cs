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
		/// Gets a value indicating whether the framework considers the window ready to present modals
		/// (the window and its page both have handlers, and <see cref="IModalNavigationPlatform.IsReady"/>
		/// returned <see langword="true"/>).
		/// </summary>
		bool IsModalReady { get; }

		/// <summary>
		/// Gets a value indicating whether several modals are being dismissed as a single batch, for
		/// example during a <see cref="Shell"/> pop-to-root. Implementations can use this to dismiss
		/// without animation or intermediate layout so that the modals in between do not flash on screen.
		/// </summary>
		bool IsBatchPopping { get; }

		/// <summary>
		/// Asks the framework to re-run the reconciliation loop that compares the requested modal stack
		/// with <see cref="PlatformModalStack"/> and issues any push or pop that is still outstanding.
		/// </summary>
		/// <remarks>
		/// Call this when <see cref="IModalNavigationPlatform.IsReady"/> transitions from
		/// <see langword="false"/> to <see langword="true"/>. The framework does not poll
		/// <see cref="IModalNavigationPlatform.IsReady"/>, so a platform that defers readiness must
		/// notify the framework through this method or queued modals will never be presented.
		/// The call is safe to make from any thread and does not block.
		/// </remarks>
		void RequestSync();
	}
}
