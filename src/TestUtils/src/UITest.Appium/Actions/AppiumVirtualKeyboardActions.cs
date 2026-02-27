using UITest.Core;

namespace UITest.Appium
{
	public class AppiumVirtualKeyboardActions : ICommandExecutionGroup
	{
		const string IsKeyboardShownCommand = "isKeyboardShown";
		const string HideKeyboardCommand = "dismissKeyboard";
		const string PressEnterCommand = "pressEnter";
		const string SendTabKeyCommand = "sendTabKey";
		const string PressVolumeDownCommand = "pressVolumeDown";
		const string PressVolumeUpCommand = "pressVolumeUp";

		protected readonly AppiumApp _app;
		readonly List<string> _commands = new()
		{
			IsKeyboardShownCommand,
			HideKeyboardCommand,
			PressEnterCommand,
			SendTabKeyCommand,
			PressVolumeDownCommand,
			PressVolumeUpCommand,
		};

		public AppiumVirtualKeyboardActions(AppiumApp app)
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
				IsKeyboardShownCommand => IsKeyboardShown(parameters),
				HideKeyboardCommand => DismissKeyboard(parameters),
				PressEnterCommand => PressEnter(parameters),
				SendTabKeyCommand => SendTabKey(parameters),
				PressVolumeDownCommand => PressVolumeDown(parameters),
				PressVolumeUpCommand => PressVolumeUp(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse IsKeyboardShown(IDictionary<string, object> parameters)
		{
			return new CommandResponse(_app.Driver.IsKeyboardShown(), CommandResponseResult.Success);
		}

		protected virtual CommandResponse DismissKeyboard(IDictionary<string, object> parameters)
		{
			return CommandResponse.SuccessEmptyResponse;
		}

		protected virtual CommandResponse PressEnter(IDictionary<string, object> parameters)
		{
			return CommandResponse.SuccessEmptyResponse;
		}

		protected virtual CommandResponse SendTabKey(IDictionary<string, object> parameters)
		{
			return CommandResponse.SuccessEmptyResponse;
		}

		protected virtual CommandResponse PressVolumeDown(IDictionary<string, object> parameters)
		{
			return CommandResponse.SuccessEmptyResponse;
		}

		protected virtual CommandResponse PressVolumeUp(IDictionary<string, object> parameters)
		{
			return CommandResponse.SuccessEmptyResponse;
		}
	}
}