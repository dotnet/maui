using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	public abstract class DeferrableEventArgs : EventArgs
	{
		private int _deferralCount;
		private Func<Task> _deferralFinishedTask;
		private TaskCompletionSource<bool> _deferredTaskCompletionSource;
		private bool _deferralCompleted = false;

		public DeferrableEventArgs(bool canCancel)
		{
#if NETSTANDARD2_0
            _deferralFinishedTask = () => Task.CompletedTask;
#else
			_deferralFinishedTask = () => Task.Delay(0);
#endif

			CanCancel = canCancel;
		}

		public bool CanCancel { get; }

		public bool Cancel()
		{
			if (!CanCancel)
				return false;

			Cancelled = true;
			return true;
		}

		public bool Cancelled { get; private set; }

		public IDeferralToken GetDeferral()
		{
			if (_deferralCompleted)
				throw new InvalidOperationException("Deferral has already been completed");

			if (!CanCancel)
				return new EmptyDeferralToken();

			DeferralRequested = true;
			var currentCount = Interlocked.Increment(ref _deferralCount);
			if (currentCount == 1)
			{
				_deferredTaskCompletionSource = new TaskCompletionSource<bool>();
			}

			return new DeferralToken(DecrementDeferral);
		}

		private async void DecrementDeferral()
		{
			if (Interlocked.Decrement(ref _deferralCount) == 0)
			{
				_deferralCompleted = true;

				try
				{
					var task = _deferralFinishedTask();
					_deferralFinishedTask = null;
					await task;
					_deferredTaskCompletionSource.SetResult(!Cancelled);
				}
				catch (TaskCanceledException)
				{
					_deferredTaskCompletionSource.SetCanceled();
				}
				catch (Exception exc)
				{
					_deferredTaskCompletionSource.SetException(exc);
				}

				_deferredTaskCompletionSource = null;
			}
		}

		internal Task<bool> DeferredTask => _deferredTaskCompletionSource?.Task;
		internal bool Animate { get; set; }

		public bool DeferralRequested { get; set; }

		internal int DeferralCount => _deferralCount;

		internal bool NavigationDelayedOrCancelled =>
			Cancelled || DeferralCount > 0;

		public void RegisterDeferralCompletedCallBack(Func<Task> deferralFinishedTask)
		{
			_deferralFinishedTask = deferralFinishedTask;
		}
	}
}
