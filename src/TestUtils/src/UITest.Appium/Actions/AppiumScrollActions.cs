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
		const string ScrollToBottomCommand = "scrollToBottom";

		readonly AppiumApp _appiumApp;

		readonly protected List<string> _commands = new()
		{
			ScrollLeftCommand,
			ScrollDownCommand,
			ScrollDownToCommand,
			ScrollRightCommand,
			ScrollUpCommand,
			ScrollUpToCommand,
			ScrollToBottomCommand,
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
				ScrollToBottomCommand => ScrollToBottom(parameters),
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

		CommandResponse ScrollToBottom(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var value);
			var element = GetAppiumElement(value);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			string bottomMarked = (string)parameters["bottomMarked"];
			ScrollStrategy strategy = (ScrollStrategy)parameters["strategy"];
			int swipeSpeed = (int)parameters["swipeSpeed"];
			int maxScrolls = (int)parameters["maxScrolls"];

			PerformScrollToBottom(_appiumApp.Driver, element, bottomMarked, strategy, swipeSpeed, maxScrolls);

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

		void PerformScrollToBottom(AppiumDriver driver, AppiumElement scrollElement, string bottomMarked, ScrollStrategy strategy, int swipeSpeed, int maxScrolls)
		{
			// Get scroll coordinates once from the scroll element
			var position = scrollElement.Location;
			var size = scrollElement.Size;
			
			int startX = position.X + size.Width / 2;
			int startY = position.Y + size.Height - 10;
			int endX = startX;
			int endY = position.Y + 10;

			// Check if bottom element is already visible AND already at bottom (can't scroll further)
			bool foundBottom = false;
			bool alreadyAtBottom = false;
			try
			{
				var element = driver.FindElement(By.XPath("//*[@text='" + bottomMarked + "' or @label='" + bottomMarked + "' or @Name='" + bottomMarked + "']"));
				if (element != null)
				{
					foundBottom = true;
					
					// Check if we can scroll further by attempting a scroll and checking if position changes
					int yPositionBefore = element.Location.Y;
					
					// Perform a test scroll
					OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
					var swipeSequence = new ActionSequence(touchDevice, 0);
					swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
					swipeSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
					swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(swipeSpeed)));
					swipeSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
					driver.PerformActions(new List<ActionSequence> { swipeSequence });
					
					System.Threading.Thread.Sleep(50);
					
					// Check if position changed
					var elementAfter = driver.FindElement(By.XPath("//*[@text='" + bottomMarked + "' or @label='" + bottomMarked + "' or @Name='" + bottomMarked + "']"));
					int yPositionAfter = elementAfter.Location.Y;
					
					if (Math.Abs(yPositionAfter - yPositionBefore) <= 5)
					{
						// Position didn't change, we're already at the bottom
						alreadyAtBottom = true;
					}
					else
					{
						// Position changed, need to continue scrolling
						foundBottom = false;
					}
				}
			}
			catch
			{
				// Element not visible yet, need to scroll
			}

			// If already at bottom and can't scroll further, exit early
			if (alreadyAtBottom)
			{
				// Already at bottom, no more scrolling needed
				return;
			}

			for (int i = 0; i < maxScrolls && !foundBottom; i++)
			{
				// Use direct Appium Actions API for fast, precise scrolling
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var swipeSequence = new ActionSequence(touchDevice, 0);
				swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
				swipeSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(swipeSpeed)));
				swipeSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				driver.PerformActions(new List<ActionSequence> { swipeSequence });

				// Brief delay to let UI settle after scroll
				System.Threading.Thread.Sleep(50);

				// Check if we reached the bottom by looking for the bottom element
				try
				{
					var element = driver.FindElement(By.XPath("//*[@text='" + bottomMarked + "' or @label='" + bottomMarked + "' or @Name='" + bottomMarked + "']"));
					if (element != null)
					{
						foundBottom = true;
					}
				}
				catch
				{
					// Element not found yet, continue scrolling
				}
			}

			if (!foundBottom)
			{
				throw new InvalidOperationException($"Could not find bottom element '{bottomMarked}' after {maxScrolls} scroll attempts");
			}

			// Perform final validation scroll
			PerformFinalValidationScroll(driver, startX, startY, endX, endY, bottomMarked, swipeSpeed);
		}

		void PerformFinalValidationScroll(AppiumDriver driver, int startX, int startY, int endX, int endY, string bottomMarked, int swipeSpeed)
		{
			// Perform one final scroll to ensure we're fully at the bottom
			// and validate the position doesn't change (meaning we're truly at bottom)
			try
			{
				var elementBeforeFinalScroll = driver.FindElement(By.XPath("//*[@text='" + bottomMarked + "' or @label='" + bottomMarked + "' or @Name='" + bottomMarked + "']"));
				int yPositionBefore = elementBeforeFinalScroll.Location.Y;

				// Perform one more scroll
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var swipeSequence = new ActionSequence(touchDevice, 0);
				swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
				swipeSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				swipeSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(swipeSpeed)));
				swipeSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				driver.PerformActions(new List<ActionSequence> { swipeSequence });

				// Verify position hasn't changed (we're at the bottom)
				var elementAfterFinalScroll = driver.FindElement(By.XPath("//*[@text='" + bottomMarked + "' or @label='" + bottomMarked + "' or @Name='" + bottomMarked + "']"));
				int yPositionAfter = elementAfterFinalScroll.Location.Y;

				// Position should be the same or very close (within a few pixels for rendering differences)
				if (Math.Abs(yPositionAfter - yPositionBefore) > 5)
				{
					throw new InvalidOperationException($"Final scroll validation failed. Element moved from Y={yPositionBefore} to Y={yPositionAfter}. May not be at true bottom.");
				}
			}
			catch (Exception ex) when (!(ex is InvalidOperationException))
			{
				// If we can't validate, that's okay - we found the element which is the main goal
			}
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