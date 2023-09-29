namespace UITest.Core
{
    public interface IUIElement : IUIElementQueryable
    {
        ICommandExecution Command { get; }
    }
}
