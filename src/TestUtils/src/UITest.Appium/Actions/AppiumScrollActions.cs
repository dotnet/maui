using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Core;

namespace UITest.Appium
{
	public enum ScrollStrategy
	{
		Auto,
		Gesture,
		Programmatically
	}

	public class AppiumScrollActions : ICommandExecutionGroup
	{
		const int ScrollTouchDownTime = 100;
		const int ProgrammaticallyScrollTime = 0;

		const string ScrollLeftCommand = "scrollLeft";
		const string ScrollDownCommand = "scrollDown";
		const string ScrollRightCommand = "scrollRight";
		const string ScrollUpCommand = "scrollUp";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ScrollLeftCommand,
			ScrollDownCommand,
			ScrollRightCommand,
			ScrollUpCommand,
		};

		public AppiumScrollActions(AppiumApp appiumApp)
		{
			_appiumApp = appiumApp;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				ScrollLeftCommand => ScrollLeft(parameters),
				ScrollDownCommand => ScrollDown(parameters),
				ScrollRightCommand => ScrollRight(parameters),
				ScrollUpCommand => ScrollUp(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse ScrollLeft(IDictionary<string, object> parameters)
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

		CommandResponse ScrollDown(IDictionary<string, object> parameters)
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

		CommandResponse ScrollRight(IDictionary<string, object> parameters)
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

		CommandResponse ScrollUp(IDictionary<string, object> parameters)
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

            int startX = (int)(position.X + (size.Width * 0.05));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * swipePercentage));
			int endY = startY;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var scrollSequence = new ActionSequence(touchDevice, 0);
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
			scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(ScrollTouchDownTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(strategy != ScrollStrategy.Programmatically ? swipeSpeed : ProgrammaticallyScrollTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([scrollSequence]);
		}

		static void ScrollToDown(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int startX = position.X + size.Width / 2;
			int startY = (int)(position.Y + (size.Height * swipePercentage));

			int endX = startX;
			int endY = (int)(position.Y + (size.Height * 0.05));

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var scrollSequence = new ActionSequence(touchDevice, 0);
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
			scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(ScrollTouchDownTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(strategy != ScrollStrategy.Programmatically ? swipeSpeed : ProgrammaticallyScrollTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([scrollSequence]);
		}

		static void ScrollToRight(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int startX = (int)(position.X + (size.Width * swipePercentage));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * 0.05));
			int endY = startY;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var scrollSequence = new ActionSequence(touchDevice, 0);
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
			scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(ScrollTouchDownTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(strategy != ScrollStrategy.Programmatically ? swipeSpeed : ProgrammaticallyScrollTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([scrollSequence]);
		}

		static void ScrollToUp(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
            var position = element is not null ? element.Location : System.Drawing.Point.Empty;
            var size = element is not null ? element.Size : driver.Manage().Window.Size;

            int startX = position.X + size.Width / 2;
			int startY = (int)(position.Y + (size.Height * 0.05));

			int endX = startX;
			int endY = (int)(position.Y + (size.Height * swipePercentage));

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var scrollSequence = new ActionSequence(touchDevice, 0);
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
			scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(ScrollTouchDownTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(strategy != ScrollStrategy.Programmatically ? swipeSpeed : ProgrammaticallyScrollTime)));
			scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([scrollSequence]);
		}
	}
}