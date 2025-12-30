using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
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
		static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

		const int ScrollTouchDownTime = 100;
		const int ProgrammaticallyScrollTime = 0;

		const string ScrollLeftCommand = "scrollLeft";
		const string ScrollDownCommand = "scrollDown";
		const string ScrollDownToCommand = "scrollDownTo";
		const string ScrollRightCommand = "scrollRight";
		const string ScrollUpCommand = "scrollUp";
		const string ScrollUpToCommand = "scrollUpTo";

		readonly AppiumApp _appiumApp;

		readonly protected List<string> _commands = new()
		{
			ScrollLeftCommand,
			ScrollDownCommand,
			ScrollDownToCommand,
			ScrollRightCommand,
			ScrollUpCommand,
			ScrollUpToCommand,
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
				ScrollDownToCommand => ScrollDownTo(parameters),
				ScrollRightCommand => ScrollRight(parameters),
				ScrollUpCommand => ScrollUp(parameters),
				ScrollUpToCommand => ScrollUpTo(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		internal CommandResponse ScrollLeft(IDictionary<string, object> parameters)
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

		CommandResponse ScrollDownTo(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			string marked = (string)parameters["marked"];
			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			ScrollToDownTo(_appiumApp.Driver, marked, element, strategy, swipePercentage, swipeSpeed, withInertia);

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

		CommandResponse ScrollUpTo(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			string marked = (string)parameters["marked"];
			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			double swipePercentage = (double)parameters["swipePercentage"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			bool withInertia = (bool)parameters["withInertia"];

			ScrollToUpTo(_appiumApp.Driver, marked, element, strategy, swipePercentage, swipeSpeed, withInertia);

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

		void ScrollToLeft(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * 0.05));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * swipePercentage));
			int endY = startY;
			PerformActions(driver, startX, startY, endX, endY, strategy, swipeSpeed, element?.Id);
		}

		void ScrollToDown(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = position.X + size.Width / 2;
			int startY = (int)(position.Y + (size.Height * swipePercentage));

			int endX = startX;
			int endY = (int)(position.Y + (size.Height * 0.05));

			PerformActions(driver, startX, startY, endX, endY, strategy, swipeSpeed, element?.Id);
		}

		AppiumElement? ScrollToDownTo(AppiumDriver driver, string marked, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var timeout = DefaultTimeout;
			var retryFrequency = TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;

			AppiumElement? result = ScrollDownUntilPresent(driver, marked, element, strategy, swipePercentage, swipeSpeed, withInertia);

			while (result is not null)
			{
				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Ticks)
				{
					throw new TimeoutException("Timed out on scroll to.");
				}

				Task.Delay(retryFrequency.Milliseconds).Wait();
				result = ScrollDownUntilPresent(driver, marked, element, strategy, swipePercentage, swipeSpeed, withInertia);
			}

			return result;
		}

		AppiumElement? ScrollDownUntilPresent(AppiumDriver driver, string marked, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			ScrollToDown(driver, element, strategy, swipePercentage, swipeSpeed, withInertia);

			var result = driver.FindElement(By.Id(marked));

			if (result is null)
			{
				// Android (text), iOS (label), Windows (Name)
				result = driver.FindElement(By.XPath("//*[@text='" + marked + "' or @label='" + marked + "' or @Name='" + marked + "']"));
			}

			return result;
		}

		void ScrollToRight(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int startX = (int)(position.X + (size.Width * swipePercentage));
			int startY = position.Y + size.Height / 2;

			int endX = (int)(position.X + (size.Width * 0.05));
			int endY = startY;
			PerformActions(driver, startX, startY, endX, endY, strategy, swipeSpeed, element?.Id);
		}

		void ScrollToUp(AppiumDriver driver, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;
			int startX = position.X + size.Width / 2;
			int startY = (int)(position.Y + (size.Height * 0.05));

			int endX = startX;
			int endY = (int)(position.Y + (size.Height * swipePercentage));
			PerformActions(driver, startX, startY, endX, endY, strategy, swipeSpeed, element?.Id);
		}

		AppiumElement? ScrollToUpTo(AppiumDriver driver, string target, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			var timeout = DefaultTimeout;
			var retryFrequency = TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;

			AppiumElement? result = ScrollUpUntilPresent(driver, target, element, strategy, swipePercentage, swipeSpeed, withInertia);

			while (result is null)
			{
				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Ticks)
				{
					throw new TimeoutException("Timed out on scroll to.");
				}

				Task.Delay(retryFrequency.Milliseconds).Wait();
				result = ScrollUpUntilPresent(driver, target, element, strategy, swipePercentage, swipeSpeed, withInertia);
			}

			return result;
		}

		AppiumElement? ScrollUpUntilPresent(AppiumDriver driver, string marked, AppiumElement element, ScrollStrategy strategy, double swipePercentage, int swipeSpeed, bool withInertia = true)
		{
			ScrollToUp(driver, element, strategy, swipePercentage, swipeSpeed, withInertia);

			var result = driver.FindElement(By.Id(marked));

			if (result is null)
			{
				// Android (text), iOS (label), Windows (Name)
				result = driver.FindElement(By.XPath("//*[@text='" + marked + "' or @label='" + marked + "' or @Name='" + marked + "']"));
			}

			return result;
		}

		virtual protected void PerformActions(
			AppiumDriver driver,
			int startX,
			int startY,
			int endX,
			int endY,
			ScrollStrategy strategy,
			int swipeSpeed,
			string? elementId)
		{

			var pointerKind = PointerKind.Touch;
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(pointerKind);
			var scrollSequence = new ActionSequence(touchDevice, 0);
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.FromMilliseconds(2)));
			scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			scrollSequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(Math.Max(ScrollTouchDownTime, 2))));

			var moveDuration = TimeSpan.FromMilliseconds(Math.Max(2, strategy != ScrollStrategy.Programmatically ? swipeSpeed : ProgrammaticallyScrollTime));
			scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, moveDuration));

			scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([scrollSequence]);
		}
	}
}