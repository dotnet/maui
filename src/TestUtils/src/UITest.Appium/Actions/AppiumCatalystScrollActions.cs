using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystScrollActions : AppiumScrollActions
	{
        public AppiumCatalystScrollActions(AppiumApp app)
			: base(app)
		{
		}
        
		protected override CommandResponse ScrollLeft(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			ScrollToLeft(_appiumApp.Driver, element, strategy, swipePercentage, swipeSpeed, withInertia);

			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse ScrollDown(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			ScrollToDown(_appiumApp.Driver, element, strategy, swipePercentage, swipeSpeed, withInertia);

			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse ScrollRight(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			ScrollToRight(_appiumApp.Driver, element, strategy, swipePercentage, swipeSpeed, withInertia);

			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse ScrollUp(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			ScrollToUp(_appiumApp.Driver, element, strategy, swipePercentage, swipeSpeed, withInertia);

			return CommandResponse.SuccessEmptyResponse;
		}

		static AppiumElement? GetAppiumElement(object? element)
		{
			if (element is AppiumElement appiumElement)
			{
				return appiumElement;
			}
			else if (element is AppiumDriverElement driverElement)
			{
				return driverElement.AppiumElement;
			}

			return null;
		}

		static void ScrollToLeft(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int x = (int)(position.X + (size.Width * 0.05));
			int y = position.Y + size.Height / 2;

			int deltaX = (int)(position.X + (size.Width * swipePercentage));
			int deltaY = y;

			var parameters = new Dictionary<string, object>
            {
                { "x", x },
                { "y", y },
                { "deltaX", deltaX },
                { "deltaY", deltaY },
            };

            if(element is not null)
            {
                parameters.Add("elementId", element.Id); 
            }

            driver.ExecuteScript("macos: scroll", parameters);
		}

		static void ScrollToDown(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int x = position.X + size.Width / 2;
			int y = (int)(position.Y + (size.Height * swipePercentage));

			int deltaX = x;
			int deltaY = (int)(position.Y + (size.Height * 0.05));

            var parameters = new Dictionary<string, object>
            {
                { "x", x },
                { "y", y },
                { "deltaX", deltaX },
                { "deltaY", deltaY },
            };

            if(element is not null)
            {
                parameters.Add("elementId", element.Id); 
            }

            driver.ExecuteScript("macos: scroll", parameters); 
		}

		static void ScrollToRight(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int x = (int)(position.X + (size.Width * swipePercentage));
			int y = position.Y + size.Height / 2;

			int deltaX = (int)(position.X + (size.Width * 0.05));
			int endY = y;

			var parameters = new Dictionary<string, object>
            {
                { "x", x },
                { "y", y },
                { "deltaX", deltaX },
                { "deltaY", endY },
            };

            if(element is not null)
            {
                parameters.Add("elementId", element.Id); 
            }

            driver.ExecuteScript("macos: scroll", parameters);
		}

		static void ScrollToUp(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int x = position.X + size.Width / 2;
			int y = (int)(position.Y + (size.Height * 0.05));

			int deltaX = x;
			int deltaY = (int)(position.Y + (size.Height * swipePercentage));

            var parameters = new Dictionary<string, object>
            {
                { "x", x },
                { "y", y },
                { "deltaX", deltaX },
                { "deltaY", deltaY },
            };

            if(element is not null)
            {
                parameters.Add("elementId", element.Id); 
            }

            driver.ExecuteScript("macos: scroll", parameters);
		}
	}
}