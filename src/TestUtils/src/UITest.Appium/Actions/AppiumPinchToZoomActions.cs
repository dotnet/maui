using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Core;

namespace UITest.Appium
{
	class AppiumPinchToZoomActions : ICommandExecutionGroup
	{
		const string PinchToZoomInCommand = "pinchToZoomIn";
		const string PinchToZoomInCoordinatesCommand = "pinchToZoomInCoordinates";
		const string PinchToZoomOutCommand = "pinchToZoomOut";
		const string PinchToZoomOutCoordinatesCommand = "pinchToZoomOutCoordinates";

		protected readonly AppiumApp _app;

		readonly List<string> _commands = new()
		{
			PinchToZoomInCommand,
			PinchToZoomInCoordinatesCommand,
			PinchToZoomOutCommand,
			PinchToZoomOutCoordinatesCommand,
		};

		public AppiumPinchToZoomActions(AppiumApp app)
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
				PinchToZoomInCommand => PinchToZoomIn(parameters),
				PinchToZoomInCoordinatesCommand => PinchToZoomInCoordinates(parameters),
				PinchToZoomOutCommand => PinchToZoomOut(parameters),
				PinchToZoomOutCoordinatesCommand => PinchToZoomOutCoordinates(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse PinchToZoomIn(IDictionary<string, object> parameters)
		{
			try
			{
				parameters.TryGetValue("element", out var value);
				var element = GetAppiumElement(value);

				TimeSpan duration = (TimeSpan)parameters["duration"];

				PinchToZoomIn(_app.Driver, element, duration);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse PinchToZoomInCoordinates(IDictionary<string, object> parameters)
		{
			try
			{
				int x = (int)parameters["x"];
				int y = (int)parameters["y"];
				TimeSpan duration = (TimeSpan)parameters["duration"];

				PinchToZoomInCoordinates(_app.Driver, x, y, duration);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse PinchToZoomOut(IDictionary<string, object> parameters)
		{
			try
			{
				parameters.TryGetValue("element", out var value);
				var element = GetAppiumElement(value);

				TimeSpan duration = (TimeSpan)parameters["duration"];

				PinchToZoomOut(_app.Driver, element, duration);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse PinchToZoomOutCoordinates(IDictionary<string, object> parameters)
		{
			try
			{
				int x = (int)parameters["x"];
				int y = (int)parameters["y"];
				TimeSpan duration = (TimeSpan)parameters["duration"];

				PinchToZoomOutCoordinates(_app.Driver, x, y, duration);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		static void PinchToZoomIn(AppiumDriver driver, AppiumElement? element, TimeSpan duration)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int touch1StartX = position.X + size.Width / 2;
			int touch1StartY = position.Y + size.Height / 2;
			int touch1EndX = position.X;
			int touch1EndY = position.Y;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch1 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch1Sequence = new ActionSequence(touch1, 0);
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1StartX, touch1StartY, TimeSpan.Zero));
			touch1Sequence.AddAction(touch1.CreatePointerDown(PointerButton.TouchContact));
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1EndX, touch1EndY, duration));
			touch1Sequence.AddAction(touch1.CreatePointerUp(PointerButton.TouchContact));

			int touch2StartX = position.X + size.Width / 2;
			int touch2StartY = position.Y + size.Height / 2;
			int touch2EndX = position.X + size.Width;
			int touch2EndY = position.Y + size.Height;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch2 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch2Sequence = new ActionSequence(touch2, 0);
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2StartX, touch2StartY, TimeSpan.Zero));
			touch2Sequence.AddAction(touch2.CreatePointerDown(PointerButton.TouchContact));
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2EndX, touch2EndY, duration));
			touch2Sequence.AddAction(touch2.CreatePointerUp(PointerButton.TouchContact));

			driver.PerformActions([touch1Sequence, touch2Sequence]);
		}

