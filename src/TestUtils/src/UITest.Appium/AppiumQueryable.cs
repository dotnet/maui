using UITest.Core;

namespace UITest.Appium
{
    public class AppiumQueryable : IUIElementQueryable
    {
        protected readonly AppiumApp _appiumApp;

        public AppiumQueryable(AppiumApp appiumApp)
        {
            _appiumApp = appiumApp ?? throw new ArgumentNullException(nameof(appiumApp));
        }

        public virtual IReadOnlyCollection<IUIElement> ById(string id)
        {
            return AppiumQuery.ById(id).FindElements(_appiumApp);
        }

        public virtual IReadOnlyCollection<IUIElement> ByAccessibilityId(string name)
        {
            return AppiumQuery.ByAccessibilityId(name).FindElements(_appiumApp);
        }

        public virtual IReadOnlyCollection<IUIElement> ByClass(string className)
        {
            return AppiumQuery.ByClass(className).FindElements(_appiumApp);
        }

        public virtual IReadOnlyCollection<IUIElement> ByName(string name)
        {
            return AppiumQuery.ByName(name).FindElements(_appiumApp);
        }

        public virtual IReadOnlyCollection<IUIElement> ByQuery(string query)
        {
            var appiumQuery = new AppiumQuery(query);
            return appiumQuery.FindElements(_appiumApp);
        }
    }
}
