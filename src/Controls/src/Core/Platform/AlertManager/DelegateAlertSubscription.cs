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
		readonly Lazy<AlertManager.IAlertManagerSubscription> _fallback;

		public DelegateAlertSubscription(
			Func<Page, AlertArguments, Task>? alertHandler,
			Func<Page, ActionSheetArguments, Task>? actionSheetHandler,
			Func<Page, PromptArguments, Task>? promptHandler,
			Func<AlertManager.IAlertManagerSubscription> createFallback)
		{
			_alertHandler = alertHandler;
			_actionSheetHandler = actionSheetHandler;
			_promptHandler = promptHandler;
			_fallback = new Lazy<AlertManager.IAlertManagerSubscription>(
				createFallback ?? throw new ArgumentNullException(nameof(createFallback)));
		}

		public void OnAlertRequested(Page sender, AlertArguments arguments)
		{
			if (_alertHandler is null)
			{
				_fallback.Value.OnAlertRequested(sender, arguments);
				return;
			}

			Invoke(() => _alertHandler(sender, arguments), arguments.Result);
		}

		public void OnActionSheetRequested(Page sender, ActionSheetArguments arguments)
		{
			if (_actionSheetHandler is null)
			{
				_fallback.Value.OnActionSheetRequested(sender, arguments);
				return;
			}

			Invoke(() => _actionSheetHandler(sender, arguments), arguments.Result);
		}

		public void OnPromptRequested(Page sender, PromptArguments arguments)
		{
			if (_promptHandler is null)
			{
				_fallback.Value.OnPromptRequested(sender, arguments);
				return;
			}

			Invoke(() => _promptHandler(sender, arguments), arguments.Result);
		}

		[Obsolete("This method is obsolete in .NET 10 and will be removed in .NET11.")]
		public void OnPageBusy(Page sender, bool enabled)
		{
			// OnPageBusy is obsolete and is not part of the delegate convention - always
			// route to the platform fallback so busy-indicator behavior is unchanged.
			_fallback.Value.OnPageBusy(sender, enabled);
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
				completion.TrySetException(new InvalidOperationException(
					"Alert delegate returned a null Task. The delegate must return a non-null Task and call SetResult(...) on the provided arguments."));
				return;
			}

			if (task.IsCompleted)
			{
				ForwardCompletion(task, completion);
				return;
			}

			task.ContinueWith(
				static (t, state) => ForwardCompletion(t, (TaskCompletionSource<T>)state!),
				completion,
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
		}

		static void ForwardCompletion<T>(Task task, TaskCompletionSource<T> completion)
		{
			// The delegate is responsible for calling SetResult(...) on successful completion.
			// Forward faults and cancellations so the caller observes them. If the delegate's
			// task completed successfully but never called SetResult, surface that as an
			// InvalidOperationException instead of letting the caller hang forever.
			// All paths use Try* so a delegate that did call SetResult is unaffected.
			if (task.IsFaulted)
			{
				completion.TrySetException(task.Exception!.InnerExceptions);
			}
			else if (task.IsCanceled)
			{
				completion.TrySetCanceled();
			}
			else if (!completion.Task.IsCompleted)
			{
				completion.TrySetException(new InvalidOperationException(
					"Alert delegate completed without calling SetResult(...) on the provided arguments. The delegate must call SetResult before its returned Task completes."));
			}
		}
	}
}
