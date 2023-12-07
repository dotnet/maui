using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumSliderActions : ICommandExecutionGroup
	{
		const string SetSliderValueCommand = "setSliderValue";

		protected readonly AppiumApp _app;

		public AppiumSliderActions(AppiumApp app)
		{
			_app = app;
		}

		readonly List<string> _commands = new()
		{
			SetSliderValueCommand,
		};

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				SetSliderValueCommand => SetSliderValue(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse SetSliderValue(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var element);
			var slider = GetAppiumElement(element);

			if (slider is not null)
			{
				var minimum = (double)parameters["minimum"];
				var maximum = (double)parameters["maximum"];
				var value = (double)parameters["value"];

				SetSliderValue(_app.Driver, slider, value, minimum, maximum);

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
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

		static void SetSliderValue(AppiumDriver driver, AppiumElement? element, double value, double minimum = 0d, double maximum = 1d)
		{
			var position = element is not null ? element.Location : System.Drawing.Point.Empty;
			var size = element is not null ? element.Size : System.Drawing.Size.Empty;

			int x = position.X;
			int y = position.Y;

			double moveToX = (x + size.Width) * value / maximum;

			TouchAction touchAction = new TouchAction(driver);
			touchAction
				.Press(x, y)
				.MoveTo(moveToX, y)
				.Release()
				.Perform();
		}
	}
}