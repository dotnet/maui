using System;
using System.Threading;
using System.Threading.Tasks;
using Java.Util.Concurrent;

namespace Microsoft.Maui
{
	public static class FutureExtensions
	{
		public static Task<Java.Lang.Object?> AsTask(this IFuture future, CancellationToken cancellationToken = default) =>
			future.AsTask<Java.Lang.Object>(cancellationToken);

		public static Task<TResult?> AsTask<TResult>(this IFuture future, CancellationToken cancellationToken = default)
			where TResult : Java.Lang.Object
		{
			cancellationToken.Register(() => future.Cancel(true));

			return Task.Run(() =>
			{
				try
				{
					var obj = future.Get();

					if (future.IsCancelled || cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException();

					return (TResult?)obj;
				}
				catch (CancellationException)
				{
					throw new OperationCanceledException();
				}
			});
		}
	}
}