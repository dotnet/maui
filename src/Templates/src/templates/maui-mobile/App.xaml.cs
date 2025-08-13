using Microsoft.Extensions.DependencyInjection;

namespace MauiApp._1;

public partial class App : Application
{
	public IServiceProvider ServiceProvider { get; private set; }

	public App()
	{
		InitializeComponent();
		var serviceCollection = new ServiceCollection();
		ConfigureServices(serviceCollection);
		ServiceProvider = serviceCollection.BuildServiceProvider();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	void ConfigureServices(IServiceCollection services)
	{
#if IOS
		services.AddSingleton<IAsyncAnnouncement, SemanticScreenReaderAsyncImplementation>();
#endif
	}
}