using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystTouchActions : ICommandExecutionGroup
	{
		const string TapCoordinatesCommand = "tapCoordinates";
		const string DoubleTapCommand = "doubleTap";
		const string DragAndDropCommand = "dragAndDrop";

		readonly List<string> _commands = new()
		{
			TapCoordinatesCommand,
			DoubleTapCommand,
			DragAndDropCommand,
		};
		readonly AppiumApp _appiumApp;

		public AppiumCatalystTouchActions(AppiumApp appiumApp)
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
				TapCoordinatesCommand => TapCoordinates(parameters),
				DoubleTapCommand => DoubleTap(parameters),
				DragAndDropCommand => DragAndDrop(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse TapCoordinates(IDictionary<string, object> parameters)
		{
			if (parameters.TryGetValue("x", out var x) &&
				parameters.TryGetValue("y", out var y))
			{
				_appiumApp.Driver.ExecuteScript("macos: click", new Dictionary<string, object>
				{
					{ "x", Convert.ToSingle(x) },
					{ "y", Convert.ToSingle(y) },
				});

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse DoubleTap(IDictionary<string, object> parameters)
		{
			var element = GetAppiumElement(parameters["element"]);

			if (element != null)
			{
				_appiumApp.Driver.ExecuteScript("macos: doubleClick", new Dictionary<string, object>
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
				_appiumApp.Driver.ExecuteScript("macos: clickAndDragAndHold", new Dictionary<string, object>
				{
					{ "holdDuration", .1 }, // Length of time to hold before releasing
                    { "duration", 1 }, // Length of time to hold after click before start dragging
                    { "velocity", 2500 }, // How fast to drag
                    { "sourceElementId", sourceAppiumElement.Id },
					{ "destinationElementId", destinationAppiumElement.Id },
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
	}
}