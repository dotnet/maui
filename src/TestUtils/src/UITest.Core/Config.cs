namespace UITest.Core
{
    public class Config : IConfig
    {
        readonly Dictionary<string, object?> _properties = new()
        {
            { "ReportFormat", "xml" },
            { "ReportDirectory", "reports" },
        };

        public void SetProperty(string name, object? val)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _properties[name] = val;
        }

        public T? GetProperty<T>(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && _properties.TryGetValue(name, out object? prop))
            {
                return (T?)prop;
            }

            return default;
        }
    }
}
