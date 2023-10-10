using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
    public class AppiumDriverElement : IUIElement
    {
        readonly AppiumElement _element;
        readonly AppiumApp _appiumApp;

        public AppiumDriverElement(AppiumElement element, AppiumApp appiumApp)
        {
            _appiumApp = appiumApp;
            _element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public ICommandExecution Command => _appiumApp.CommandExecutor;

        internal AppiumElement AppiumElement { get { return _element; } }

        public IReadOnlyCollection<IUIElement> ById(string id)
        {
            return AppiumQuery.ById(id).FindElements(_element, _appiumApp);
        }

        public IReadOnlyCollection<IUIElement> ByClass(string className)
        {
            return AppiumQuery.ByClass(className).FindElements(_element, _appiumApp);
        }

        public IReadOnlyCollection<IUIElement> ByName(string name)
        {
            return AppiumQuery.ByName(name).FindElements(_element, _appiumApp);
        }

        public IReadOnlyCollection<IUIElement> ByAccessibilityId(string id)
        {
            return AppiumQuery.ByAccessibilityId(id).FindElements(_element, _appiumApp);
        }

        public IReadOnlyCollection<IUIElement> ByQuery(string query)
        {
            return new AppiumQuery(query).FindElements(_element, _appiumApp);
        }
    }
}
