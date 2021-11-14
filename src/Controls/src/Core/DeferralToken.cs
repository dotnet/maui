using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Maui.Controls
{
	public interface IDeferralToken
	{
		void Complete();

		bool IsCompleted { get; }
	}

	public class DeferralToken : IDeferralToken
	{
		Action _completed;

		internal DeferralToken(Action completed)
		{
			_completed = completed;
		}

		public void Complete()
		{
			var taskToComplete = Interlocked.Exchange(ref _completed, null);

			if (taskToComplete != null)
				taskToComplete?.Invoke();
		}

		public bool IsCompleted => _completed == null;
	}

	/// <summary>
	/// Blank Deferral Token For When Cancellation Is Not Enabled
	/// </summary>
	public class EmptyDeferralToken : IDeferralToken
	{
		public bool IsCompleted => true;

		/* If CanCancel == false & the user has not checked for this, GetDeferralToken() would otherwise
* return null causing the Complete(); method to throw a NullReferenceException. This class prevents
* this condition from occuring
*/

		public void Complete()
		{
		}
	}
}
