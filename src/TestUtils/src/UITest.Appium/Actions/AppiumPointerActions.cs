using System.Diagnostics;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumPointerActions : ICommandExecutionGroup
	{
		const string ClickCommand = "click";
		const string DoubleClickCommand = "doubleClick";
		const string DragAndDropCommand = "dragAndDrop";
		const string ScrollToCommand = "scrollTo";
		const string TapCoordinatesCommand = "tapCoordinates";
		const string DragCoordinatesCommand = "dragCoordinates";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ClickCommand,
			DoubleClickCommand,
			DragAndDropCommand,
			ScrollToCommand,
			TapCoordinatesCommand,
			DragCoordinatesCommand
		};

		public AppiumPointerActions(AppiumApp appiumApp)
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
				ClickCommand => Click(parameters),
				DoubleClickCommand => DoubleClick(parameters),
				DragAndDropCommand => DragAndDrop(parameters),
				ScrollToCommand => ScrollTo(parameters),
				TapCoordinatesCommand => TapCoordinates(parameters),
				DragCoordinatesCommand => DragCoordinates(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse Click(IDictionary<string, object> parameters)
		{
			if (parameters.TryGetValue("element", out var val))
			{
				AppiumElement? element = GetAppiumElement(parameters["element"]);
				if (element == null)
				{
					return CommandResponse.FailedEmptyResponse;
				}
				return ClickElement(element);
			}
			else if (parameters.TryGetValue("x", out var x) &&
					 parameters.TryGetValue("y", out var y))
			{
				return ClickCoordinates(Convert.ToSingle(x), Convert.ToSingle(y));
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ClickElement(AppiumElement element)
		{
			try
			{
				element.Click();
				return CommandResponse.SuccessEmptyResponse;
			}
			catch (InvalidOperationException)
			{
				return ProcessException();
			}
			catch (WebDriverException)
			{
				return ProcessException();
			}

			CommandResponse ProcessException()
			{
				// Some elements aren't "clickable" from an automation perspective (e.g., Frame renders as a Border
				// with content in it; if the content is just a TextBlock, we'll end up here)

				// All is not lost; we can figure out the location of the element in in the application window and Tap in that spot
				PointF p = ElementToClickablePoint(element);
				ClickCoordinates(p.X, p.Y);
				return CommandResponse.SuccessEmptyResponse;
			}
		}

		CommandResponse ClickCoordinates(float x, float y)
		{
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var sequence = new ActionSequence(touchDevice, 0);
			sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)x, (int)y, TimeSpan.FromMilliseconds(5)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_appiumApp.Driver.PerformActions(new List<ActionSequence> { sequence });

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse DoubleClick(IDictionary<string, object> parameters)
		{
			var element = GetAppiumElement(parameters["element"]);

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var sequence = new ActionSequence(touchDevice, 0);
			sequence.AddAction(touchDevice.CreatePointerMove(element, 0, 0, TimeSpan.FromMilliseconds(5)));

			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(250)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_appiumApp.Driver.PerformActions(new List<ActionSequence> { sequence });

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse DragAndDrop(IDictionary<string, object> actionParams)
		{
			AppiumElement? sourceAppiumElement = GetAppiumElement(actionParams["sourceElement"]);
			AppiumElement? destinationAppiumElement = GetAppiumElement(actionParams["destinationElement"]);

			if (sourceAppiumElement != null && destinationAppiumElement != null)
			{
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var sequence = new ActionSequence(touchDevice, 0);
				sequence.AddAction(touchDevice.CreatePointerMove(sourceAppiumElement, 0, 0, TimeSpan.FromMilliseconds(5)));
				sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromSeconds(1))); // Have to pause so the device doesn't think we are scrolling
				sequence.AddAction(touchDevice.CreatePointerMove(destinationAppiumElement, 0, 0, TimeSpan.FromSeconds(1)));
				sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_appiumApp.Driver.PerformActions(new List<ActionSequence> { sequence });

				return CommandResponse.SuccessEmptyResponse;
			}
			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ScrollTo(IDictionary<string, object> parameters)
		{
			// This method will keep scrolling in the specified direction until it finds an element 
			// which matches the query, or until it times out.

			bool down = !parameters.TryGetValue("down", out object? val) || (bool)val;
			string toElementId = (string)parameters["elementId"];

			// First we need to determine the area within which we'll make our scroll gestures
			var window = _appiumApp?.Driver.Manage().Window
				?? throw new InvalidOperationException("Element to scroll within not specified, and no Window available. Cannot scroll.");
			Size scrollAreaSize = window.Size;

			var x = scrollAreaSize.Width / 2;
			var windowHeight = scrollAreaSize.Height;
			var topEdgeOfScrollAction = windowHeight * 0.1;
			var bottomEdgeOfScrollAction = windowHeight * 0.5;
			var startY = down ? bottomEdgeOfScrollAction : topEdgeOfScrollAction;
			var endY = down ? topEdgeOfScrollAction : bottomEdgeOfScrollAction;

			var timeout = TimeSpan.FromSeconds(15);
			DateTime start = DateTime.Now;

			while (true)
			{
				try
				{
					IUIElement found = _appiumApp.FindElement(toElementId);

					if (found != null)
					{
						// Success!
						return CommandResponse.SuccessEmptyResponse;
					}
				}
				catch (TimeoutException)
				{
					// Haven't found it yet, keep scrolling
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Ticks}");
					throw new TimeoutException($"Timed out scrolling to {toElementId}");
				}

				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var scrollSequence = new ActionSequence(touchDevice, 0);
				scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, (int)startY, TimeSpan.Zero));
				scrollSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				scrollSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, (int)endY, TimeSpan.FromMilliseconds(500)));
				scrollSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_appiumApp.Driver.PerformActions([scrollSequence]);
			}
		}

		CommandResponse TapCoordinates(IDictionary<string, object> parameters)
		{
			if (parameters.TryGetValue("x", out var x) &&
				parameters.TryGetValue("y", out var y))
			{
				return ClickCoordinates(Convert.ToSingle(x), Convert.ToSingle(y));
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse DragCoordinates(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("fromX", out var fromX);
			parameters.TryGetValue("fromY", out var fromY);

			parameters.TryGetValue("toX", out var toX);
			parameters.TryGetValue("toY", out var toY);

			if (fromX is not null && fromY is not null && toX is not null && toY is not null)
			{
				DragCoordinates(
					_appiumApp.Driver,
					Convert.ToDouble(fromX),
					Convert.ToDouble(fromY),
					Convert.ToDouble(toX),
					Convert.ToDouble(toY));

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		static AppiumElement? GetAppiumElement(object element)
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

		static PointF ElementToClickablePoint(AppiumElement element)
		{
			string cpString = element.GetAttribute("ClickablePoint");
			string[] parts = cpString.Split(',');
			float x = float.Parse(parts[0]);
			float y = float.Parse(parts[1]);

			return new PointF(x, y);
		}

		static void DragCoordinates(AppiumDriver driver, double fromX, double fromY, double toX, double toY)
		{
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var dragSequence = new ActionSequence(touchDevice, 0);
			dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)fromX, (int)fromY, TimeSpan.Zero));
			dragSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)toX, (int)toY, TimeSpan.FromMilliseconds(250)));
			dragSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			driver.PerformActions([dragSequence]);
		}
	}
}