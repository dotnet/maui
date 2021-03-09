using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	public class ShellNavigatingDeferral
	{
		Action _completed;

		internal ShellNavigatingDeferral(Action completed)
		{
			_completed = completed;
		}

		public void Complete()
		{
			var taskToComplete = Interlocked.Exchange(ref _completed, null);

			if (taskToComplete != null)
				taskToComplete?.Invoke();
		}

		internal bool IsCompleted => _completed == null;
	}
}