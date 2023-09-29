using System.Diagnostics;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Windows;

namespace TestUtils.Appium.UITests
{
	public sealed class AppiumContext : IContext
	{
		const int Port = 4723;
		readonly AppiumOptions _appiumOptions;
		readonly List<UITestContext> _contexts; // Since tests don't know when they are done, we need to keep track of all the contexts we create so we can dispose them
		readonly int _serverStartWaitDelay = 1000;
		readonly TimeSpan _maxServerWaitTime = TimeSpan.FromSeconds(15);
		readonly object _serverLock = new object();
		AppiumLocalService? _server;

		public AppiumContext()
		{
			_appiumOptions = new AppiumOptions();
			_contexts = new List<UITestContext>();
		}

		public void CreateAndStartServer(int port = Port)
		{
			lock (_serverLock)
			{
				_server?.Dispose();

				var arguments = new OpenQA.Selenium.Appium.Service.Options.OptionCollector();
				arguments.AddArguments(new KeyValuePair<string, string>("--base-path", "/wd/hub"));

				var logFile = Environment.GetEnvironmentVariable("APPIUM_LOG_FILE") ?? "appium.log";

				var service = new AppiumServiceBuilder()
					.WithArguments(arguments)
					.UsingPort(port)
					.WithLogFile(new FileInfo(logFile))
					.Build();

				service.OutputDataReceived += (s, e) => Debug.WriteLine($"Appium {e.Data}");
				service.Start();
				_server = service;

				DateTime start = DateTime.Now;

				while (!_server.IsRunning)
				{
					long elapsed = DateTime.Now.Subtract(start).Ticks;
					if (elapsed >= _maxServerWaitTime.Ticks)
					{
						Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {_maxServerWaitTime.Ticks}");

						throw new TimeoutException($"Timed out waiting for Appium server to start after waiting for {_maxServerWaitTime.Seconds}s");
					}

					Task.Delay(_serverStartWaitDelay).Wait();
				}
			}
		}

		public UITestContext CreateUITestContext(TestConfig testConfig)
		{
			lock (_serverLock)
			{
				if (_server == null || !_server.IsRunning)
				{
					TestContext.Error.WriteLine($">>>>> {DateTime.Now} Server was not running when calling {nameof(CreateUITestContext)}, starting it ourselves...");
					CreateAndStartServer();
				}
			}

			if (testConfig == null)
				throw new ArgumentNullException(nameof(testConfig));

			SetGeneralAppiumOptions(testConfig, _appiumOptions);
			SetPlatformAppiumOptions(testConfig, _appiumOptions);

			var driverUri = new Uri($"http://localhost:{Port}/wd/hub");

			AppiumDriver driver;
			int retries = 0;

			// It has been observed on iOS CI runs that the first launch can take some time (Unclear if simulator, runtime, or test framework is the cause)
			// So we will try a few times before we give up
			while (true)
			{
				try
				{
					driver = testConfig.TestDevice switch
					{
						TestDevice.Android => new AndroidDriver(driverUri, _appiumOptions),
						TestDevice.iOS => new IOSDriver(driverUri, _appiumOptions),
						TestDevice.Mac => new MacDriver(driverUri, _appiumOptions),
						TestDevice.Windows => new WindowsDriver(driverUri, _appiumOptions),
						_ => throw new InvalidOperationException("Unknown test device"),
					};
					break;
				}
				catch (WebDriverException)
				{
					// Default command timeout is 60 seconds when executing the NewSessionCommand
					if (retries++ < 10)
					{
						TestContext.Error.WriteLine($">>>>> {DateTime.Now} Retrying to create the driver, attempt #{retries}");
					}
					else
					{
						throw;
					}
				}
			}

			var newContext = new UITestContext(new AppiumUITestApp(testConfig.AppId, driver), testConfig);
			_contexts.Add(newContext);

			return newContext;
		}

		static void SetPlatformAppiumOptions(TestConfig testConfig, AppiumOptions appiumOptions)
		{
			var appId = testConfig.BundleId ?? testConfig.AppId;
			appiumOptions.PlatformName = testConfig.PlatformName;
			appiumOptions.AutomationName = testConfig.AutomationName;

			if (!string.IsNullOrEmpty(testConfig.DeviceName))
				appiumOptions.DeviceName = testConfig.DeviceName;

			if (!string.IsNullOrEmpty(testConfig.PlatformVersion))
				appiumOptions.PlatformVersion = testConfig.PlatformVersion;

			if (!string.IsNullOrEmpty(testConfig.AppPath))
				appiumOptions.App = testConfig.AppPath;

			switch (testConfig.TestDevice)
			{
				case TestDevice.iOS:
					appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, testConfig.Udid);
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
					break;
				case TestDevice.Mac:
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
					appiumOptions.AddAdditionalAppiumOption("showServerLogs", true);
					break;
			}
		}

		static void SetGeneralAppiumOptions(TestConfig testConfig, AppiumOptions appiumOptions)
		{
			appiumOptions.AddAdditionalAppiumOption("reportDirectory", testConfig.ReportDirectory);
			appiumOptions.AddAdditionalAppiumOption("reportFormat", testConfig.ReportFormat);

			if (string.IsNullOrEmpty(testConfig.TestName))
				appiumOptions.AddAdditionalAppiumOption("testName", testConfig.TestName);

			if (testConfig.FullReset)
				appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.FullReset, "true");

			appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.NewCommandTimeout, 3000);
		}

		public void Dispose()
		{
			foreach (var context in _contexts)
			{
				context.Dispose();
			}

			_contexts.Clear();

			lock (_serverLock)
			{
				_server?.Dispose();
				_server = null;
			}
		}
	}
}
