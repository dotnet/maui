using UITest.Core;

namespace UITest.Appium
{
    public class AppiumAndroidQueryable : AppiumQueryable
    {
        public AppiumAndroidQueryable(AppiumApp appiumApp) 
            : base(appiumApp)
        {
        }

        public override IReadOnlyCollection<IUIElement> ById(string id)
        {
            // Android needs to be in the form of "{appId}:id/{id}"
            // e.g. com.microsoft.maui.uitests:id/navigationlayout_appbar
            id = $"{_appiumApp.Config.GetProperty<string>("AppId")}:id/{id}";
            return base.ById(id);
        }
    }
}
