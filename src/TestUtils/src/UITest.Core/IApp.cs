namespace UITest.Core
{
    public interface IApp : IDisposable
    {
        IConfig Config { get; }
        IUIElementQueryable Query { get; }
        ApplicationState AppState { get; }
        IUIElement FindElement(string id);
        IUIElement FindElement(IQuery query);
        IReadOnlyCollection<IUIElement> FindElements(string id);
        IReadOnlyCollection<IUIElement> FindElements(IQuery query);
        ICommandExecution CommandExecutor { get; }
        void Click(float x, float y);
        FileInfo Screenshot(string fileName);
        byte[] Screenshot();
        string ElementTree { get; }
    }
}
