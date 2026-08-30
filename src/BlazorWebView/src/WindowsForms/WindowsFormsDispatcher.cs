// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
{
	/// <summary>
	/// Dispatcher implementation for Windows Forms that invokes methods on the UI thread. The <see cref="Dispatcher"/>
	/// class uses the async <see cref="Task"/> pattern so everything must be mapped from the <see cref="IAsyncResult"/>
	/// pattern using techniques listed in https://docs.microsoft.com/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types.
	/// </summary>
	internal sealed class WindowsFormsDispatcher : Dispatcher
	{
		private readonly Control _dispatchThreadControl;

		/// <summary>
		/// Creates a new instance of <see cref="WindowsFormsDispatcher"/>.
		/// </summary>
		/// <param name="dispatchThreadControl">A control that was created on the thread from which UI dispatches must
		/// occur. This can typically be any control because all controls must have been created on the UI thread to
		/// begin with.</param>
		public WindowsFormsDispatcher(Control dispatchThreadControl)
		{
			if (dispatchThreadControl is null)
			{
				throw new ArgumentNullException(nameof(dispatchThreadControl));
			}

			_dispatchThreadControl = dispatchThreadControl;
		}

		public override bool CheckAccess()
			=> !_dispatchThreadControl.InvokeRequired;

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
			// BeginInvoke accepts an Action, so capture the asynchronous result and all outcomes in the returned task.
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

			var asyncResult = _dispatchThreadControl.BeginInvoke(action);
			await Task.Factory.FromAsync(asyncResult, _dispatchThreadControl.EndInvoke).ConfigureAwait(false);
			return await completion.Task.ConfigureAwait(false);
		}
	}
}
