using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Provides an interface for managing alert, action sheet, and prompt dialogs.
	/// Custom platform backends can implement this interface and register it via dependency injection
	/// to replace the default alert management behavior.
	/// </summary>
	public interface IAlertManager
	{
		/// <summary>
		/// Subscribes to alert requests from the associated window's pages.
		/// Implementations must be idempotent: calling <see cref="Subscribe"/> when already
		/// subscribed must be a safe no-op, as the framework may call it without a preceding
		/// <see cref="Unsubscribe"/> when a page with an already-attached handler is assigned
		/// to the window.
		/// </summary>
		void Subscribe();

		/// <summary>
		/// Unsubscribes from alert requests.
		/// </summary>
		void Unsubscribe();

		/// <summary>
		/// Requests that an alert dialog be displayed.
		/// <para>
		/// Note: <see cref="AlertArguments"/> is part of the stable public API contract for this interface,
		/// despite residing in the <c>Microsoft.Maui.Controls.Internals</c> namespace.
		/// </para>
		/// </summary>
		void RequestAlert(Page page, AlertArguments arguments);

		/// <summary>
		/// Requests that an action sheet be displayed.
		/// <para>
		/// Note: <see cref="ActionSheetArguments"/> is part of the stable public API contract for this interface,
		/// despite residing in the <c>Microsoft.Maui.Controls.Internals</c> namespace.
		/// </para>
		/// </summary>
		void RequestActionSheet(Page page, ActionSheetArguments arguments);

		/// <summary>
		/// Requests that a prompt dialog be displayed.
		/// <para>
		/// Note: <see cref="PromptArguments"/> is part of the stable public API contract for this interface,
		/// despite residing in the <c>Microsoft.Maui.Controls.Internals</c> namespace.
		/// </para>
		/// </summary>
		void RequestPrompt(Page page, PromptArguments arguments);

		/// <summary>
		/// Requests that the page busy indicator be shown or hidden.
		/// Implementers may provide an empty body for this method; it will be removed in .NET 11.
		/// </summary>
		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET 11.")]
#if !NETSTANDARD2_0
		void RequestPageBusy(Page page, bool isBusy) { }
#else
		void RequestPageBusy(Page page, bool isBusy);
#endif
	}
}
