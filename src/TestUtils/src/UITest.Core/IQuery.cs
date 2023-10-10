namespace UITest.Core
{
    public interface IQuery
    {
        IQuery ById(string id);
        IQuery ByName(string name);
        IQuery ByClass(string className);
        IQuery ByAccessibilityId(string id);
    }
}
