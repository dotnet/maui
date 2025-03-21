using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.UnitTests
{
	internal static class TestHelpers
	{
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

		public static void Collect(int count = 10)
		{
			for (int i = 0; i < count; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public static async Task CollectAsync(int count = 10)
		{
			for (int i = 0; i < count; i++)
			{
				await Task.Yield();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}
	}
}
