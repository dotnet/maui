using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
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

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var swipeSequence = new ActionSequence(touchDevice, 0);
			swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
			swipeSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(swipeSpeed)));
			swipeSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([swipeSequence]);
		}

		static void SwipeToLeft(AppiumDriver driver, AppiumElement? element, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * swipePercentage));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * 0.05));
			int endY = startY;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var swipeSequence = new ActionSequence(touchDevice, 0);
			swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
			swipeSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(swipeSpeed)));
			swipeSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([swipeSequence]);
		}
	}
}