		static void PinchToZoomInCoordinates(AppiumDriver driver, int x, int y, TimeSpan duration)
		{
			const int distance = 50;

			int touch1StartX = x;
			int touch1StartY = y;
			int touch1EndX = x - distance;
			int touch1EndY = y - distance;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch1 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch1Sequence = new ActionSequence(touch1, 0);
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1StartX, touch1StartY, TimeSpan.Zero));
			touch1Sequence.AddAction(touch1.CreatePointerDown(PointerButton.TouchContact));
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1EndX, touch1EndY, duration));
			touch1Sequence.AddAction(touch1.CreatePointerUp(PointerButton.TouchContact));

			int touch2StartX = x;
			int touch2StartY = y;
			int touch2EndX = x + distance;
			int touch2EndY = y + distance;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch2 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch2Sequence = new ActionSequence(touch2, 0);
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2StartX, touch2StartY, TimeSpan.Zero));
			touch2Sequence.AddAction(touch2.CreatePointerDown(PointerButton.TouchContact));
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2EndX, touch2EndY, duration));
			touch2Sequence.AddAction(touch2.CreatePointerUp(PointerButton.TouchContact));

			driver.PerformActions([touch1Sequence, touch2Sequence]);
		}

		static void PinchToZoomOut(AppiumDriver driver, AppiumElement? element, TimeSpan duration)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : driver.Manage().Window.Size;

			int touch1StartX = position.X;
			int touch1StartY = position.Y;
			int touch1EndX = position.X + size.Width / 2;
			int touch1EndY = position.Y + size.Height / 2;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch1 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch1Sequence = new ActionSequence(touch1, 0);
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1StartX, touch1StartY, TimeSpan.Zero));
			touch1Sequence.AddAction(touch1.CreatePointerDown(PointerButton.TouchContact));
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1EndX, touch1EndY, duration));
			touch1Sequence.AddAction(touch1.CreatePointerUp(PointerButton.TouchContact));

			int touch2StartX = position.X + size.Width;
			int touch2StartY = position.Y + size.Height;
			int touch2EndX = position.X + size.Width / 2;
			int touch2EndY = position.Y + size.Height / 2;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch2 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch2Sequence = new ActionSequence(touch2, 0);
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2StartX, touch2StartY, TimeSpan.Zero));
			touch2Sequence.AddAction(touch2.CreatePointerDown(PointerButton.TouchContact));
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2EndX, touch2EndY, duration));
			touch2Sequence.AddAction(touch2.CreatePointerUp(PointerButton.TouchContact));

			driver.PerformActions([touch1Sequence, touch2Sequence]);
		}

		static void PinchToZoomOutCoordinates(AppiumDriver driver, int x, int y, TimeSpan duration)
		{
			const int distance = 50;

			int touch1StartX = x - distance;
			int touch1StartY = y - distance;
			int touch1EndX = x;
			int touch1EndY = y;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch1 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch1Sequence = new ActionSequence(touch1, 0);
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1StartX, touch1StartY, TimeSpan.Zero));
			touch1Sequence.AddAction(touch1.CreatePointerDown(PointerButton.TouchContact));
			touch1Sequence.AddAction(touch1.CreatePointerMove(CoordinateOrigin.Viewport, touch1EndX, touch1EndY, duration));
			touch1Sequence.AddAction(touch1.CreatePointerUp(PointerButton.TouchContact));

			int touch2StartX = x + distance;
			int touch2StartY = y + distance;
			int touch2EndX = x;
			int touch2EndY = y;

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touch2 = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			ActionSequence touch2Sequence = new ActionSequence(touch2, 0);
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2StartX, touch2StartY, TimeSpan.Zero));
			touch2Sequence.AddAction(touch2.CreatePointerDown(PointerButton.TouchContact));
			touch2Sequence.AddAction(touch2.CreatePointerMove(CoordinateOrigin.Viewport, touch2EndX, touch2EndY, duration));
			touch2Sequence.AddAction(touch2.CreatePointerUp(PointerButton.TouchContact));

			driver.PerformActions([touch1Sequence, touch2Sequence]);
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
	}
}
