using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Provides platform-specific implementations for alert, action sheet, and prompt dialogs.
	/// Custom platform backends can implement this interface and register it via dependency injection
	/// to provide custom dialog implementations.
	/// </summary>
	public interface IAlertManagerSubscription
	{
		/// <summary>
		/// Called when an action sheet is requested.
		/// </summary>
		void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);

		/// <summary>
		/// Called when an alert dialog is requested.
		/// </summary>
		void OnAlertRequested(Page sender, AlertArguments arguments);

		/// <summary>
		/// Called when a prompt dialog is requested.
		/// </summary>
		void OnPromptRequested(Page sender, PromptArguments arguments);

		/// <summary>
		/// Called when the page busy state changes.
		/// </summary>
		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET 11.")]
		void OnPageBusy(Page sender, bool enabled) { }
	}
}
