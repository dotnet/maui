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
	//     services.AddKeyedSingleton<Func<Page, AlertArguments, Task<bool>>>(
	//         AlertManager.DisplayAlertServiceKey, ShowAlertAsync);
	//     services.AddKeyedSingleton<Func<Page, ActionSheetArguments, Task<string>>>(
	//         AlertManager.DisplayActionSheetServiceKey, ShowActionSheetAsync);
	//     services.AddKeyedSingleton<Func<Page, PromptArguments, Task<string>>>(
	//         AlertManager.DisplayPromptServiceKey, ShowPromptAsync);
	//
	// The delegate returns the dialog result. This wrapper completes the argument's
	// TaskCompletionSource so that the calling DisplayXyzAsync() call observes the result,
	// exception, or cancellation.
	internal sealed class DelegateAlertSubscription : AlertManager.IAlertManagerSubscription
	{
		readonly Func<Page, AlertArguments, Task<bool>>? _alertHandler;
		readonly Func<Page, ActionSheetArguments, Task<string>>? _actionSheetHandler;
		readonly Func<Page, PromptArguments, Task<string>>? _promptHandler;
		readonly Lazy<AlertManager.IAlertManagerSubscription> _fallback;

		public DelegateAlertSubscription(
			Func<Page, AlertArguments, Task<bool>>? alertHandler,
			Func<Page, ActionSheetArguments, Task<string>>? actionSheetHandler,
			Func<Page, PromptArguments, Task<string>>? promptHandler,
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

		static void Invoke<T>(Func<Task<T>> invoker, TaskCompletionSource<T> completion)
		{
			Task<T>? task;
			try
			{
				task = invoker();
			}
			catch (OperationCanceledException ex)
			{
				completion.TrySetCanceled(ex.CancellationToken);
				return;
			}
			catch (Exception ex)
			{
				completion.TrySetException(ex);
				return;
			}

			if (task is null)
			{
				completion.TrySetException(new InvalidOperationException(
					"Dialog delegate returned a null Task. The delegate must return a non-null Task containing the dialog result."));
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

		static void ForwardCompletion<T>(Task<T> task, TaskCompletionSource<T> completion)
		{
			if (task.IsFaulted)
			{
				completion.TrySetException(task.Exception!.InnerExceptions);
			}
			else if (task.IsCanceled)
			{
				try
				{
					task.GetAwaiter().GetResult();
				}
				catch (OperationCanceledException ex)
				{
					completion.TrySetCanceled(ex.CancellationToken);
					return;
				}

				completion.TrySetCanceled();
			}
			else
			{
				completion.TrySetResult(task.Result);
			}
		}
	}
}
