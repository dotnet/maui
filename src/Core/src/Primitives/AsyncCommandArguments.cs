//using System;
//using System.Threading.Tasks;

//namespace Microsoft.Maui
//{
//	/// <summary>
//	/// Arguments for an asynchronous handler command which returns a value.
//	/// </summary>
//	/// <typeparam name="T">The type of the value to be returned.</typeparam>
//	/// <remarks>
//	/// AsyncCommandArguments are passed to a mapped handler command via the <see cref="Microsoft.Maui.IElementHandler.Invoke(string, object?)" /> method.
//	/// The mapped handler command is responsible for calling the <see cref="SetResult"/> method to 
//	/// signal to the caller that the command has completed. 
//	/// </remarks>
//	public class AsyncCommandArguments<T> 
//	{
//		readonly TaskCompletionSource<T> _taskCompletionSource;

//		/// <summary>
//		/// Called by the handler to set the result when available.
//		/// </summary>
//		public Action<T> SetResult { get; }

//		/// <summary>
//		/// The task to be awaited by the cross-platform control invoking the command.
//		/// </summary>
//		public Task<T> Result => _taskCompletionSource.Task;

//		public AsyncCommandArguments() 
//		{
//			_taskCompletionSource = new TaskCompletionSource<T>();
//			SetResult = _taskCompletionSource.SetResult;
//		}
//	}

//	/// <summary>
//	/// Arguments for an asynchronous handler command 
//	/// </summary>
//	/// <remarks>
//	/// AsyncCommandArguments are passed to a mapped handler command via the <see cref="Microsoft.Maui.IElementHandler.Invoke(string, object?)" /> method.
//	/// The mapped handler command is responsible for calling the <see cref="SetComplete"/> method to 
//	/// signal to the caller that the command has completed.
//	/// </remarks>
//	public class AsyncCommandArguments
//	{
//		readonly TaskCompletionSource<object?> _taskCompletionSource;

//		/// <summary>
//		/// Called by the handler to signal when the command is complete.
//		/// </summary>
//		public Action SetComplete { get; }

//		/// <summary>
//		/// The task to be awaited by the cross-platform control invoking the command.
//		/// </summary>
//		public Task Task => _taskCompletionSource.Task;

//		public AsyncCommandArguments()
//		{
//			_taskCompletionSource = new TaskCompletionSource<object?>();
//			SetComplete = () => _taskCompletionSource.SetResult(null);
//		}
//	}
//}