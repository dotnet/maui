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
		}

		public bool IsInvokeRequired =>
			_isInvokeRequired?.Invoke() ?? false;

		public void BeginInvokeOnMainThread(Action action)
		{
			if (_invokeOnMainThread is null)
				action();
			else
				_invokeOnMainThread.Invoke(action);
		}
	}

	class DispatcherProviderStub : IDispatcherProvider
	{
		public IDispatcher? GetForCurrentThread() =>
			DispatcherProviderStubOptions.SkipDispatcherCreation
				? null
				: new DispatcherStub(
					DispatcherProviderStubOptions.IsInvokeRequired,
					DispatcherProviderStubOptions.InvokeOnMainThread);
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