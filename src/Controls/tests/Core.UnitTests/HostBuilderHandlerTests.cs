using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using NUnit;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class HostBuilderHandlerTests
	{
		[Test]
		public void DefaultHandlersAreRegistered()
		{
			var host = new AppHostBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handler = host.Handlers.GetHandler(typeof(Button));

			Assert.NotNull(handler);
			Assert.AreEqual(handler.GetType(), typeof(ButtonHandler));
		}

		[Test]
		public void CanSpecifyHandler()
		{
			var host = new AppHostBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<Button, ButtonHandlerStub>())
				.Build();

			var specificHandler = host.Handlers.GetHandler(typeof(Button));

			Assert.NotNull(specificHandler);
			Assert.AreEqual(specificHandler.GetType(), typeof(ButtonHandlerStub));
		}
	}
}
