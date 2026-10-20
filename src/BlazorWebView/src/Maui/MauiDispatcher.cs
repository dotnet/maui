using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal sealed class MauiDispatcher : Dispatcher
	{
		readonly IDispatcher _dispatcher;

		public MauiDispatcher(IDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public override bool CheckAccess()
		{
			return !_dispatcher.IsDispatchRequired;
		}

		// Await to normalize OperationCanceledException to canceled tasks, matching the Blazor dispatcher contract.
		public override async Task InvokeAsync(Action workItem)
		{
			await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}

		public override async Task InvokeAsync(Func<Task> workItem)
		{
			await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}

		public override async Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
		{
			return await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}

		public override async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
		{
			return await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}
	}
}
