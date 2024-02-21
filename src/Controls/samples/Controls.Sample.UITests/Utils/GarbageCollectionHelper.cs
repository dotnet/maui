using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{
	public static class GarbageCollectionHelper
	{
		public static async Task WaitForGC(params WeakReference[] references) => await WaitForGC(5000, references);

		public static async Task WaitForGC(int timeout, params WeakReference[] references)
		{
			bool referencesCollected()
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();

				foreach (var reference in references)
				{
					if (reference.IsAlive)
					{
						return false;
					}
				}

				return true;
			}

			await AssertEventually(referencesCollected, timeout);
		}

		public static async Task AssertEventually(this Func<bool> assertion, int timeout = 1000, int interval = 100, string message = "Assertion timed out")
		{
			do
			{
				if (assertion())
				{
					return;
				}

				await Task.Delay(interval);
				timeout -= interval;

			}
			while (timeout >= 0);

			if (!assertion())
			{
				throw new Exception(message);
			}
		}
	}
}
