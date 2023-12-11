using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumSwipeActions : ICommandExecutionGroup
	{
		const string SwipeLeftToRightCommand = "swipeLeftToRight";
		const string SwipeRightToLeftCommand = "swipeRightToLeft";

		protected readonly AppiumApp _app;

		readonly List<string> _commands = new()
		{
			SwipeLeftToRightCommand,
			SwipeRightToLeftCommand
		};

		public AppiumSwipeActions(AppiumApp app)
		{
			_app = app;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				SwipeLeftToRightCommand => SwipeLeftToRight(parameters),
				SwipeRightToLeftCommand => SwipeRightToLeft(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse SwipeLeftToRight(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			SwipeToRight(_app.Driver, element, swipePercentage, swipeSpeed, withInertia);

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse SwipeRightToLeft(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			SwipeToLeft(_app.Driver, element, swipePercentage, swipeSpeed, withInertia);

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

		static void SwipeToRight(AppiumDriver driver, AppiumElement? element, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * 0.05));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * swipePercentage));
			int endY = startY;

			new TouchAction(driver)
				.Press(startX, startY)
				.Wait(swipeSpeed)
				.MoveTo(endX, endY)
				.Release()
				.Perform();
		}

		static void SwipeToLeft(AppiumDriver driver, AppiumElement? element, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * swipePercentage));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * 0.05));
			int endY = startY;

			new TouchAction(driver)
				.Press(startX, startY)
				.Wait(swipeSpeed)
				.MoveTo(endX, endY)
				.Release()
				.Perform();
		}
	}
}