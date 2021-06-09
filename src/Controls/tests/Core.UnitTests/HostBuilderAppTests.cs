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
	public class HostBuilderAppTests
	{
		[Test]
		public void UseMauiAppRegistersApp()
		{
			var host = new AppHostBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app = (ApplicationStub)host.Services.GetRequiredService<IApplication>();
			Assert.AreEqual("Default", app.Property);
		}

		[Test]
		public void UseMauiAppRegistersAppWithFactory()
		{
			var host = new AppHostBuilder()
				.UseMauiApp(services => new ApplicationStub { Property = "Factory" })
				.Build();

			var app = (ApplicationStub)host.Services.GetRequiredService<IApplication>();
			Assert.AreEqual("Factory", app.Property);
		}

		[Test]
		public void UseMauiAppRegistersSingleton()
		{
			var host = new AppHostBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app1 = host.Services.GetRequiredService<IApplication>();
			var app2 = host.Services.GetRequiredService<IApplication>();

			Assert.AreEqual(app1, app2);
		}
	}
}
