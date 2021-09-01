using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class HostBuilderHandlerTests
	{
		[Test]
		public void DefaultHandlersAreRegistered()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>();
			Assert.NotNull(handlers);
			var handler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(handler);
			Assert.AreEqual(handler.GetType(), typeof(ButtonHandler));
		}

		[Test]
		public void CanSpecifyHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, ButtonHandlerStub>())
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>();
			Assert.NotNull(handlers);

			var specificHandler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(specificHandler);
			Assert.AreEqual(specificHandler.GetType(), typeof(ButtonHandlerStub));
		}
	}
}
