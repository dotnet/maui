using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public class HandlerTestFixture : IDisposable
	{
		AppStub _app;
		IHost _host;

		public HandlerTestFixture()
		{
			_app = new AppStub();
			_host = _app
				.CreateBuilder()
				.ConfigureFonts((ctx, fonts) =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				})
				.Build(_app);
		}

		public void Dispose()
		{
			_host.Dispose();
			_host = null;

			_app.Dispose();
			_app = null;
		}

		public IApp App => _app;
	}
}