#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	// An IAlertManagerSubscription implementation that dispatches alert, action sheet, and prompt
	// requests to user-supplied delegates resolved from DI. If a delegate isn't supplied for a
	// particular operation, the call falls through to the platform default subscription, so backends
	// can override only the operations they care about.
	//
	// This is the internal backing for the "delegate convention" extensibility seam. Consumers
	// register any of the following in their MauiAppBuilder.Services (all types are already public):
	//
	//     services.AddSingleton<Func<Page, AlertArguments, Task>>(ShowAlertAsync);
	//     services.AddSingleton<Func<Page, ActionSheetArguments, Task>>(ShowActionSheetAsync);
	//     services.AddSingleton<Func<Page, PromptArguments, Task>>(ShowPromptAsync);
	//
	// The delegate is expected to complete the argument's TaskCompletionSource via SetResult(...).
	// If the delegate's returned Task faults or is cancelled, the exception / cancellation is
	// forwarded to the TaskCompletionSource so that the calling DisplayXyzAsync() call observes it.
	internal sealed class DelegateAlertSubscription : AlertManager.IAlertManagerSubscription
	{
		readonly Func<Page, AlertArguments, Task>? _alertHandler;
		readonly Func<Page, ActionSheetArguments, Task>? _actionSheetHandler;
		readonly Func<Page, PromptArguments, Task>? _promptHandler;
		readonly AlertManager.IAlertManagerSubscription _fallback;

		public DelegateAlertSubscription(
			Func<Page, AlertArguments, Task>? alertHandler,
			Func<Page, ActionSheetArguments, Task>? actionSheetHandler,
			Func<Page, PromptArguments, Task>? promptHandler,
			AlertManager.IAlertManagerSubscription fallback)
		{
			_alertHandler = alertHandler;
			_actionSheetHandler = actionSheetHandler;
			_promptHandler = promptHandler;
			_fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
		}

		public void OnAlertRequested(Page sender, AlertArguments arguments)
		{
			if (_alertHandler is null)
			{
				_fallback.OnAlertRequested(sender, arguments);
				return;
			}

			Invoke(() => _alertHandler(sender, arguments), arguments.Result);
		}

		public void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
		{
			if (_actionSheetHandler is null)
			{
				_fallback.OnActionSheetRequested(sender, arguments);
				return;
			}

			Invoke(() => _actionSheetHandler(sender, arguments), arguments.Result);
		}

		public void OnPromptRequested(Page sender, PromptArguments arguments)
		{
			if (_promptHandler is null)
			{
				_fallback.OnPromptRequested(sender, arguments);
				return;
			}

			Invoke(() => _promptHandler(sender, arguments), arguments.Result);
		}

		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET11.")]
		public void OnPageBusy(Page sender, bool enabled)
		{
			// OnPageBusy is obsolete and is not part of the delegate convention - always
			// route to the platform fallback so busy-indicator behavior is unchanged.
			_fallback.OnPageBusy(sender, enabled);
		}

		static void Invoke<T>(Func<Task> invoker, TaskCompletionSource<T> completion)
		{
			Task? task;
			try
			{
				task = invoker();
			}
			catch (Exception ex)
			{
				completion.TrySetException(ex);
				return;
			}

			if (task is null)
			{
				return;
			}

			if (task.IsCompleted)
			{
				ForwardFaultOrCancellation(task, completion);
				return;
			}

			task.ContinueWith(
				static (t, state) => ForwardFaultOrCancellation(t, (TaskCompletionSource<T>)state!),
				completion,
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
		}

		static void ForwardFaultOrCancellation<T>(Task task, TaskCompletionSource<T> completion)
		{
			// The delegate is responsible for calling SetResult(...) on successful completion.
			// Only forward faults and cancellations so we don't overwrite the delegate's result.
			if (task.IsFaulted && task.Exception is not null)
			{
				completion.TrySetException(task.Exception.InnerExceptions);
			}
			else if (task.IsCanceled)
			{
				completion.TrySetCanceled();
			}
		}
	}
}
