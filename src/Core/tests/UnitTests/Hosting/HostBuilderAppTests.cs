using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderAppTests
	{
		[Fact]
		public void UseMauiAppRegistersApp()
		{
			var host = new AppHostBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app = host.Services.GetRequiredService<IApplication>();

			var stub = Assert.IsType<ApplicationStub>(app);
			Assert.Equal("Default", stub.Property);
		}

		[Fact]
		public void UseMauiAppRegistersAppWithFactory()
		{
			var host = new AppHostBuilder()
				.UseMauiApp(services => new ApplicationStub { Property = "Factory" })
				.Build();

			var app = host.Services.GetRequiredService<IApplication>();

			var stub = Assert.IsType<ApplicationStub>(app);
			Assert.Equal("Factory", stub.Property);
		}

		[Fact]
		public void UseMauiAppRegistersSingleton()
		{
			var host = new AppHostBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app1 = host.Services.GetRequiredService<IApplication>();
			var app2 = host.Services.GetRequiredService<IApplication>();

			Assert.Equal(app1, app2);
		}
	}
}