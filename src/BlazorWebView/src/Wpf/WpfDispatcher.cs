// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using WindowsDispatcher = System.Windows.Threading.Dispatcher;

namespace Microsoft.AspNetCore.Components.WebView.Wpf
{
	internal sealed class WpfDispatcher : Dispatcher
	{
		private readonly WindowsDispatcher _windowsDispatcher;

		public WpfDispatcher(WindowsDispatcher windowsDispatcher)
		{
			_windowsDispatcher = windowsDispatcher ?? throw new ArgumentNullException(nameof(windowsDispatcher));
		}

		public override bool CheckAccess()
			=> _windowsDispatcher.CheckAccess();

		public override Task InvokeAsync(Action workItem) =>
			InvokeAsyncCore(() =>
			{
				workItem();
				return Task.FromResult(true);
			});

		public override Task InvokeAsync(Func<Task> workItem) =>
			InvokeAsyncCore(async () =>
			{
				await workItem().ConfigureAwait(false);
				return true;
			});

		public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem) =>
			InvokeAsyncCore(() =>
			{
				var result = workItem();
				return Task.FromResult(result);
			});

		public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem) =>
			InvokeAsyncCore(workItem);

		private async Task<TResult> InvokeAsyncCore<TResult>(Func<Task<TResult>> workItem)
		{
			if (CheckAccess())
			{
				return await workItem().ConfigureAwait(false);
			}

			var completion = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			// WPF dispatches an Action, so capture the asynchronous result and all outcomes in the returned task.
			Action action = async () =>
			{
				try
				{
					completion.TrySetResult(await workItem().ConfigureAwait(false));
				}
				catch (Exception ex)
				{
					// Preserve the exception here; awaiting completion below lets the outer async method
					// classify an OperationCanceledException as cancellation without replacing its instance.
					completion.TrySetException(ex);
				}
			};

			await _windowsDispatcher.InvokeAsync(action).Task.ConfigureAwait(false);
			return await completion.Task.ConfigureAwait(false);
		}
	}
}
