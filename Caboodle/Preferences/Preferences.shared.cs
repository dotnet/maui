namespace Microsoft.Caboodle
{
    public sealed partial class Preferences
    {
        public Preferences()
        {
        }

        public Preferences(string sharedName)
        {
            SharedName = sharedName;
        }

        public string SharedName { get; }

        public string Get(string key, string defaultValue) =>
            Get<string>(key, defaultValue);

        public bool Get(string key, bool defaultValue) =>
            Get<bool>(key, defaultValue);

        public int Get(string key, int defaultValue) =>
            Get<int>(key, defaultValue);

        public double Get(string key, double defaultValue) =>
            Get<double>(key, defaultValue);

        public float Get(string key, float defaultValue) =>
            Get<float>(key, defaultValue);

        public long Get(string key, long defaultValue) =>
            Get<long>(key, defaultValue);

        public void Set(string key, string value) =>
            Set<string>(key, value);

        public void Set(string key, bool value) =>
            Set<bool>(key, value);

        public void Set(string key, int value) =>
            Set<int>(key, value);

        public void Set(string key, double value) =>
            Set<double>(key, value);

        public void Set(string key, float value) =>
            Set<float>(key, value);

        public void Set(string key, long value) =>
            Set<long>(key, value);
    }
}
