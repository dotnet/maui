using OpenQA.Selenium.Appium;
using UITest.Core;
using static System.Net.Mime.MediaTypeNames;

namespace UITest.Appium
{
    public class AppiumTextActions : ICommandExecutionGroup
    {
        const string SendKeysCommand = "sendKeys";
        const string ClearCommand = "clear";
        const string GetTextCommand = "getText";

        readonly List<string> _commands = new()
        {
            SendKeysCommand,
            ClearCommand,
            GetTextCommand,
        };

        public bool IsCommandSupported(string commandName)
        {
            return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
        }

        public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
        {
            return commandName switch
            {
                SendKeysCommand => SendKeys(parameters),
                ClearCommand => Clear(parameters),
                GetTextCommand => GetText(parameters),
                _ => CommandResponse.FailedEmptyResponse,
            };
        }

        CommandResponse GetText(IDictionary<string, object> parameters)
        {
            var element = GetAppiumElement(parameters["element"]);
            return element != null 
                ? new CommandResponse(element.Text, CommandResponseResult.Success) 
                : CommandResponse.FailedEmptyResponse;
        }

        CommandResponse SendKeys(IDictionary<string, object> parameters)
        {
            var text = (string)parameters["text"];
            var element = GetAppiumElement(parameters["element"]);
            element?.SendKeys(text);
            return CommandResponse.SuccessEmptyResponse;
        }

        CommandResponse Clear(IDictionary<string, object> parameters)
        {
            var element = GetAppiumElement(parameters["element"]);
            element?.Clear();
            return CommandResponse.SuccessEmptyResponse;
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