using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Adapts a MAUI <see cref="IDispatcher"/> to the Blazor <see cref="Dispatcher"/> that a
	/// <see cref="WebViewManager"/> requires.
	/// </summary>
	/// <remarks>
	/// This is the dispatcher the built-in BlazorWebView handlers use. Handlers implementing
	/// <see cref="IBlazorWebViewHandler"/> for other platforms should reuse it rather than writing their own
	/// adapter, so that Blazor's thread affinity matches the rest of MAUI on that platform. Resolve the
	/// <see cref="IDispatcher"/> from the handler's services:
	/// <code>
	/// new MauiDispatcher(Services!.GetRequiredService&lt;IDispatcher&gt;())
	/// </code>
	/// </remarks>
	public sealed class MauiDispatcher : Dispatcher
	{
		readonly IDispatcher _dispatcher;

		/// <summary>
		/// Initializes a new instance of <see cref="MauiDispatcher"/> over the specified MAUI dispatcher.
		/// </summary>
		/// <param name="dispatcher">The MAUI <see cref="IDispatcher"/> to dispatch work through.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> is <see langword="null"/>.</exception>
		public MauiDispatcher(IDispatcher dispatcher)
		{
			ArgumentNullException.ThrowIfNull(dispatcher);

			_dispatcher = dispatcher;
		}

		/// <inheritdoc />
		public override bool CheckAccess()
		{
			return !_dispatcher.IsDispatchRequired;
		}

		/// <inheritdoc />
		// Await to normalize OperationCanceledException to canceled tasks, matching the Blazor dispatcher contract.
		public override async Task InvokeAsync(Action workItem)
		{
			await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public override async Task InvokeAsync(Func<Task> workItem)
		{
			await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public override async Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
		{
			return await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public override async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
		{
			return await _dispatcher.DispatchAsync(workItem).ConfigureAwait(false);
		}
	}
}
