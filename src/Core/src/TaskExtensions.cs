#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	internal static class TaskExtensions
	{
		public static async void FireAndForget<TResult>(
			   this Task<TResult> task,
			   Action<Exception>? errorCallback = null)
		{
			TResult? result = default;
			try
			{
				result = await task.ConfigureAwait(false);
			}
			catch (Exception exc)
			{
				errorCallback?.Invoke(exc);
#if DEBUG
				throw;
#endif
			}
		}

		public static async void FireAndForget(
			this Task task,
			Action<Exception>? errorCallback = null
			)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				errorCallback?.Invoke(ex);
#if DEBUG
				throw;
#endif
			}
		}

		public static void FireAndForget(this Task task, ILogger? logger, [CallerMemberName] string? callerName = null) =>
			task.FireAndForget(ex => Log(logger, ex, callerName));

#if !WEBVIEW2_MAUI
		public static void FireAndForget<T>(this Task task, T? viewHandler, [CallerMemberName] string? callerName = null)
			where T : IElementHandler
		{
			task.FireAndForget(ex => Log(viewHandler?.CreateLogger<T>(), ex, callerName));
		}

		static ILogger? CreateLogger<T>(this IElementHandler? elementHandler) =>
			elementHandler?.MauiContext?.Services?.CreateLogger<T>();
#endif

		static void Log(ILogger? logger, Exception ex, string? callerName) =>
			logger?.LogError(ex, "Unexpected exception in {Member}.", callerName);

		public static async void RunAndReport<T>(this TaskCompletionSource<T> request, Task<T> task)
		{
			try
			{
				var result = await task.ConfigureAwait(false);
				request.SetResult(result);
			}
			catch (Exception ex)
			{
				request.SetException(ex);
			}
		}

#if WINDOWS
		public static async void RunAndReport<T>(this TaskCompletionSource<T> request, global::Windows.Foundation.IAsyncOperation<T> task)
		{
			try
			{
				var result = await task;
				request.SetResult(result);
			}
			catch (Exception ex)
			{
				request.SetException(ex);
			}
		}
#endif
	}
}
