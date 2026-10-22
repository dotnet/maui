using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Components.WebView;

internal static class WebViewTaskExtensions
{
	public static async Task ObserveExceptionsAsync(
		this Task task,
		ILogger logger,
		[CallerMemberName] string? callerName = null)
	{
		try
		{
			await task.ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (task.IsCanceled)
		{
			// Cancellation is an expected completion state for discarded dispatcher work.
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected exception in {Member}.", callerName);
		}
	}
}
