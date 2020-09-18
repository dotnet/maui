using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public class ShellNavigatingEventArgs : EventArgs
	{
		int _deferalCount;
		Func<Task> _deferralFinishedTask;
		TaskCompletionSource<bool> _deferredTaskCompletionSource;

		public ShellNavigatingEventArgs(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)
		{
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
			if (!CanCancel)
				return null;

			DeferredEventArgs = true;
			var currentCount = Interlocked.Increment(ref _deferalCount);
			if(currentCount == 1)
			{
				_deferredTaskCompletionSource = new TaskCompletionSource<bool>();
			}

			return new ShellNavigatingDeferral(DecrementDeferral);
		}

		async void DecrementDeferral()
		{
			if (Interlocked.Decrement(ref _deferalCount) == 0)
			{
				var task = _deferralFinishedTask();
				_deferralFinishedTask = null;

				try
				{
					await task;
					_deferredTaskCompletionSource.SetResult(true);
				}
				catch(TaskCanceledException)
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

		internal int DeferralCount => _deferalCount;

		internal void RegisterDeferralCompletedCallBack(Func<Task> deferralFinishedTask)
		{
			_deferralFinishedTask = deferralFinishedTask;
		}
	}
}