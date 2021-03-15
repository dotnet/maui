using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public class HandlerTestFixture : IDisposable
	{
		AppStub _app;
		IHost _host;
		IMauiContext _context;

		public HandlerTestFixture()
		{
			StartupStub startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureFonts((ctx, fonts) =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				})
				.ConfigureServices((ctx, services) =>
				{
					services.AddSingleton(_context);
				});

			startup.Configure(appBuilder);

			_host = appBuilder.Build();

			_app = new AppStub();

			appBuilder.SetServiceProvider(_app);

			_context = new ContextStub(_app);
		}

		public void Dispose()
		{
			_host.Dispose();
			_host = null;

			_app.Dispose();
			_app = null;

			_context = null;
		}

		public IApp App => _app;
	}
}