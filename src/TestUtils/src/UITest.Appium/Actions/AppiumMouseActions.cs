using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumMouseActions : ICommandExecutionGroup
	{
		const string ClickCommand = "click";
		const string ClickCoordinatesCommand = "clickCoordinates";
		const string DoubleClickCommand = "doubleClick";
		const string DoubleClickCoordinatesCommand = "doubleClickCoordinates";
		const string LongPressCommand = "longPress";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ClickCommand,
			ClickCoordinatesCommand,
			DoubleClickCommand,
			DoubleClickCoordinatesCommand,
			LongPressCommand,
		};

		public AppiumMouseActions(AppiumApp appiumApp)
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
				ClickCoordinatesCommand => ClickCoordinates(parameters),
				DoubleClickCommand => DoubleClick(parameters),
				DoubleClickCoordinatesCommand => DoubleClickCoordinates(parameters),
				LongPressCommand => LongPress(parameters),
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

		CommandResponse DoubleClickCoordinates(IDictionary<string, object> parameters)
		{
			if (parameters.TryGetValue("x", out var x) &&
				parameters.TryGetValue("y", out var y))
			{
				return DoubleClickCoordinates(Convert.ToSingle(x), Convert.ToSingle(y));
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ClickCoordinates(IDictionary<string, object> parameters)
		{
			if (parameters.TryGetValue("x", out var x) &&
				parameters.TryGetValue("y", out var y))
			{
				return ClickCoordinates(Convert.ToSingle(x), Convert.ToSingle(y));
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ClickElement(AppiumElement element)
		{
			string tagName = string.Empty;

			// If the click fails on catalyst we need to retrieve the element again
			if (_appiumApp.Driver is MacDriver)
				tagName = element.TagName;

			try
			{
				element.Click();
				return CommandResponse.SuccessEmptyResponse;
			}
			catch (InvalidOperationException ioe)
			{
				Console.WriteLine($"WebDriverException: {ioe}");
				return ProcessException();
			}
			catch (WebDriverException we)
			{
				Console.WriteLine($"WebDriverException: {we}");
				return ProcessException();
			}

			CommandResponse ProcessException()
			{
				// Appium elements will sometimes become stale
				// Which appears to happen if click fails, so, we retrieve it here
				if (!String.IsNullOrWhiteSpace(tagName))
					element = (AppiumElement)_appiumApp.FindElement(tagName);

				if (element is null)
				{
					return CommandResponse.FailedEmptyResponse;
				}

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
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Mouse);
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

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Mouse);
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

		CommandResponse DoubleClickCoordinates(float x, float y)
		{
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Mouse);
			var sequence = new ActionSequence(touchDevice, 0);
			sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)x, (int)y, TimeSpan.FromMilliseconds(5)));

			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(250)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_appiumApp.Driver.PerformActions(new List<ActionSequence> { sequence });

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse LongPress(IDictionary<string, object> parameters)
		{
			var element = GetAppiumElement(parameters["element"]);

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Mouse);
			var longPress = new ActionSequence(touchDevice, 0);

			longPress.AddAction(touchDevice.CreatePointerMove(element, 0, 0, TimeSpan.FromMilliseconds(0)));
			longPress.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			longPress.AddAction(touchDevice.CreatePointerMove(element, 0, 0, TimeSpan.FromMilliseconds(2000)));
			longPress.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_appiumApp.Driver.PerformActions(new List<ActionSequence> { longPress });

			return CommandResponse.SuccessEmptyResponse;
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
	}
}