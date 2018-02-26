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

		public string SharedName { get; private set; }

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

		public string Set(string key, string value) =>
			Set(key, value);
		public bool Set(string key, bool value) =>
			Set(key, value);
		public int Set(string key, int value) =>
			Set(key, value);
		public double Set(string key, double value) =>
			Set(key, value);
		public float Set(string key, float value) =>
			Set(key, value);
		public long Set(string key, long value) =>
			Set(key, value);
	}
}
