using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using UITest.Core;

namespace UITest.Appium
{
    public abstract class AppiumApp : IApp
    {
        protected readonly AppiumDriver _driver;
        protected readonly IConfig _config;
        protected readonly AppiumCommandExecutor _commandExecutor;

        public AppiumApp(AppiumDriver driver, IConfig config)
        {
            _driver = driver;
            _config = config;

            _commandExecutor = new AppiumCommandExecutor();
            _commandExecutor.AddCommandGroup(new AppiumPointerActions(this));
            _commandExecutor.AddCommandGroup(new AppiumTextActions());
            _commandExecutor.AddCommandGroup(new AppiumGeneralActions());
            _commandExecutor.AddCommandGroup(new AppiumVirtualKeyboardActions(this));
        }

        public abstract ApplicationState AppState { get; }
        public virtual IUIElementQueryable Query => new AppiumQueryable(this);
        public IConfig Config => _config;
        public AppiumDriver Driver => _driver;
        public ICommandExecution CommandExecutor => _commandExecutor;
        public string ElementTree => _driver.PageSource;

        public void Click(float x, float y)
        {
            CommandExecutor.Execute("click", new Dictionary<string, object>()
            {
                { "x", x },
                { "y", y }
            });
        }

        public FileInfo Screenshot(string fileName)
        {
            if (_driver == null)
            {
                throw new NullReferenceException("Screenshot: _driver is null");
            }

            string filename = $"{fileName}.png";
            Screenshot screenshot = _driver.GetScreenshot();
            screenshot.SaveAsFile(filename, ScreenshotImageFormat.Png);
            var file = new FileInfo(filename);
            return file;
        }

        public byte[] Screenshot()
        {
            if (_driver == null)
            {
                throw new NullReferenceException("Screenshot: _driver is null");
            }

            Screenshot screenshot = _driver.GetScreenshot();
            return screenshot.AsByteArray;
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
