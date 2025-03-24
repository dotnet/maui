using UITest.Core;

namespace UITest.Appium
{
	public class AppiumWindowsSpecificActions : ICommandExecutionGroup
	{
		const string MoreButton = "MoreButton";

		const string ToggleSecondaryToolbarItemsCommand = "toggleSecondaryToolbarItems";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ToggleSecondaryToolbarItemsCommand,
		};

		public AppiumWindowsSpecificActions(AppiumApp appiumApp)
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
				ToggleSecondaryToolbarItemsCommand => ToggleSecondaryToolbarItems(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse ToggleSecondaryToolbarItems(IDictionary<string, object> parameters)
		{
			try
			{
				_appiumApp.WaitForElement(MoreButton);
				_appiumApp.Tap(MoreButton);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}
	}
}