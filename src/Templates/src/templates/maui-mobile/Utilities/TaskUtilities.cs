using MauiApp._1.Services;

namespace MauiApp._1.Utilities;

/// <summary>
/// Task Utilities.
/// </summary>
public static class TaskUtilities
{
	/// <summary>
	/// Extensions for asynchronous tasks.
	/// </summary>
	/// <param name="task">Task to Fire and Forget.</param>
	extension(Task task)
	{
		/// <summary>
		/// Fire and Forget Safe Async.
		/// </summary>
		/// <param name="handler">Error Handler.</param>
		public async void FireAndForgetSafeAsync(IErrorHandler? handler = null)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				handler?.HandleError(ex);
			}
		}
	}
}