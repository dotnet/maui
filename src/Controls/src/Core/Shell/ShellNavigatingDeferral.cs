#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingDeferral.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellNavigatingDeferral']/Docs/*" />
	public class ShellNavigatingDeferral
	{
		Action _completed;

		internal ShellNavigatingDeferral(Action completed)
		{
			_completed = completed;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingDeferral.xml" path="//Member[@MemberName='Complete']/Docs/*" />
		public void Complete()
		{
			var taskToComplete = Interlocked.Exchange(ref _completed, null);

			if (taskToComplete != null)
				taskToComplete?.Invoke();
		}

		internal bool IsCompleted => _completed == null;
	}
}