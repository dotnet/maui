using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class HostBuilderAppTests
	{
		[Test]
		public void UseMauiAppRegistersApp()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app = (ApplicationStub)mauiApp.Services.GetRequiredService<IApplication>();
			Assert.AreEqual("Default", app.Property);
		}

		[Test]
		public void UseMauiAppRegistersAppWithFactory()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp(services => new ApplicationStub { Property = "Factory" })
				.Build();

			var app = (ApplicationStub)mauiApp.Services.GetRequiredService<IApplication>();
			Assert.AreEqual("Factory", app.Property);
		}

		[Test]
		public void UseMauiAppRegistersSingleton()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app1 = mauiApp.Services.GetRequiredService<IApplication>();
			var app2 = mauiApp.Services.GetRequiredService<IApplication>();

			Assert.AreEqual(app1, app2);
		}
	}
}
