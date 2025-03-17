using OpenQA.Selenium.Appium;

namespace UITest.Appium
{
	public class AppiumCatalystSwipeActions : AppiumSwipeActions
	{
		public AppiumCatalystSwipeActions(AppiumApp appiumApp) : base(appiumApp) { }

		protected override void SwipeToRight(AppiumDriver driver, AppiumElement? element, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * 0.05));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * swipePercentage));
			int endY = startY;

			var parameters = new Dictionary<string, object>
			{
				{ "startX" , startX },
				{ "startY" , startY },
				{ "endX" , endX },
				{ "endY" , endY },
				{ "duration" , 0.5 },
			};

			driver.ExecuteScript("macos: clickAndDrag", parameters);
		}

		protected override void SwipeToLeft(AppiumDriver driver, AppiumElement? element, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * swipePercentage));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * 0.05));
			int endY = startY;

			var parameters = new Dictionary<string, object>
			{
				{ "startX" , startX },
				{ "startY" , startY },
				{ "endX" , endX },
				{ "endY" , endY },
				{ "duration" , 0.5 },
			};

			driver.ExecuteScript("macos: pressAndDrag", parameters);
		}
	}
}