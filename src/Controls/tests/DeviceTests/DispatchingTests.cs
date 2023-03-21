using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Dispatcher)]
	public class DispatchingTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task DispatchFromBackgroundThread()
		{
			bool dispatched = false;
			await Task.Run(async () =>
			{
				var app = await InvokeOnMainThreadAsync(() => new Application(false));
				var handler = new ApplicationHandlerStub();
				handler.SetMauiContext(MauiContext);
				app.Handler = handler;

				await app
					.FindDispatcher()
					.DispatchAsync(async () =>
				{
					await Task.Delay(0);
					dispatched = true;
				});
			});

			await Task.Delay(500);
			Assert.True(dispatched);
		}

		public class ApplicationHandlerStub : ElementHandler<Application, object>
		{
			public ApplicationHandlerStub() : base(ElementHandler.ElementMapper)
			{

			}

			protected override object CreatePlatformElement() => new Object();
		}
	}
}
