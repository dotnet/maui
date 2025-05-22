using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal static class TestHelpers
	{
		public static async Task Collect()
		{
			await Task.Yield();
			GC.Collect(2);
			await Task.Yield();
			GC.WaitForPendingFinalizers();
			await Task.Yield();
			GC.Collect(2);
			await Task.Yield();
			GC.WaitForPendingFinalizers();
		}


		public static async Task<bool> WaitForCollect(this WeakReference reference)
		{
			for (int i = 0; i < 40 && reference.IsAlive; i++)
			{
				await Task.Yield();
				GC.Collect(2);
				await Task.Yield();
				GC.WaitForPendingFinalizers();
				await Task.Yield();
				GC.Collect(2);
				await Task.Yield();
				GC.WaitForPendingFinalizers();
			}

			return reference.IsAlive;
		}
	}
}
