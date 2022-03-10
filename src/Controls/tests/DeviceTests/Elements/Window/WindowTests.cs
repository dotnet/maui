using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowTests : HandlerTestBase
	{
		[Fact]
		public async Task WindowHasReasonableDisplayDensity()
		{
			var window = MauiProgram.CurrentTestApp.Windows[0];

			var density = await InvokeOnMainThreadAsync(() => window.DisplayDensity);

			Assert.InRange(density, 0.1f, 4f);
		}
	}
}
