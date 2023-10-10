using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
    public class AppiumGeneralActions : ICommandExecutionGroup
    {
        const string GetAttributeCommand = "getAttribute";
        const string GetRectCommand = "getRect";

        readonly List<string> _commands = new()
        {
            GetAttributeCommand,
            GetRectCommand,
        };

        public bool IsCommandSupported(string commandName)
        {
            return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
        }

        public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
        {
            return commandName switch
            {
                GetAttributeCommand => GetAttribute(parameters),
                GetRectCommand => GetRect(parameters),
                _ => CommandResponse.FailedEmptyResponse,
            };
        }

        CommandResponse GetRect(IDictionary<string, object> parameters)
        {
            var element = parameters["element"];

            if (element is AppiumElement appiumElement)
            {
                return new CommandResponse(appiumElement.Rect, CommandResponseResult.Success);
            }
            else if (element is AppiumDriverElement driverElement)
            {
                return new CommandResponse(driverElement.AppiumElement.Rect, CommandResponseResult.Success);
            }
            return CommandResponse.FailedEmptyResponse;
        }

        CommandResponse GetAttribute(IDictionary<string, object> parameters)
        {
            var element = parameters["element"];
            var attributeName = (string)parameters["attributeName"];
            
            if (element is AppiumElement appiumElement)
            {
                return new CommandResponse(appiumElement.GetAttribute(attributeName), CommandResponseResult.Success);
            }
            else if (element is AppiumDriverElement driverElement)
            {
                return new CommandResponse(driverElement.AppiumElement.GetAttribute(attributeName), CommandResponseResult.Success);
            }
            return CommandResponse.FailedEmptyResponse;
        }
    }
}