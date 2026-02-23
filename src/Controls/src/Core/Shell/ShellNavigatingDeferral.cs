#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Allows async operations to complete before Shell navigation finishes.
	/// </summary>
	public class ShellNavigatingDeferral
	{
		Action _completed;

		internal ShellNavigatingDeferral(Action completed)
		{
			_completed = completed;
		}

		/// <summary>
		/// Signals that the deferred navigation can proceed.
		/// </summary>
		public void Complete()
		{
			var taskToComplete = Interlocked.Exchange(ref _completed, null);

			if (taskToComplete != null)
				taskToComplete?.Invoke();
		}

		internal bool IsCompleted => _completed == null;
	}
}