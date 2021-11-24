using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	static class ApplicationExtensions
	{
		public static Window LoadPage(this Application app, Page page)
		{
			app.MainPage = page;

			return ((IApplication)app).CreateWindow(null) as Window;
		}

		public static void CreateAndSetMockApplication()
		{
			var appBuilder = MauiApp.CreateBuilder()
									.UseMauiApp<MockApplication>();
			appBuilder.Services.AddLogging(logging => logging.AddUnitTestLogger());
			var mauiApp = appBuilder.Build();
			var application = mauiApp.Services.GetRequiredService<IApplication>();
			application.Handler = new ApplicationHandlerStub();
			application.Handler.SetMauiContext(new HandlersContextStub(mauiApp.Services));
		}

	}
}