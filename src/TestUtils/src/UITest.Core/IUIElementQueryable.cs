namespace UITest.Core
{
    public interface IUIElementQueryable
    {
        IReadOnlyCollection<IUIElement> ById(string id);
        IReadOnlyCollection<IUIElement> ByName(string name);
        IReadOnlyCollection<IUIElement> ByClass(string className);
        IReadOnlyCollection<IUIElement> ByAccessibilityId(string name);
        IReadOnlyCollection<IUIElement> ByQuery(string query);
    }
}
