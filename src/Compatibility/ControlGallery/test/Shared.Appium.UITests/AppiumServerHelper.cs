using OpenQA.Selenium.Appium.Service;

namespace UITests;

public static class AppiumServerHelper
{
	private static AppiumLocalService? _appiumLocalService;

	public const string DefaultHostAddress = "127.0.0.1";
	public const int DefaultHostPort = 4723;

	public static void StartAppiumLocalServer(string host = DefaultHostAddress,
		int port = DefaultHostPort)
	{
		if (_appiumLocalService is not null)
		{
			return;
		}

		var builder = new AppiumServiceBuilder()
			.WithIPAddress(host)
			.UsingPort(port);

		// Start the server with the builder
		_appiumLocalService = builder.Build();
		_appiumLocalService.Start();
	}

	public static void DisposeAppiumLocalServer()
	{
		_appiumLocalService?.Dispose();
	}
}