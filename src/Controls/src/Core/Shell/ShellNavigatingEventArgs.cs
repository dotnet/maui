using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	public class ShellNavigatingEventArgs : EventArgs
	{
		int _deferralCount;
		Func<Task> _deferralFinishedTask;
		TaskCompletionSource<bool> _deferredTaskCompletionSource;
		bool _deferralCompleted = false;

		public ShellNavigatingEventArgs(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)
		{

#if !NETSTANDARD1_0
			_deferralFinishedTask = () => Task.CompletedTask;
#else
			_deferralFinishedTask = () => Task.Delay(0);
#endif
			Current = current;
			Target = target;
			Source = source;
			CanCancel = canCancel;
			Animate = true;
		}

		public ShellNavigationState Current { get; }

		public ShellNavigationState Target { get; }

		public ShellNavigationSource Source { get; }

		public bool CanCancel { get; }

		public bool Cancel()
		{
			if (!CanCancel)
				return false;

			Cancelled = true;
			return true;
		}

		public bool Cancelled { get; private set; }

		public ShellNavigatingDeferral GetDeferral()
		{
			if (_deferralCompleted)
				throw new InvalidOperationException("Deferral has already been completed");

			if (!CanCancel)
				return null;

			DeferredEventArgs = true;
			var currentCount = Interlocked.Increment(ref _deferralCount);
			if (currentCount == 1)
			{
				_deferredTaskCompletionSource = new TaskCompletionSource<bool>();
			}

			return new ShellNavigatingDeferral(DecrementDeferral);
		}

		async void DecrementDeferral()
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
		internal bool DeferredEventArgs { get; set; }

		internal int DeferralCount => _deferralCount;

		internal bool NavigationDelayedOrCancelled =>
			Cancelled || DeferralCount > 0;

		internal void RegisterDeferralCompletedCallBack(Func<Task> deferralFinishedTask)
		{
			_deferralFinishedTask = deferralFinishedTask;
		}
	}
}