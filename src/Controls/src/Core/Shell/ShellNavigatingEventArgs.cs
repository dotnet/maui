#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{

	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellNavigatingEventArgs']/Docs/*" />
	public class ShellNavigatingEventArgs : EventArgs
	{
		int _deferralCount;
		Func<Task> _deferralFinishedTask;
		TaskCompletionSource<bool> _deferredTaskCompletionSource;
		bool _deferralCompleted = false;

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ShellNavigatingEventArgs(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)
		{

			_deferralFinishedTask = () => Task.CompletedTask;
			Current = current;
			Target = target;
			Source = source;
			CanCancel = canCancel;
			Animate = true;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Current']/Docs/*" />
		public ShellNavigationState Current { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Target']/Docs/*" />
		public ShellNavigationState Target { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Source']/Docs/*" />
		public ShellNavigationSource Source { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='CanCancel']/Docs/*" />
		public bool CanCancel { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Cancel']/Docs/*" />
		public bool Cancel()
		{
			if (!CanCancel)
				return false;

			Cancelled = true;
			return true;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Cancelled']/Docs/*" />
		public bool Cancelled { get; private set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='GetDeferral']/Docs/*" />
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