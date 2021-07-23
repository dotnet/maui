using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	static class ThreadPoolHelper
	{
		public static async void RunAsync(Action action)
		{

			var task = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			try
			{
				await task;
			}
			catch (Exception e)
			{
				if (Debugger.IsAttached)
				{
					Debugger.Break();
					Debug.WriteLine(e);
				}
			}

		}
	}
}
