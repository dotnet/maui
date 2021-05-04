#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public static class TaskExtensions
	{
		public static async void FireAndForget(
			this Task task,
			Action<Exception>? errorCallback = null,
			Action? finishedCallBack = null
			)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				errorCallback?.Invoke(ex);
			}
			finally
			{
				finishedCallBack?.Invoke();
			}
		}

		public static void FireAndForget(this Task task, ILogger? logger, [CallerMemberName] string? callerName = null) =>
			task.FireAndForget(ex => Log(logger, ex, callerName));

		public static void FireAndForget<T>(this Task task, T? viewHandler, [CallerMemberName] string? callerName = null)
			where T : IViewHandler
		{
			task.FireAndForget(ex => Log(viewHandler?.CreateLogger<T>(), ex, callerName));
		}

		static ILogger? CreateLogger<T>(this IViewHandler? viewHandler) =>
			viewHandler?.MauiContext?.Services?.CreateLogger<T>();

		static void Log(ILogger? logger, Exception ex, string? callerName) =>
			logger?.LogError(ex, "Unexpected exception in {Member}.", callerName);
	}
}
