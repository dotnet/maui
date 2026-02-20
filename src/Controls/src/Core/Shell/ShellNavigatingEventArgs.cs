#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{

	/// <summary>
	/// Provides data for the <see cref="Shell.Navigating"/> event.
	/// </summary>
	public class ShellNavigatingEventArgs : EventArgs
	{
		int _deferralCount;
		Func<Task> _deferralFinishedTask;
		TaskCompletionSource<bool> _deferredTaskCompletionSource;
		bool _deferralCompleted = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellNavigatingEventArgs"/> class.
		/// </summary>
		/// <param name="current">The current navigation state.</param>
		/// <param name="target">The target navigation state.</param>
		/// <param name="source">The source of the navigation.</param>
		/// <param name="canCancel">Whether the navigation can be cancelled.</param>
		public ShellNavigatingEventArgs(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)
		{

			_deferralFinishedTask = () => Task.CompletedTask;
			Current = current;
			Target = target;
			Source = source;
			CanCancel = canCancel;
			Animate = true;
		}

		/// <summary>
		/// Gets the current navigation state.
		/// </summary>
		public ShellNavigationState Current { get; }

		/// <summary>
		/// Gets the target navigation state.
		/// </summary>
		public ShellNavigationState Target { get; }

		/// <summary>
		/// Gets the source of the navigation.
		/// </summary>
		public ShellNavigationSource Source { get; }

		/// <summary>
		/// Gets a value indicating whether the navigation can be cancelled.
		/// </summary>
		public bool CanCancel { get; }

		/// <summary>
		/// Cancels the navigation if <see cref="CanCancel"/> is <see langword="true"/>.
		/// </summary>
		/// <returns><see langword="true"/> if the navigation was cancelled; otherwise, <see langword="false"/>.</returns>
		public bool Cancel()
		{
			if (!CanCancel)
				return false;

			Cancelled = true;
			return true;
		}

		/// <summary>
		/// Gets a value indicating whether the navigation has been cancelled.
		/// </summary>
		public bool Cancelled { get; private set; }

		/// <summary>
		/// Gets a deferral to delay navigation until the deferral is completed.
		/// </summary>
		/// <returns>A <see cref="ShellNavigatingDeferral"/> object, or <see langword="null"/> if <see cref="CanCancel"/> is <see langword="false"/>.</returns>
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