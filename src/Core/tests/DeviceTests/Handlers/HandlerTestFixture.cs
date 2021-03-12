using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public class HandlerTestFixture : IDisposable
	{
		ApplicationStub _app;
		IHost _host;
		IMauiContext _context;

		public HandlerTestFixture()
		{
			_app = new ApplicationStub();
			_context = new ContextStub(_app);
			_host = _app
				.CreateBuilder()
				.ConfigureFonts((ctx, fonts) =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				})
				.ConfigureServices((ctx, services) =>
				{
					services.AddSingleton(_context);
				})
				.Build(_app);
		}

		public void Dispose()
		{
			_host.Dispose();
			_host = null;

			_app.Dispose();
			_app = null;

			_context = null;
		}

		public IApplication App => _app;
	}
}