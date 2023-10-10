using UITest.Core;

namespace UITest.Appium
{
    public class AppiumCommandExecutor : ICommandExecution
    {
        readonly Stack<ICommandExecutionGroup> _commands = new();

        public void AddCommandGroup(ICommandExecutionGroup commandExecuteGroup)
        {
            _commands.Push(commandExecuteGroup);
        }

        public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
        {
            foreach (var command in _commands)
            {
                if (command.IsCommandSupported(commandName))
                {
                    return command.Execute(commandName, parameters);
                }
            }

            return CommandResponse.FailedEmptyResponse;
        }
    }
}