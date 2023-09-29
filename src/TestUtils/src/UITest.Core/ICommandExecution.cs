namespace UITest.Core
{
    public interface ICommandExecution
    {
        CommandResponse Execute(string commandName, IDictionary<string, object> parameters);
    }
}