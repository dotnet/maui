using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumClipboardActions : ICommandExecutionGroup
	{
		const string GetClipboardTextCommand = "getClipboardText";
		const string SetClipboardTextCommand = "setClipboardText";

		protected readonly AppiumApp _app;

		public AppiumClipboardActions(AppiumApp app)
		{
			_app = app;
		}

		readonly List<string> _commands = new()
		{
			GetClipboardTextCommand,
			SetClipboardTextCommand,
		};

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				GetClipboardTextCommand => GetClipboardText(parameters),
				SetClipboardTextCommand => SetClipboardText(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse GetClipboardText(IDictionary<string, object> parameters)
		{
			string clipboardText = string.Empty;

			if (_app.Driver is AndroidDriver androidDriver)
				clipboardText = androidDriver.GetClipboardText();

			if (_app.Driver is IOSDriver iOSDriver)
				clipboardText = iOSDriver.GetClipboardText();

			if (!string.IsNullOrEmpty(clipboardText))
				return new CommandResponse(clipboardText, CommandResponseResult.Success);

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse SetClipboardText(IDictionary<string, object> parameters)
		{
			var content = (string)parameters["content"];
			var label = (string?)parameters["label"];

			if (!string.IsNullOrEmpty(content))
			{
				if (_app.Driver is AndroidDriver androidDriver)
					androidDriver.SetClipboardText(content, label ?? string.Empty);

				if (_app.Driver is IOSDriver iOSDriver)
					iOSDriver.SetClipboardText(content, label);
			}

			return CommandResponse.FailedEmptyResponse;
		}
	}
}