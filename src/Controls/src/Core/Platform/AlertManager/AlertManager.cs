using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO: Make this public in .NET 11
	/// <summary>
	/// Manages alert, action sheet, and prompt requests for a window.
	/// </summary>
	internal interface IAlertManager
	{
		/// <summary>
		/// Gets the window associated with this alert manager.
		/// </summary>
		Window Window { get; }

		/// <summary>
		/// Subscribes the alert manager to handle alert requests.
		/// </summary>
		void Subscribe();

		/// <summary>
		/// Unsubscribes the alert manager from handling alert requests.
		/// </summary>
		void Unsubscribe();

		/// <summary>
		/// Requests an action sheet to be displayed.
		/// </summary>
		/// <param name="page">The page making the request.</param>
		/// <param name="arguments">The action sheet arguments.</param>
		void RequestActionSheet(Page page, ActionSheetArguments arguments);

		/// <summary>
		/// Requests an alert to be displayed.
		/// </summary>
		/// <param name="page">The page making the request.</param>
		/// <param name="arguments">The alert arguments.</param>
		void RequestAlert(Page page, AlertArguments arguments);

		/// <summary>
		/// Requests a prompt to be displayed.
		/// </summary>
		/// <param name="page">The page making the request.</param>
		/// <param name="arguments">The prompt arguments.</param>
		void RequestPrompt(Page page, PromptArguments arguments);

		/// <summary>
		/// Requests that a page's busy state be displayed.
		/// </summary>
		/// <param name="page">The page making the request.</param>
		/// <param name="isBusy">Whether the page is busy.</param>
		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET11.")]
		void RequestPageBusy(Page page, bool isBusy);
	}

	internal partial class AlertManager : IAlertManager
	{
		readonly Window _window;

		IAlertManagerSubscription? _subscription;

		public AlertManager(Window window)
		{
			_window = window;
		}

		public Window Window => _window;

		public IAlertManagerSubscription? Subscription => _subscription;

		public void Subscribe()
		{
			var context = _window.Handler?.MauiContext;

			if (context is null)
			{
				return;
			}

			if (_subscription is not null)
			{
				context.CreateLogger<AlertManager>()?.LogWarning("Warning - Window already had an alert manager subscription, but a new one was requested. Not going to do anything.");
				return;
			}

			_subscription =
				// try use services
				context.Services.GetService<IAlertManagerSubscription>() ??
				// fall back to the platform implementation and a "null implementation" on non-platforms
				CreateSubscription(context);

			if (_subscription is null)
			{
				context.CreateLogger<AlertManager>()?.LogWarning("Warning - Unable to create alert manager subscription.");
			}
		}

		public void Unsubscribe() =>
			_subscription = null;

		public void RequestActionSheet(Page page, ActionSheetArguments arguments) =>
			_subscription?.OnActionSheetRequested(page, arguments);

		public void RequestAlert(Page page, AlertArguments arguments) =>
			_subscription?.OnAlertRequested(page, arguments);

		public void RequestPrompt(Page page, PromptArguments arguments) =>
			_subscription?.OnPromptRequested(page, arguments);

		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET11.")]
		public void RequestPageBusy(Page page, bool isBusy) =>
			_subscription?.OnPageBusy(page, isBusy);

		private partial IAlertManagerSubscription CreateSubscription(IMauiContext mauiContext);

		internal interface IAlertManagerSubscription
		{
			void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);

			void OnAlertRequested(Page sender, AlertArguments arguments);

			void OnPromptRequested(Page sender, PromptArguments arguments);

			[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET11.")]
			void OnPageBusy(Page sender, bool enabled);
		}

		internal partial class AlertRequestHelper : IAlertManagerSubscription
		{
			public partial void OnActionSheetRequested(Page sender, ActionSheetArguments arguments);

			public partial void OnAlertRequested(Page sender, AlertArguments arguments);

			public partial void OnPromptRequested(Page sender, PromptArguments arguments);

			[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET11.")]
			public partial void OnPageBusy(Page sender, bool enabled);
		}
	}
}
