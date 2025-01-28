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

			int y = (int)(position.X + (size.Width * swipePercentage));
			int x = position.Y + size.Height / 2;

			var parameters = new Dictionary<string, object>
			{
				{ "x" , x },
				{ "y" , y },
				{ "direction" , "right" },
				{ "velocity" , swipeSpeed },

			};

			if (element is not null)
			{
				// The internal element identifier to swipe on.
				// If both are set then x and y are considered as relative element coordinates.
				// If only x and y are set then these are parsed as absolute coordinates.
				parameters.Add("elementId", element.Id);
			}

			driver.ExecuteScript("mobile: swipe", parameters);
		}

		protected override void SwipeToLeft(AppiumDriver driver, AppiumElement? element, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int x = (int)(position.X + (size.Width * 0.05));
			int y = position.Y + size.Height / 2;

			var parameters = new Dictionary<string, object>
			{
				{ "x" , x },
				{ "y" , y },
				{ "direction" , "left" },
				{ "velocity" , swipeSpeed },

			};

			if (element is not null)
			{
				// The internal element identifier to swipe on.
				// If both are set then x and y are considered as relative element coordinates.
				// If only x and y are set then these are parsed as absolute coordinates.
				parameters.Add("elementId", element.Id);
			}

			driver.ExecuteScript("mobile: swipe", parameters);
		}
	}
}