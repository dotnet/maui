using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class AsyncLock
	{
		readonly SemaphoreSlim semaphore;
		readonly Task<Releaser> releaser;

		public AsyncLock()
		{
			semaphore = new SemaphoreSlim(1);
			releaser = Task.FromResult(new Releaser(this));
		}

		public struct Releaser : IDisposable
		{
			readonly AsyncLock toRelease;

			internal Releaser(AsyncLock toRelease)
			{
				this.toRelease = toRelease;
			}

			public void Dispose()
			{
				toRelease?.semaphore.Release();
			}
		}

#if DEBUG
		public Task<Releaser> LockAsync([CallerMemberName] string callingMethod = null, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
		{
			Debug.WriteLine("AsyncLock.LockAsync called by: " + callingMethod + " in file: " + path + " : " + line);
#else
		public Task<Releaser> LockAsync()
		{
#endif
			var wait = semaphore.WaitAsync();

			return wait.IsCompleted ?
					   releaser :
					   wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
										 this, CancellationToken.None,
										 TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}
	}
}