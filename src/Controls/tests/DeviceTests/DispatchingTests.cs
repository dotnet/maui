using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Dispatcher)]
	public class DispatchingTests
	{
		[Fact]
		public async Task DispatchFromBackgroundThread()
		{
			await Task.Run(async () =>
			{
				await Device.InvokeOnMainThreadAsync(async () =>
				{
					await Task.Delay(0);
				});
			});
		}

	}
}
