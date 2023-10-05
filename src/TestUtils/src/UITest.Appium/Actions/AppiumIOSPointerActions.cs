using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumIOSPointerActions : ICommandExecutionGroup
	{
		const string DoubleClickCommand = "doubleClick";
		const string DragAndDropCommand = "dragAndDrop";

		readonly List<string> _commands = new()
		{
			DoubleClickCommand,
			DragAndDropCommand
		};
		readonly AppiumApp _appiumApp;

		public AppiumIOSPointerActions(AppiumApp appiumApp)
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
				DoubleClickCommand => DoubleClick(parameters),
				DragAndDropCommand => DragAndDrop(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse DoubleClick(IDictionary<string, object> parameters)
		{
			var element = GetAppiumElement(parameters["element"]);

			if (element != null)
			{
				_appiumApp.Driver.ExecuteScript("mobile: doubleTap", new Dictionary<string, object>
				{
					{ "elementId", element.Id },
				});
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse DragAndDrop(IDictionary<string, object> actionParams)
		{
			AppiumElement? sourceAppiumElement = GetAppiumElement(actionParams["sourceElement"]);
			AppiumElement? destinationAppiumElement = GetAppiumElement(actionParams["destinationElement"]);

			if (sourceAppiumElement != null && destinationAppiumElement != null)
			{
				var sourceCenterX = sourceAppiumElement.Location.X + (sourceAppiumElement.Size.Width / 2);
				var sourceCenterY = sourceAppiumElement.Location.Y + (sourceAppiumElement.Size.Height / 2);
				var destCenterX = destinationAppiumElement.Location.X + (destinationAppiumElement.Size.Width / 2);
				var destCenterY = destinationAppiumElement.Location.Y + (destinationAppiumElement.Size.Height / 2);

				// iOS doesn't seem to work with the action API, so we are using script calls
				_appiumApp.Driver.ExecuteScript("mobile: dragFromToWithVelocity", new Dictionary<string, object>
				{
					{ "pressDuration", 1 }, // Length of time to hold after click before start dragging
					{ "holdDuration", .1 }, // Length of time to hold before releasing
					{ "velocity", CalculateDurationForSwipe(sourceCenterX, sourceCenterY, destCenterX,destCenterY, 500) }, // How fast to drag
					// from/to are absolute screen coordinates unless 'element' is specified then everything will be relative
					{ "fromX", sourceCenterX},
					{ "fromY", sourceCenterY },
					{ "toX", destCenterX },
					{ "toY", destCenterY }
				});
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

		static int CalculateDurationForSwipe(int startX, int startY, int endX, int endY, int pixelsPerSecond)
		{
			var distance = Math.Sqrt(Math.Pow(startX - endX, 2) + Math.Pow(startY - endY, 2));

			return (int)(distance / (pixelsPerSecond / 1000.0));
		}
	}
}