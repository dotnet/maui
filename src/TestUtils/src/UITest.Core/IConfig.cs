namespace UITest.Core
{
    public interface IConfig
    {
        void SetProperty(string name, object? val);
        T? GetProperty<T>(string name);
    }
}
