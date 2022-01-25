#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.UnitTests
{
	class DispatcherStub : IDispatcher
	{
		readonly Func<bool>? _isInvokeRequired;
		readonly Action<Action>? _invokeOnMainThread;

		public DispatcherStub(Func<bool>? isInvokeRequired, Action<Action>? invokeOnMainThread)
		{
			_isInvokeRequired = isInvokeRequired;
			_invokeOnMainThread = invokeOnMainThread;

			ManagedThreadId = Environment.CurrentManagedThreadId;
		}

		public bool IsDispatchRequired =>
			_isInvokeRequired?.Invoke() ?? false;

		public int ManagedThreadId { get; }

		public bool Dispatch(Action action)
		{
			if (_invokeOnMainThread is null)
				action();
			else
				_invokeOnMainThread.Invoke(action);
			return true;
		}
	}

	class DispatcherProviderStub : IDispatcherProvider, IDisposable
	{
		ThreadLocal<IDispatcher?> s_dispatcherInstance = new(() =>
			DispatcherProviderStubOptions.SkipDispatcherCreation
				? null
				: new DispatcherStub(
					DispatcherProviderStubOptions.IsInvokeRequired,
					DispatcherProviderStubOptions.InvokeOnMainThread));

		public void Dispose() =>
			s_dispatcherInstance.Dispose();

		public IDispatcher? GetForCurrentThread() =>
			s_dispatcherInstance.Value;
	}

	public class DispatcherProviderStubOptions
	{
		[ThreadStatic]
		public static bool SkipDispatcherCreation;

		[ThreadStatic]
		public static Func<bool>? IsInvokeRequired;

		[ThreadStatic]
		public static Action<Action>? InvokeOnMainThread;
	}

	public static class DispatcherTest
	{
		public static Task Run(Action testAction) =>
			Run(() =>
			{
				testAction();
				return Task.CompletedTask;
			});

		public static Task Run(Func<Task> testAction)
		{
			var tcs = new TaskCompletionSource();

			var thread = new Thread(async () =>
			{
				try
				{
					await testAction();

					tcs.SetResult();
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			thread.Start();

			return tcs.Task;
		}
	}
}