using System;
using System.Runtime.ExceptionServices;
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

		private static readonly Action<Exception> RethrowException = exception =>
			ExceptionDispatchInfo.Capture(exception).Throw();

		public override bool CheckAccess()
		{
			return !_dispatcher.IsDispatchRequired;
		}

		public override async Task InvokeAsync(Action workItem)
		{
			await _dispatcher.DispatchAsync(workItem);

			var current = System.AppDomain.CurrentDomain;

			//try
			//{
			//	if (CheckAccess())
			//	{
			//		workItem();
			//	}
			//	else
			//	{
			//		await _dispatcher.DispatchAsync(workItem);
			//	}
			//}
			//catch (Exception ex)
			//{
			//	_ = _dispatcher.DispatchAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
			//	throw;
			//}
		}

		public override async Task InvokeAsync(Func<Task> workItem)
		{
			try
			{
				if (CheckAccess())
				{
					await workItem();
				}
				else
				{
					await _dispatcher.DispatchAsync(workItem);
				}
			}
			catch (Exception ex)
			{
				// TODO: Determine whether this is the right kind of rethrowing pattern
				// You do have to do something like this otherwise unhandled exceptions
				// throw from inside Dispatcher.InvokeAsync are simply lost.
				_ = _dispatcher.DispatchAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
				throw;
			}
		}

		public override async Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
		{
			try
			{
				if (CheckAccess())
				{
					return workItem();
				}
				else
				{
					return await _dispatcher.DispatchAsync(workItem);
				}
			}
			catch (Exception ex)
			{
				// TODO: Determine whether this is the right kind of rethrowing pattern
				// You do have to do something like this otherwise unhandled exceptions
				// throw from inside Dispatcher.InvokeAsync are simply lost.
				_ = _dispatcher.DispatchAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
				throw;
			}
		}

		public override async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
		{
			try
			{
				if (CheckAccess())
				{
					return await workItem();
				}
				else
				{
					return await _dispatcher.DispatchAsync(workItem);
				}
			}
			catch (Exception ex)
			{
				// TODO: Determine whether this is the right kind of rethrowing pattern
				// You do have to do something like this otherwise unhandled exceptions
				// throw from inside Dispatcher.InvokeAsync are simply lost.
				_ = _dispatcher.DispatchAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
				throw;
			}
		}
	}
}
