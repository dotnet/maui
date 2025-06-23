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
			GC.Collect(2, GCCollectionMode.Forced, true);
			GC.WaitForPendingFinalizers();
			GC.Collect(2, GCCollectionMode.Forced, true);
			await Task.Yield();
		}


		public static async Task<bool> WaitForCollect(this WeakReference reference)
		{
			for (int i = 0; i < 40 && reference.IsAlive; i++)
			{
				await Collect();
			}

			return reference.IsAlive;
		}

		public static async Task<bool> WaitForCollect(this WeakReference<object> reference)
		{
			for (int i = 0; i < 40 && reference.TryGetTarget(out _); i++)
			{
				await Collect();
			}

			return reference.TryGetTarget(out _);
		}
	}
}