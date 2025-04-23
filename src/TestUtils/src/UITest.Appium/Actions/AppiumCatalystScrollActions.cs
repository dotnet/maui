using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystScrollActions : AppiumScrollActions
	{
		public AppiumCatalystScrollActions(AppiumApp appiumApp) : base(appiumApp) { }
		protected override void PerformActions(AppiumDriver driver, int startX, int startY, int endX, int endY, ScrollStrategy strategy, int swipeSpeed, string? elementId = "")
		{
			driver.ExecuteScript("macos: scroll", new Dictionary<string, object>
			{
				{"elementId", elementId!},
				{"deltaY" , endY - startY},
				{"deltaX" , endX - startX},
			});
			return;
		}
	}
}