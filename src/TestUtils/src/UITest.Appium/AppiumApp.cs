using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using UITest.Core;

namespace UITest.Appium
{
	public abstract class AppiumApp : IApp, IScreenshotSupportedApp, ILogsSupportedApp
	{
		protected readonly AppiumDriver _driver;
		protected readonly IConfig _config;
		protected readonly AppiumCommandExecutor _commandExecutor;

		public AppiumApp(AppiumDriver driver, IConfig config)
		{
			_driver = driver ?? throw new ArgumentNullException(nameof(driver));
			_config = config ?? throw new ArgumentNullException(nameof(config));

			_commandExecutor = new AppiumCommandExecutor();
			_commandExecutor.AddCommandGroup(new AppiumPointerActions(this));
			_commandExecutor.AddCommandGroup(new AppiumTextActions());
			_commandExecutor.AddCommandGroup(new AppiumGeneralActions());
			_commandExecutor.AddCommandGroup(new AppiumVirtualKeyboardActions(this));
			_commandExecutor.AddCommandGroup(new AppiumSliderActions(this));
			_commandExecutor.AddCommandGroup(new AppiumSwipeActions(this));
			_commandExecutor.AddCommandGroup(new AppiumScrollActions(this));
			_commandExecutor.AddCommandGroup(new AppiumOrientationActions(this));
			_commandExecutor.AddCommandGroup(new AppiumLifecycleActions(this));
		}

		public abstract ApplicationState AppState { get; }
		public virtual IUIElementQueryable Query => new AppiumQueryable(this);
		public IConfig Config => _config;
		public AppiumDriver Driver => _driver;
		public ICommandExecution CommandExecutor => _commandExecutor;
		public string ElementTree => _driver.PageSource;

		public FileInfo Screenshot(string fileName)
		{
			Screenshot screenshot = _driver.GetScreenshot();
			screenshot.SaveAsFile(fileName);
			var file = new FileInfo(fileName);
			return file;
		}

		public byte[] Screenshot()
		{
			Screenshot screenshot = _driver.GetScreenshot();
			return screenshot.AsByteArray;
		}

		public IEnumerable<string> GetLogTypes()
		{
			return _driver.Manage().Logs.AvailableLogTypes;
		}

		public IEnumerable<string> GetLogEntries(string logType)
		{
			var entries = _driver.Manage().Logs.GetLog(logType);
			foreach (var entry in entries)
			{
				yield return entry.Message;
			}
		}

#nullable disable
		public virtual IUIElement FindElement(string id)
		{
			return Query.ById(id).FirstOrDefault();
		}
#nullable enable

		public virtual IUIElement FindElement(IQuery query)
		{
			AppiumQuery? appiumQuery = query as AppiumQuery;
			if (appiumQuery is not null)
			{
				return appiumQuery.FindElement(this);
			}

			var queryString = query.ToString() ?? throw new InvalidOperationException($"{nameof(FindElement)} could not get query string");
			var q = new AppiumQuery(queryString);
			return q.FindElement(this);
		}

		public virtual IReadOnlyCollection<IUIElement> FindElements(string id)
		{
			return Query.ById(id);
		}

		public virtual IReadOnlyCollection<IUIElement> FindElements(IQuery query)
		{
			AppiumQuery? appiumQuery = query as AppiumQuery;
			if (appiumQuery is not null)
			{
				return appiumQuery.FindElements(this);
			}

			var queryString = query.ToString() ?? throw new InvalidOperationException($"{nameof(FindElement)} could not get query string");
			var q = new AppiumQuery(queryString);
			return q.FindElements(this);
		}

		protected static void SetGeneralAppiumOptions(IConfig config, AppiumOptions appiumOptions)
		{
			appiumOptions.AddAdditionalAppiumOption("reportDirectory", config.GetProperty<string>("ReportDirectory"));
			appiumOptions.AddAdditionalAppiumOption("reportFormat", config.GetProperty<string>("ReportFormat"));

			var testName = config.GetProperty<string>("TestName");
			if (!string.IsNullOrWhiteSpace(testName))
				appiumOptions.AddAdditionalAppiumOption("testName", testName);

			if (config.GetProperty<bool>("FullReset"))
				appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.FullReset, "true");

			if (config.GetProperty<bool>("NoReset"))
				appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.NoReset, "true");

			var appPath = config.GetProperty<string>("AppPath");
			if (!string.IsNullOrEmpty(appPath))
				appiumOptions.App = appPath;

			appiumOptions.PlatformName = config.GetProperty<string>("PlatformName");
			appiumOptions.AutomationName = config.GetProperty<string>("AutomationName");

			var deviceName = config.GetProperty<string>("DeviceName");
			if (!string.IsNullOrEmpty(deviceName))
				appiumOptions.DeviceName = deviceName;

			var platformVersion = config.GetProperty<string>("PlatformVersion");
			if (!string.IsNullOrEmpty(platformVersion))
				appiumOptions.PlatformVersion = platformVersion;

			appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.NewCommandTimeout, 3000);
		}

		public void Dispose()
		{
			_driver?.Dispose();
		}
	}
}
