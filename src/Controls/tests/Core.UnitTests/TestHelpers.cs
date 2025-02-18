using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal static class TestHelpers
	{
		public static async Task Collect()
		{
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}


		public static async Task<bool> WaitForCollect(this WeakReference reference)
		{
			for (int i = 0; i < 40 && reference.IsAlive; i++)
			{
				await Task.Yield();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			return reference.IsAlive;
		}

		public static async Task<bool> WaitForCollect<T>(this WeakReference<T> reference)
			where T : class
		{
			for (int i = 0; i < 40 && reference.TryGetTarget(out _); i++)
			{
				await Task.Yield();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			return reference.TryGetTarget(out _);
		}
	}
}
