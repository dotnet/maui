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
	}
}
