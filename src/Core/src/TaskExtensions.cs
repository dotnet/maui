#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
				await task;
			}
			catch (Exception exc)
			{
				errorCallback?.Invoke(exc);
			}
			finally
			{
				finishedCallBack?.Invoke();
			}
		}
	}
}
