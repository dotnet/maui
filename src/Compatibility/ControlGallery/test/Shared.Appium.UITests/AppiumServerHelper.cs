using OpenQA.Selenium.Appium.Service;

namespace UITests;

public static class AppiumServerHelper
{
	static AppiumLocalService? AppiumLocalService;

	public const string DefaultHostAddress = "127.0.0.1";
	public const int DefaultHostPort = 4723;

	public static void StartAppiumLocalServer(string host = DefaultHostAddress,
		int port = DefaultHostPort)
	{
		if (AppiumLocalService is not null)
		{
			return;
		}

		var builder = new AppiumServiceBuilder()
			.WithIPAddress(host)
			.UsingPort(port);

		// Start the server with the builder
		AppiumLocalService = builder.Build();
		AppiumLocalService.Start();
	}

	public static void DisposeAppiumLocalServer()
	{
		AppiumLocalService?.Dispose();
	}
}