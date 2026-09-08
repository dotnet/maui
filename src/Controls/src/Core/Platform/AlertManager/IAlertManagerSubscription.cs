using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Provides platform-specific implementations for alert, action sheet, and prompt dialogs.
	/// Custom platform backends can implement this interface and register it via dependency injection
	/// to provide custom dialog implementations.
	/// <para>
	/// Note: This interface is used by the default <c>AlertManager</c> implementation. If a custom
	/// <see cref="IAlertManager"/> is registered via dependency injection, this interface will not
	/// be resolved by the framework — the custom <see cref="IAlertManager"/> is responsible for
	/// its own subscription management.
	/// </para>
	/// </summary>
	internal interface IAlertManagerSubscription
	{
		/// <summary>
		/// Called when an action sheet is requested.
		/// <para>
		/// Note: <see cref="ActionSheetArguments"/> is part of the stable public API contract for this interface,
		/// despite residing in the <c>Microsoft.Maui.Controls.Internals</c> namespace.
		/// </para>
		/// </summary>
		void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);

		/// <summary>
		/// Called when an alert dialog is requested.
		/// <para>
		/// Note: <see cref="AlertArguments"/> is part of the stable public API contract for this interface,
		/// despite residing in the <c>Microsoft.Maui.Controls.Internals</c> namespace.
		/// </para>
		/// </summary>
		void OnAlertRequested(Page sender, AlertArguments arguments);

		/// <summary>
		/// Called when a prompt dialog is requested.
		/// <para>
		/// Note: <see cref="PromptArguments"/> is part of the stable public API contract for this interface,
		/// despite residing in the <c>Microsoft.Maui.Controls.Internals</c> namespace.
		/// </para>
		/// </summary>
		void OnPromptRequested(Page sender, PromptArguments arguments);

		/// <summary>
		/// Called when the page busy state changes.
		/// Implementers may provide an empty body for this method; it will be removed in .NET 11.
		/// </summary>
		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET 11.")]
#if !NETSTANDARD2_0
		void OnPageBusy(Page sender, bool enabled) { }
#else
		void OnPageBusy(Page sender, bool enabled);
#endif
	}
}
