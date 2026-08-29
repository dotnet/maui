using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Components.WebView;

internal static class TaskExtensions
{
	public static async void FireAndForget(
		this Task task,
		ILogger logger,
		[CallerMemberName] string? callerName = null)
	{
		try
		{
			await task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected exception in {Member}.", callerName);
		}
	}
}
