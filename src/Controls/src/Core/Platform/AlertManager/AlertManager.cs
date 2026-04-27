using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
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
				// try use services - an explicitly-registered subscription wins over everything else
				context.Services.GetService<IAlertManagerSubscription>() ??
				// then check for the delegate-based extensibility convention (see DelegateAlertSubscription)
				TryCreateDelegateSubscription(context) ??
				// finally fall back to the platform implementation (or a no-op on non-platforms)
				CreateSubscription(context);

			if (_subscription is null)
			{
				context.CreateLogger<AlertManager>()?.LogWarning("Warning - Unable to create alert manager subscription.");
			}
		}

		// Looks for per-operation dialog delegates registered in DI using ONLY already-public types:
		//   Func<Page, AlertArguments, Task>
		//   Func<Page, ActionSheetArguments, Task>
		//   Func<Page, PromptArguments, Task>
		// This lets third-party backends supply alert/dialog implementations without MAUI having to
		// expose IAlertManagerSubscription publicly. Any delegate that isn't registered falls through
		// to the platform default (so backends can override only what they care about).
		IAlertManagerSubscription? TryCreateDelegateSubscription(IMauiContext context)
		{
			var services = context.Services;

			var alertHandler = services.GetService<Func<Page, AlertArguments, Task>>();
			var actionSheetHandler = services.GetService<Func<Page, ActionSheetArguments, Task>>();
			var promptHandler = services.GetService<Func<Page, PromptArguments, Task>>();

			if (alertHandler is null && actionSheetHandler is null && promptHandler is null)
			{
				return null;
			}

			return new DelegateAlertSubscription(alertHandler, actionSheetHandler, promptHandler, () => CreateSubscription(context));
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
