using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
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

		public static async Task<bool> WaitForCollect(params WeakReference[] references)
		{
			foreach (var reference in references)
			{
				Assert.NotNull(reference);
				await reference.WaitForCollect();
			}

			// Collecting later references can also release an earlier reference.
			foreach (var reference in references)
			{
				if (reference.IsAlive)
					return false;
			}

			return true;
		}

		public static async Task WaitForGC(params WeakReference[] references)
		{
			Assert.NotEmpty(references);

			var collectResult = await WaitForCollect(references);

			Assert.True(collectResult, $"Expected all references to be collected, but some are still alive. {ListLivingReferences(references)}");
		}

		static string ListLivingReferences(WeakReference[] references)
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach (var weakReference in references)
			{
				if (weakReference.IsAlive && weakReference.Target is object x)
				{
					stringBuilder.Append($"Reference to {x} (type {x.GetType()} is still alive.\n");
				}
			}

			return stringBuilder.ToString();
		}
	}
}
