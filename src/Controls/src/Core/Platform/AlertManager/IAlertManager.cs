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
		/// </summary>
		void Subscribe();

		/// <summary>
		/// Unsubscribes from alert requests.
		/// </summary>
		void Unsubscribe();

		/// <summary>
		/// Requests that an alert dialog be displayed.
		/// </summary>
		void RequestAlert(Page page, AlertArguments arguments);

		/// <summary>
		/// Requests that an action sheet be displayed.
		/// </summary>
		void RequestActionSheet(Page page, ActionSheetArguments arguments);

		/// <summary>
		/// Requests that a prompt dialog be displayed.
		/// </summary>
		void RequestPrompt(Page page, PromptArguments arguments);

		/// <summary>
		/// Requests that the page busy indicator be shown or hidden.
		/// </summary>
		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET 11.")]
		void RequestPageBusy(Page page, bool isBusy) { }
	}
}
